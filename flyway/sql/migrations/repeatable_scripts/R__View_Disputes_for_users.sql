DROP VIEW IF EXISTS disputes_for_users;

CREATE OR REPLACE VIEW disputes_for_users
AS
	SELECT 
			u.user_id,
			u.email,
			u.username,
			d.dispute_id
	FROM disputes d
	INNER JOIN transactions t on d.disputed_transaction_reference_id = t.transaction_reference_id
	INNER JOIN accounts a on t.account_id = a.account_id
	INNER JOIN users u on u.user_id = a.user_id