ALTER TABLE IF EXISTS accounts 
  DROP CONSTRAINT IF EXISTS min_account_balance_check;

ALTER TABLE IF EXISTS transactions 
  DROP CONSTRAINT IF EXISTS min_balance_after_transaction_check;