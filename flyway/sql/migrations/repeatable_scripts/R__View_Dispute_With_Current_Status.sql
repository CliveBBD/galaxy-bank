DROP VIEW IF EXISTS dispute_with_current_status;

CREATE OR REPLACE VIEW dispute_with_current_status
AS
WITH
	dispute_status_history_with_to_date AS (
		SELECT
			dispute_history_id,
			dispute_id,
			dispute_status_id,
			updated_at AS from_date,
			updated_by_id,
			LEAD(updated_at, 1, '9999-12-31') OVER (PARTITION BY dispute_id ORDER BY updated_at) AS to_date
		FROM dispute_status_history
	),
	dispute_users AS (
		SELECT 
			u.user_id,
			u.email,
			u.username,
			dsh.dispute_history_id,
			dsh.dispute_id,
			dsh.dispute_status_id,
			dsh.from_date,
			dsh.to_date,
			dsh.updated_by_id
		FROM dispute_status_history_with_to_date dsh
		INNER JOIN disputes d ON dsh.dispute_id = d.dispute_id
		INNER JOIN transactions t on d.disputed_transaction_reference_id = t.transaction_reference_id
		INNER JOIN accounts a on t.account_id = a.account_id
		INNER JOIN users u on u.user_id = a.user_id
	)
	SELECT 
		d.dispute_id,
		d.reason,
		d.disputed_transaction_reference_id,
		d.created_at,
		dsh.dispute_history_id,
		dsh.dispute_status_id,
		dsh.from_date AS updated_at,
		dsh.updated_by_id,
		ds.name,
		u.username AS updated_by,
		u.email AS updated_by_email,
		dsh.user_id as involved_user_id,
		dsh.email as involved_email,
		dsh.username as involved_username
	FROM disputes d
		INNER JOIN dispute_users dsh ON d.dispute_id = dsh.dispute_id
		INNER JOIN dispute_statuses ds ON dsh.dispute_status_id = ds.dispute_status_id
		INNER JOIN users u ON dsh.updated_by_id = u.user_id
	WHERE NOW() BETWEEN dsh.from_date AND dsh.to_date;