--
-- File generated with SQLiteStudio v3.2.1 on Wed Aug 12 13:51:38 2020
--
-- Text encoding used: System
--
BEGIN TRANSACTION;

-- Table: AlgorithmType
CREATE TABLE AlgorithmType (
    Id          INTEGER      PRIMARY KEY,
    Type        nvarchar (50)  UNIQUE,
    Description nvarchar (300) 
);

-- Table: Algorithm
CREATE TABLE Algorithm (
    Id          INTEGER      PRIMARY KEY,
    Name        nvarchar (100),
    Description nvarchar (300),
    TypeId      INTEGER      REFERENCES AlgorithmType (Id),
    AlgorithmId INTEGER      REFERENCES Algorithm (Id) 
);

-- Table: Application
CREATE TABLE Application (
    Id            INTEGER      PRIMARY KEY,
    Name          nvarchar (100) UNIQUE,
    Description   nvarchar (300),
    UpdateDt      TIME         NOT NULL
                               DEFAULT (getdate() ),
    ApplicationId INTEGER      REFERENCES Application (Id) 
);


-- Table: ApplicationData
CREATE TABLE ApplicationData (
    Id                 INTEGER      PRIMARY KEY,
    ApplicationId      INTEGER      REFERENCES Application (Id),
    DatasetName        nvarchar (150) NOT NULL,
    DatasetDescription nvarchar (300),
    DatasetValue       TEXT         NOT NULL
);


COMMIT TRANSACTION;

