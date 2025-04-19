INSERT INTO roles (name)
VALUES ('system_admin');

UPDATE roles
SET name = 'dispute_officer'
WHERE name = 'admin';