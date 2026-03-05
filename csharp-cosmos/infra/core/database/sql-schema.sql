-- WO-7: Reference tables for Azure SQL Database (Assets, AuditLog, LicenseUtilization).
-- Run this script against the provisioned database to create schema.

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assets')
BEGIN
    CREATE TABLE dbo.Assets (
        AssetID nvarchar(128) NOT NULL,
        AssetName nvarchar(256) NOT NULL,
        AssetType nvarchar(64) NOT NULL,
        TenantID nvarchar(128) NOT NULL,
        CreatedDate datetime2(7) NOT NULL,
        LastModifiedDate datetime2(7) NOT NULL,
        CONSTRAINT PK_Assets PRIMARY KEY (AssetID)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLog')
BEGIN
    CREATE TABLE dbo.AuditLog (
        AuditID uniqueidentifier NOT NULL,
        AssetID nvarchar(128) NOT NULL,
        Action nvarchar(64) NOT NULL,
        UserID nvarchar(128) NOT NULL,
        Timestamp datetime2(7) NOT NULL,
        Details nvarchar(max) NULL,
        CONSTRAINT PK_AuditLog PRIMARY KEY (AuditID),
        CONSTRAINT FK_AuditLog_Assets FOREIGN KEY (AssetID) REFERENCES dbo.Assets(AssetID)
    );
    CREATE NONCLUSTERED INDEX IX_AuditLog_Timestamp ON dbo.AuditLog (Timestamp DESC);
    CREATE NONCLUSTERED INDEX IX_AuditLog_AssetID ON dbo.AuditLog (AssetID);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LicenseUtilization')
BEGIN
    CREATE TABLE dbo.LicenseUtilization (
        UtilizationID uniqueidentifier NOT NULL,
        AssetID nvarchar(128) NOT NULL,
        LicenseType nvarchar(64) NOT NULL,
        UsedCount int NOT NULL,
        TotalCount int NOT NULL,
        SnapshotDate datetime2(7) NOT NULL,
        CONSTRAINT PK_LicenseUtilization PRIMARY KEY (UtilizationID),
        CONSTRAINT FK_LicenseUtilization_Assets FOREIGN KEY (AssetID) REFERENCES dbo.Assets(AssetID)
    );
    CREATE NONCLUSTERED INDEX IX_LicenseUtilization_SnapshotDate ON dbo.LicenseUtilization (SnapshotDate DESC);
    CREATE NONCLUSTERED INDEX IX_LicenseUtilization_AssetID ON dbo.LicenseUtilization (AssetID);
END
GO
