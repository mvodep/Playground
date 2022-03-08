CREATE SCHEMA IF NOT EXISTS playground;

DROP TABLE IF EXISTS playground.notices_history, playground.persons_history, playground.notices, playground.persons, playground.requests;

CREATE TABLE playground.requests
(
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    request_time TIMESTAMPTZ NOT NULL, -- always with TZ if you want to use UTC
    -- needed for foreign key - but we assume: 1 request per microsecond (maximal resolution --> 14 digits)
    UNIQUE(id, request_time)
);

CREATE TABLE playground.persons
(
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR (255) NOT NULL,
    changed_by_request_id INTEGER NOT NULL REFERENCES playground.requests(id),
    -- soft delete is in most cases a good idea
    deleted_by_request_id INTEGER REFERENCES playground.requests(id)
);

CREATE TABLE playground.notices
(
    id INTEGER GENERATED ALWAYS AS IDENTITY,
    person_id INTEGER NOT NULL REFERENCES playground.persons(id),
    notice VARCHAR(255) NOT NULL,
    changed_by_request_id INTEGER NOT NULL REFERENCES playground.requests(id),
    -- soft delete is in most cases a good idea
    deleted_by_request_id INTEGER REFERENCES playground.requests(id)
);

CREATE TABLE playground.persons_history
(
    id INTEGER NOT NULL,
    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR (255) NOT NULL,
    changed_by_request_id INTEGER NOT NULL,
    deleted_by_request_id INTEGER,
    history_valid_from TIMESTAMPTZ NOT NULL,
    history_valid_to TIMESTAMPTZ CHECK (history_valid_from < history_valid_to),
    FOREIGN KEY (changed_by_request_id, history_valid_from) REFERENCES playground.requests(id, request_time),
    FOREIGN KEY (deleted_by_request_id, history_valid_to) REFERENCES playground.requests(id, request_time),
     -- a record can be just changed once by one request
    UNIQUE(id, changed_by_request_id),
     -- a record can be just deleted once by one request
    UNIQUE(id, deleted_by_request_id)
);

CREATE TABLE playground.notices_history
(
    id INTEGER NOT NULL,
    person_id INTEGER NOT NULL REFERENCES playground.persons(id),
    notice VARCHAR(255) NOT NULL,
    changed_by_request_id INTEGER NOT NULL REFERENCES playground.requests(id),
    deleted_by_request_id INTEGER REFERENCES playground.requests(id),
    history_valid_from TIMESTAMPTZ NOT NULL,
    history_valid_to TIMESTAMPTZ CHECK (history_valid_from < history_valid_to),
    FOREIGN KEY (changed_by_request_id, history_valid_from) REFERENCES playground.requests(id, request_time),
    FOREIGN KEY (deleted_by_request_id, history_valid_to) REFERENCES playground.requests(id, request_time),
     -- a record can be just changed once by one request
    UNIQUE(id, changed_by_request_id),
     -- a record can be just deleted once by one request
    UNIQUE(id, deleted_by_request_id)
);


CREATE OR REPLACE FUNCTION playground.insert_persons_history() RETURNS TRIGGER AS
$$
BEGIN
    INSERT INTO playground.persons_history
        (id, first_name, last_name, changed_by_request_id, deleted_by_request_id, history_valid_from)
        VALUES(NEW.id, NEW.first_name, NEW.last_name, NEW.changed_by_request_id, NEW.deleted_by_request_id, (SELECT request_time FROM playground.requests WHERE id = NEW.changed_by_request_id));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION playground.insert_notices_history() RETURNS TRIGGER AS
$$
BEGIN
    INSERT INTO playground.notices_history
        (id, person_id, notice, changed_by_request_id, deleted_by_request_id, history_valid_from)
        VALUES(NEW.id, NEW.person_id, NEW.notice, NEW.changed_by_request_id, NEW.deleted_by_request_id, (SELECT request_time FROM playground.requests WHERE id = NEW.changed_by_request_id));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION playground.update_persons_history() RETURNS TRIGGER AS
$$
DECLARE var_request_time TIMESTAMPTZ;
BEGIN
    SELECT request_time INTO var_request_time FROM playground.requests WHERE id = NEW.changed_by_request_id;

    IF NEW.deleted_by_request_id IS NULL THEN
        UPDATE playground.persons_history SET history_valid_to = var_request_time WHERE id = NEW.id AND history_valid_to IS NULL;

        INSERT INTO playground.persons_history
            (id, first_name, last_name, changed_by_request_id, deleted_by_request_id, history_valid_from)
            VALUES(NEW.id, NEW.first_name, NEW.last_name, NEW.changed_by_request_id, NEW.deleted_by_request_id, var_request_time);
    ELSE
        UPDATE playground.persons_history SET deleted_by_request_id = NEW.deleted_by_request_id, history_valid_to = var_request_time WHERE id = NEW.id AND history_valid_to IS NULL;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION playground.update_notices_history() RETURNS TRIGGER AS
$$
DECLARE var_request_time TIMESTAMPTZ;
BEGIN
    SELECT request_time INTO var_request_time FROM playground.requests WHERE id = NEW.changed_by_request_id;

    IF NEW.deleted_by_request_id IS NULL THEN
        UPDATE playground.notices_history SET history_valid_to = var_request_time WHERE id = NEW.id AND history_valid_to IS NULL;

        INSERT INTO playground.notices_history
        (id, person_id, notice, changed_by_request_id, deleted_by_request_id, history_valid_from)
        VALUES(NEW.id, NEW.person_id, NEW.notice, NEW.changed_by_request_id, NEW.deleted_by_request_id, var_request_time);
    ELSE
        UPDATE playground.notices_history SET deleted_by_request_id = NEW.deleted_by_request_id, history_valid_to = var_request_time WHERE id = NEW.id AND history_valid_to IS NULL;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER insert_persons
    AFTER INSERT ON playground.persons
    FOR EACH ROW
    EXECUTE PROCEDURE playground.insert_persons_history();

CREATE TRIGGER insert_notices
    AFTER INSERT ON playground.notices
    FOR EACH ROW
    EXECUTE PROCEDURE playground.insert_notices_history();

CREATE TRIGGER update_persons
    AFTER UPDATE ON playground.persons
    FOR EACH ROW
    EXECUTE PROCEDURE playground.update_persons_history();

CREATE TRIGGER update_notices
    AFTER UPDATE ON playground.notices
    FOR EACH ROW
    EXECUTE PROCEDURE playground.update_notices_history();