BEGIN;

-- 1. Populate Lookup Tables
INSERT INTO roles (name) VALUES ('admin'), ('customer');
INSERT INTO account_types (name) VALUES ('checking'), ('savings'), ('credit_card');
INSERT INTO transaction_types (name) VALUES ('deposit'), ('withdrawal'), ('transfer_out'), ('transfer_in'), ('reversal');
INSERT INTO dispute_statuses (name) VALUES ('open'), ('under_review'), ('accepted'), ('rejected');

-- 2. Generate Users
WITH role_ids AS (
    SELECT 
        (SELECT role_id FROM roles WHERE name = 'admin') AS admin_id,
        (SELECT role_id FROM roles WHERE name = 'customer') AS customer_id
)
INSERT INTO users (google_id, username, email, role_id)
SELECT
    'google_id_' || substr(md5(random()::text), 0, 25),
    'user_' || n,
    'user_' || n || '@example.com',
    CASE WHEN random() < 0.95 THEN customer_id ELSE admin_id END
FROM generate_series(1, 1000) n
CROSS JOIN role_ids;

-- 3. Create Accounts with Initial Balances
INSERT INTO accounts (user_id, account_type_id, balance, created_at)
SELECT
    user_id,
    (SELECT account_type_id FROM account_types ORDER BY random() LIMIT 1),
    GREATEST(1000, (random() * 100000 + 1000)::int),
    NOW() - interval '1 year'
FROM users
CROSS JOIN generate_series(1, 3);

-- 4. Generate Transaction References
DO $$
DECLARE 
    total_transactions INT := (SELECT COUNT(*) FROM accounts) * 60 + 600;
BEGIN
  INSERT INTO transaction_references (transaction_reference_id)
  SELECT generate_series(1, total_transactions);
END $$;

-- 5. Generate Natural Transaction Flow with Balance Validation
DO $$
DECLARE
    account_rec RECORD;
    tx_rec RECORD;
    current_balance INT;
    tx_ref INT := 1;
	amount INT;
	transfer_time TIMESTAMP;
	receiver_id INT;
	receiver_balance INT;
BEGIN
    -- Create temporary table for randomized transaction timeline
    CREATE TEMP TABLE account_transactions AS
    SELECT 
        a.account_id,
        a.balance AS initial_balance,
        generate_series AS tx_num,
        CASE WHEN random() < 0.7 THEN 'deposit' ELSE 'withdrawal' END AS tx_type,
        a.created_at + random() * interval '365 days' AS tx_time
    FROM accounts a
    CROSS JOIN generate_series(1, 60);  -- Max 60 transactions per account

    -- Process accounts in random order
    FOR account_rec IN SELECT DISTINCT account_id, initial_balance FROM account_transactions LOOP
        current_balance := account_rec.initial_balance;
        
        -- Process transactions in chronological order for this account
        FOR tx_rec IN 
            SELECT *
            FROM account_transactions
            WHERE account_id = account_rec.account_id
            ORDER BY tx_time
        LOOP
            -- Calculate amount based on current balance
            CASE WHEN tx_rec.tx_type = 'deposit' THEN
                -- Generate deposit
                amount := (random() * 10000 + 100)::int;
                current_balance := current_balance + amount;
            ELSE
                -- Generate safe withdrawal
                amount := LEAST(
                    (random() * 5000 + 100)::int,
                    current_balance - 1000  -- Maintain $10 minimum
                );
                IF amount > 0 THEN
                    current_balance := current_balance - amount;
                END IF;
            END CASE;

            IF amount > 0 THEN
                -- Insert transaction with balance validation
                INSERT INTO transactions (
                    transaction_reference_id,
                    reference,
                    account_id,
                    amount,
                    transaction_type_id,
                    created_at,
                    balance_after_transaction
                ) VALUES (
                    tx_ref,
                    CASE 
                        WHEN tx_rec.tx_type = 'deposit' THEN 'Deposit ' || tx_rec.tx_num
                        ELSE 'Withdrawal ' || tx_rec.tx_num
                    END,
                    account_rec.account_id,
                    CASE WHEN tx_rec.tx_type = 'deposit' THEN amount ELSE -amount END,
                    (SELECT transaction_type_id FROM transaction_types WHERE name = tx_rec.tx_type),
                    tx_rec.tx_time,
                    current_balance
                );

                tx_ref := tx_ref + 1;
            END IF;
        END LOOP;

        -- Update final account balance
        UPDATE accounts 
        SET balance = current_balance
        WHERE account_id = account_rec.account_id;
    END LOOP;

    -- Generate P2P Transfers with chronological integration
    FOR i IN 1..300 LOOP
        -- Select random sender with sufficient balance
        SELECT a.account_id, a.balance INTO account_rec
        FROM accounts a 
        WHERE balance > 1000 
        ORDER BY random() 
        LIMIT 1;

        IF FOUND THEN
            -- Find suitable transfer time
            SELECT created_at INTO transfer_time 
            FROM transactions 
            WHERE account_id = account_rec.account_id
            ORDER BY created_at DESC
            LIMIT 1;

            amount := LEAST(
                (random() * 5000 + 100)::int,
                account_rec.balance - 1000
            );

            -- Select receiver
            SELECT account_id INTO receiver_id 
            FROM accounts 
            WHERE account_id != account_rec.account_id 
            ORDER BY random() 
            LIMIT 1;

            -- Get receiver's latest balance
            SELECT balance INTO receiver_balance 
            FROM accounts 
            WHERE account_id = receiver_id;

            -- Insert transfer pair with proper timing
            INSERT INTO transactions (
                transaction_reference_id,
                reference,
                account_id,
                amount,
                transaction_type_id,
                created_at,
                balance_after_transaction
            ) VALUES (
                tx_ref,
                'Transfer OUT',
                account_rec.account_id,
                -amount,
                (SELECT transaction_type_id FROM transaction_types WHERE name = 'transfer_out'),
                transfer_time + interval '1 minute',
                account_rec.balance - amount
            ),(
                tx_ref,
                'Transfer IN',
                receiver_id,
                amount,
                (SELECT transaction_type_id FROM transaction_types WHERE name = 'transfer_in'),
                transfer_time + interval '2 minute',
                receiver_balance + amount
            );

            -- Update balances
            UPDATE accounts SET balance = balance - amount WHERE account_id = account_rec.account_id;
            UPDATE accounts SET balance = balance + amount WHERE account_id = receiver_id;

            tx_ref := tx_ref + 1;
        END IF;
    END LOOP;

    DROP TABLE account_transactions;
END $$;

-- 6. Final Validation
DO $$
BEGIN
    -- Check current balances
    IF EXISTS(SELECT 1 FROM accounts WHERE balance < 1000) THEN
        RAISE EXCEPTION 'Account balance validation failed';
    END IF;

    -- Check transaction balances
    IF EXISTS(SELECT 1 FROM transactions WHERE balance_after_transaction < 0) THEN
        RAISE EXCEPTION 'Transaction balance validation failed';
    END IF;
END $$;

COMMIT;