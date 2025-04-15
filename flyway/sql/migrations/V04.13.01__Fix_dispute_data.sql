-- Script to undo and re-seed disputes with transfer-out only rule
BEGIN;

-- Part 1: Undo previous dispute seeding
-- First, identify the reversal transactions created by disputes
CREATE TEMP TABLE dispute_reversals AS
SELECT 
  t.transaction_id,
  t.account_id,
  t.amount
FROM transactions t
WHERE t.transaction_type_id = (
  SELECT transaction_type_id FROM transaction_types WHERE name = 'reversal'
)
AND t.reference LIKE 'Dispute reversal for #%';

-- Fix account balances by adding back amounts from accepted disputes
UPDATE accounts a
SET balance = balance + dr.amount
FROM dispute_reversals dr
WHERE a.account_id = dr.account_id;

-- Remove reversal transactions
DELETE FROM transactions 
WHERE transaction_id IN (SELECT transaction_id FROM dispute_reversals);

-- Remove dispute status history
DELETE FROM dispute_status_history 
WHERE dispute_id IN (
  SELECT dispute_id FROM disputes
);

-- Remove all disputes
TRUNCATE disputes RESTART IDENTITY CASCADE;

-- Clean up temporary table
DROP TABLE dispute_reversals;

-- Part 2: Re-seed disputes with new transfer-out only rule
-- Set a random seed for consistent results
SELECT setseed(0.5);

-- Temporary table to hold transfer-out transactions we'll dispute
CREATE TEMP TABLE candidate_transfer_transactions AS
WITH ranked_transactions AS (
  SELECT 
    t.transaction_id,
    t.transaction_reference_id,
    t.account_id,
    t.amount,
    t.transaction_type_id,
    t.created_at,
    t.balance_after_transaction,
    a.user_id,
    a.balance as current_balance,
    ROW_NUMBER() OVER (PARTITION BY a.user_id ORDER BY random()) as user_rank
  FROM transactions t
  JOIN accounts a ON t.account_id = a.account_id
  WHERE t.transaction_type_id = 3 -- Only transfer-out transactions
  -- Exclude transactions that are already part of a dispute
  AND NOT EXISTS (
    SELECT 1 FROM disputes d 
    WHERE d.disputed_transaction_reference_id = t.transaction_reference_id
  )
  -- Only include transactions older than 1 week
  AND t.created_at < now() - interval '7 days'
  -- Only include transactions that wouldn't cause negative balance if reversed
  AND (t.amount <= 0 OR (a.balance - t.amount) >= 0)
)
SELECT * FROM ranked_transactions
-- Select about 300 transactions to dispute
WHERE user_rank <= 5 OR random() < 0.05
LIMIT 300;

-- Create the disputes with realistic status histories
INSERT INTO disputes (reason, disputed_transaction_reference_id, created_at)
SELECT 
  CASE 
    WHEN random() < 0.3 THEN 'Unauthorized transfer'
    WHEN random() < 0.6 THEN 'Incorrect transfer amount'
    WHEN random() < 0.8 THEN 'Duplicate transfer'
    ELSE 'Transfer to wrong recipient'
  END as reason,
  transaction_reference_id,
  created_at + (random() * interval '7 days') as created_at
FROM candidate_transfer_transactions;

-- Create status history for each dispute
-- All disputes start as "open"
INSERT INTO dispute_status_history (dispute_id, dispute_status_id, updated_at, updated_by_id)
SELECT 
  d.dispute_id,
  1 as dispute_status_id, -- open
  d.created_at as updated_at,
  (SELECT user_id FROM users WHERE role_id = (SELECT role_id FROM roles WHERE name = 'admin') ORDER BY random() LIMIT 1) as updated_by_id
FROM disputes d
JOIN candidate_transfer_transactions ct ON d.disputed_transaction_reference_id = ct.transaction_reference_id;

-- Some disputes move to "under_review"
INSERT INTO dispute_status_history (dispute_id, dispute_status_id, updated_at, updated_by_id)
SELECT 
  d.dispute_id,
  2 as dispute_status_id, -- under_review
  d.created_at + (random() * interval '2 days') as updated_at,
  (SELECT user_id FROM users WHERE role_id = (SELECT role_id FROM roles WHERE name = 'admin') ORDER BY random() LIMIT 1) as updated_by_id
FROM disputes d
JOIN candidate_transfer_transactions ct ON d.disputed_transaction_reference_id = ct.transaction_reference_id
WHERE random() < 0.8; -- 80% of disputes get reviewed

-- Some disputes get resolved (accepted or rejected)
INSERT INTO dispute_status_history (dispute_id, dispute_status_id, updated_at, updated_by_id)
SELECT 
  d.dispute_id,
  CASE WHEN random() < 0.7 THEN 3 ELSE 4 END as dispute_status_id, -- 70% accepted, 30% rejected
  d.created_at + (random() * interval '5 days') as updated_at,
  (SELECT user_id FROM users WHERE role_id = (SELECT role_id FROM roles WHERE name = 'admin') ORDER BY random() LIMIT 1) as updated_by_id
FROM disputes d
JOIN candidate_transfer_transactions ct ON d.disputed_transaction_reference_id = ct.transaction_reference_id
WHERE random() < 0.6; -- 60% of reviewed disputes get resolved

-- Create reversal transactions for accepted disputes
INSERT INTO transactions (
  transaction_reference_id,
  reference,
  account_id,
  amount,
  transaction_type_id,
  created_at,
  balance_after_transaction
)
SELECT 
  d.disputed_transaction_reference_id,
  'Dispute reversal for transfer #' || ct.transaction_id as reference,
  ct.account_id,
  -ct.amount as amount, -- reverse the original amount
  (SELECT transaction_type_id FROM transaction_types WHERE name = 'reversal') as transaction_type_id,
  dsh.updated_at as created_at,
  a.balance - ct.amount as balance_after_transaction
FROM disputes d
JOIN dispute_status_history dsh ON d.dispute_id = dsh.dispute_id
JOIN candidate_transfer_transactions ct ON d.disputed_transaction_reference_id = ct.transaction_reference_id
JOIN accounts a ON ct.account_id = a.account_id
WHERE dsh.dispute_status_id = 3 -- accepted
AND dsh.dispute_history_id = (
  SELECT MAX(dispute_history_id) 
  FROM dispute_status_history 
  WHERE dispute_id = d.dispute_id
);

-- Update account balances for accepted disputes
UPDATE accounts a
SET balance = balance - ct.amount
FROM disputes d
JOIN dispute_status_history dsh ON d.dispute_id = dsh.dispute_id
JOIN candidate_transfer_transactions ct ON d.disputed_transaction_reference_id = ct.transaction_reference_id
WHERE dsh.dispute_status_id = 3 -- accepted
AND dsh.dispute_history_id = (
  SELECT MAX(dispute_history_id) 
  FROM dispute_status_history 
  WHERE dispute_id = d.dispute_id
)
AND a.account_id = ct.account_id;

-- Clean up
DROP TABLE candidate_transfer_transactions;

COMMIT;

-- Verify we didn't create any negative balances
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM accounts WHERE balance < 0) THEN
    RAISE EXCEPTION 'Dispute seeding created negative balances - rolling back';
  END IF;
END $$;