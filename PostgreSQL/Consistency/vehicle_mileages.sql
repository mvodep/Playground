DROP TABLE IF EXISTS playground.vehicle_mileages CASCADE;

CREATE TABLE vehicle_mileages (
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    vehicle_id int NOT NULL,
    mileage int NOT NULL,
    reported_at date NOT NULL,
    UNIQUE (vehicle_id, reported_at)
);

CREATE OR REPLACE FUNCTION playground.vehicle_mileages_increasing_check ()
    RETURNS TRIGGER
    AS $$
BEGIN
    -- Gain lock for all verhicles to ensure consistency. If another transaction holds a row lock, this one will block
    PERFORM
        1
    FROM
        playground.vehicle_mileages
    WHERE
        vehicle_id = NEW.vehicle_id
    ORDER BY
        reported_at
    FOR NO KEY UPDATE;
    -- User window functions to check if its increasing. There won't be that much records - so its ok to use them
    IF EXISTS (
        SELECT
        FROM (
            SELECT
                mileage,
                lag(mileage) OVER (ORDER BY reported_at) AS prev_mileage
            FROM
                playground.vehicle_mileages
            WHERE
                vehicle_id = NEW.vehicle_id) AS s
        WHERE
            mileage < prev_mileage) THEN
    RAISE EXCEPTION 'Mileage must be increasing' USING ERRCODE = 'MV001';
    END IF;
    RETURN NEW;
END;
$$
LANGUAGE plpgsql
VOLATILE;

DROP TRIGGER IF EXISTS trigger_insert_update_vehicle_mileages_increasing_check ON playground.vehicle_mileages;

CREATE TRIGGER trigger_insert_update_vehicle_mileages_increasing_check
    AFTER INSERT OR UPDATE ON playground.vehicle_mileages
    FOR EACH ROW
    EXECUTE PROCEDURE playground.vehicle_mileages_increasing_check ();

INSERT INTO vehicle_mileages (vehicle_id, mileage, reported_at) VALUES (1, 10, '2023-05-01'), (1, 10, '2023-05-02'), (1, 250, '2023-05-05');
INSERT INTO vehicle_mileages (vehicle_id, mileage, reported_at) VALUES (2, 350, '2023-05-07');

-- T1    
BEGIN;
UPDATE vehicle_mileages SET mileage = 350 WHERE reported_at = '2023-05-05' AND vehicle_id = 1;
COMMIT;
    
-- T2
BEGIN;
INSERT INTO vehicle_mileages (vehicle_id, mileage, reported_at) VALUES (1, 300, '2023-05-08');
COMMIT;

-- T1 
BEGIN;
UPDATE vehicle_mileages set vehicle_id = 1 WHERE reported_at = '2023-05-07' AND vehicle_id = 2;
COMMIT;
  
-- T2
BEGIN;
INSERT INTO vehicle_mileages (vehicle_id, mileage, reported_at) VALUES (1, 300, '2023-05-08');
COMMIT;
   
SELECT * FROM vehicle_mileages WHERE vehicle_id = 1 ORDER BY reported_at;