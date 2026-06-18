IF DB_ID(N'MAUN_TOMER') IS NULL
BEGIN
    CREATE DATABASE MAUN_TOMER;
END
GO

USE MAUN_TOMER;
GO

IF OBJECT_ID(N'dbo.Tomer_CertificateInventory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tomer_CertificateInventory
    (
        CertificateId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tomer_CertificateInventory PRIMARY KEY,
        CertificateDate DATETIME NOT NULL,
        IdentityOrPassportNo NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(255) NOT NULL,
        CertificateNo NVARCHAR(100) NULL,
        [Level] NVARCHAR(50) NOT NULL,
        ReadingScore INT NOT NULL,
        WritingScore INT NOT NULL,
        ListeningScore INT NOT NULL,
        SpeakingScore INT NOT NULL,
        TotalScore INT NULL,
        PassingStatus NVARCHAR(100) NULL
    );

    CREATE INDEX IX_Tomer_CertificateInventory_IdentityOrPassportNo
        ON dbo.Tomer_CertificateInventory(IdentityOrPassportNo);
END
GO
