CREATE OR REPLACE FUNCTION reverse_transactions(transaction_ids int[])
RETURNS boolean AS
$$
DECLARE
    original_tx RECORD;
    original_tx_id INT; -- ✅ declare the loop variable
    reversal_type_id INT;
    new_balance INT;
    current_account_balance INT;
    success BOOLEAN := TRUE;
BEGIN
    -- Get the transaction_type_id for 'reversal'
    SELECT transaction_type_id INTO reversal_type_id
    FROM transaction_types
    WHERE name = 'reversal';

    IF reversal_type_id IS NULL THEN
        RAISE NOTICE 'Transaction type "reversal" not found.';
        RETURN FALSE;
    END IF;

    -- Loop through each transaction ID
    FOREACH original_tx_id IN ARRAY transaction_ids LOOP
        BEGIN
            -- Get the original transaction
            SELECT * INTO original_tx
            FROM transactions
            WHERE transaction_id = original_tx_id;


            IF NOT FOUND THEN
                RAISE NOTICE 'Transaction ID % not found, skipping...', original_tx_id;
                success := FALSE;
                CONTINUE;
            END IF;

            -- Get the current account balance
            select a.balance INTO current_account_balance
            from transactions t
            inner join accounts a ON t.account_id = a.account_id
            where t.transaction_id = original_tx_id;

            -- Calculate new balance
            new_balance := current_account_balance - original_tx.amount;

            -- Insert reversal transaction
            INSERT INTO transactions (
                transaction_reference_id,
                reference,
                account_id,
                amount,
                transaction_type_id,
                created_at,
                balance_after_transaction
            ) VALUES (
                original_tx.transaction_reference_id,
                CONCAT('Reversal of TX ', original_tx.transaction_id),
                original_tx.account_id,
                -original_tx.amount,
                reversal_type_id,
                NOW(),
                new_balance
            );

            -- Update account balance
            UPDATE accounts
            SET balance = new_balance
            WHERE account_id = original_tx.account_id;
        
        EXCEPTION
            WHEN OTHERS THEN
                RAISE NOTICE 'Error processing transaction ID %: %', original_tx_id, SQLERRM;
                success := FALSE;
                CONTINUE;
        END;
    END LOOP;

    RETURN success;
END;
$$ LANGUAGE plpgsql;
