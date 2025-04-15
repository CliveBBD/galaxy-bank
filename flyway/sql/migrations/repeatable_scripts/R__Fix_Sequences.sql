DO $$
DECLARE
    rec RECORD;
    seq_name TEXT;
    max_id BIGINT;
BEGIN
    FOR rec IN
        SELECT 
            c.relname AS table_name,
            a.attname AS column_name,
            pg_get_serial_sequence(c.relname, a.attname) AS sequence_name
        FROM pg_class c
        JOIN pg_attribute a ON a.attrelid = c.oid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE 
            a.attnum > 0 
            AND NOT a.attisdropped
            AND pg_get_serial_sequence(c.relname, a.attname) IS NOT NULL
            AND n.nspname = 'public'  -- Change if you're using a different schema
    LOOP
        EXECUTE format('SELECT MAX(%I) FROM %I', rec.column_name, rec.table_name)
        INTO max_id;

        IF max_id IS NOT NULL THEN
            EXECUTE format('SELECT setval(%L, %s)', rec.sequence_name, max_id);
        END IF;
    END LOOP;
END $$;
