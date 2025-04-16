-- //Add account_number Column (Nullable Initially)
ALTER TABLE accounts
ADD COLUMN account_number VARCHAR(10);


-- //Create a Function to Generate a Unique 10-Digit Number
CREATE OR REPLACE FUNCTION generate_unique_account_number()
RETURNS TEXT AS $$
DECLARE
    acc_num TEXT;
BEGIN
    LOOP
        acc_num := (
            SELECT string_agg((trunc(random() * 10)::int)::text, '')
            FROM generate_series(1, 10)
        );
        -- Ensure it's unique
        EXIT WHEN NOT EXISTS (
            SELECT 1 FROM accounts WHERE account_number = acc_num
        );
    END LOOP;
    RETURN acc_num;
END;
$$ LANGUAGE plpgsql;


-- //Set a Default Value Using the Function
ALTER TABLE accounts
ALTER COLUMN account_number SET DEFAULT generate_unique_account_number();

-- ///Update Existing Rows
UPDATE accounts
SET account_number = generate_unique_account_number()
WHERE account_number IS NULL;


-- //Make It Unique and Not Null
ALTER TABLE accounts
ADD CONSTRAINT account_number_unique UNIQUE (account_number);

ALTER TABLE accounts
ALTER COLUMN account_number SET NOT NULL;

