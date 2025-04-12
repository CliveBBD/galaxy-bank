-- Recalculates all account balances and transaction balance_after_transaction fields
DO $$
DECLARE
    recalculate_start_time TIMESTAMP := clock_timestamp();
BEGIN
    RAISE NOTICE 'Starting balance recalculation at %', recalculate_start_time;

    -- Temporarily disable triggers to improve performance
    ALTER TABLE transactions DISABLE TRIGGER ALL;
    ALTER TABLE accounts DISABLE TRIGGER ALL;

    -- Calculate balance_after_transaction for all transactions
    WITH ordered_transactions AS (
        SELECT 
            transaction_id,
            account_id,
            amount,
            created_at,
            SUM(amount) OVER (
                PARTITION BY account_id 
                ORDER BY created_at, transaction_id
                ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
            ) AS cumulative_balance
        FROM transactions
    )
    UPDATE transactions t
    SET balance_after_transaction = ot.cumulative_balance
    FROM ordered_transactions ot
    WHERE t.transaction_id = ot.transaction_id;

    -- Update account balances with latest transaction balance
    UPDATE accounts a
    SET balance = COALESCE((
        SELECT balance_after_transaction
        FROM transactions
        WHERE account_id = a.account_id
        ORDER BY created_at DESC, transaction_id DESC
        LIMIT 1
    ), 0)
    WHERE EXISTS (
        SELECT 1 FROM transactions WHERE account_id = a.account_id
    );

    -- Update accounts with no transactions to balance 0
    UPDATE accounts
    SET balance = 0
    WHERE account_id NOT IN (
        SELECT DISTINCT account_id FROM transactions
    );

    -- Re-enable triggers
    ALTER TABLE transactions ENABLE TRIGGER ALL;
    ALTER TABLE accounts ENABLE TRIGGER ALL;

    RAISE NOTICE 'Balance recalculation completed in %', 
        clock_timestamp() - recalculate_start_time;

EXCEPTION
    WHEN OTHERS THEN
        RAISE WARNING 'Balance recalculation failed: %', SQLERRM;
        -- Ensure triggers are re-enabled even on failure
        ALTER TABLE transactions ENABLE TRIGGER ALL;
        ALTER TABLE accounts ENABLE TRIGGER ALL;
        ROLLBACK;
        RAISE;
END $$;