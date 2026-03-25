-- WO-8: Row-level security for organization (tenant) isolation. Run after sql-schema.sql / sql-schema.sql baseline.
-- Requires SESSION_CONTEXT 'TenantId' set by the application (see SqlSessionContextInterceptor).

IF COL_LENGTH('dbo.AuditLog', 'TenantID') IS NULL
BEGIN
    ALTER TABLE dbo.AuditLog ADD TenantID nvarchar(128) NULL;
    UPDATE al
    SET al.TenantID = a.TenantID
    FROM dbo.AuditLog AS al
    INNER JOIN dbo.Assets AS a ON al.AssetID = a.AssetID;
    ALTER TABLE dbo.AuditLog ALTER COLUMN TenantID nvarchar(128) NOT NULL;
END
GO

IF COL_LENGTH('dbo.LicenseUtilization', 'TenantID') IS NULL
BEGIN
    ALTER TABLE dbo.LicenseUtilization ADD TenantID nvarchar(128) NULL;
    UPDATE lu
    SET lu.TenantID = a.TenantID
    FROM dbo.LicenseUtilization AS lu
    INNER JOIN dbo.Assets AS a ON lu.AssetID = a.AssetID;
    ALTER TABLE dbo.LicenseUtilization ALTER COLUMN TenantID nvarchar(128) NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Security')
    EXEC('CREATE SCHEMA Security');
GO

IF OBJECT_ID(N'Security.fn_tenant_predicate', N'IF') IS NOT NULL
    DROP FUNCTION Security.fn_tenant_predicate;
GO

CREATE FUNCTION Security.fn_tenant_predicate (@TenantID nvarchar(128))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_access
WHERE @TenantID = CAST(SESSION_CONTEXT(N'TenantId') AS nvarchar(128));
GO

IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = N'TenantIsolationPolicy')
    DROP SECURITY POLICY dbo.TenantIsolationPolicy;
GO

CREATE SECURITY POLICY dbo.TenantIsolationPolicy
    ADD FILTER PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.Assets,
    ADD BLOCK PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.Assets AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.Assets BEFORE UPDATE,
    ADD FILTER PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.AuditLog,
    ADD BLOCK PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.AuditLog AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.AuditLog BEFORE UPDATE,
    ADD FILTER PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.LicenseUtilization,
    ADD BLOCK PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.LicenseUtilization AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_tenant_predicate(TenantID) ON dbo.LicenseUtilization BEFORE UPDATE
WITH (STATE = ON);
GO
