CREATE DATABASE playground;

CREATE SCHEMA IF NOT EXISTS playground;

DROP TABLE IF EXISTS playground.streets;

CREATE TABLE playground.streets
(
    gemnr TEXT,
    gemnam38 TEXT,
    okz TEXT,
    ortname TEXT,
    skz TEXT,
    stroffi TEXT,
    plznr TEXT,
    gemnr2 TEXT,
    zustort TEXT,
    code TEXT
);

CREATE EXTENSION pg_trgm;

CREATE INDEX streets_stroffi_idx ON playground.streets USING GiST(stroffi GiST_trgm_ops); 
CREATE INDEX streets_code_idx ON playground.streets USING GiST(code GiST_trgm_ops); 
CREATE INDEX streets_plznr_idx ON playground.streets USING GiST(plznr GiST_trgm_ops); 