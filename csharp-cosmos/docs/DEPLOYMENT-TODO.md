# Deployment TODO

Everything that is still required or recommended before and after deploying the csharp-cosmos stack.

---

## 1. Infrastructure (Bicep / parameters)

| Item | Description | Where |
|------|-------------|--------|
| **SQL admin password** | Azure SQL Server requires `sqlAdministratorLoginPassword`. Empty default will cause SQL deployment to fail. | Set in `infra/main.parameters.json` (e.g. reference a secret or use a secure pipeline variable) or pass at deploy time. Do not commit the password. |
| **SQL admin login** | Optional override; default is `sqladmin`. | `main.bicep` param `sqlAdministratorLogin`; add to `main.parameters.json` if needed. |
| **Principal ID** | Required for Key Vault access and Cosmos RBAC. Must be the object (principal) ID of the user or app that needs access. | `main.parameters.json`: `principalId` → `${AZURE_PRINCIPAL_ID}`. Ensure this is set in your deployment pipeline or `azd` environment. |
| **Environment name and location** | Required for all deployments. | `main.parameters.json`: `environmentName` → `${AZURE_ENV_NAME}`, `location` → `${AZURE_LOCATION}`. |

---

## 2. Post-deploy: SQL schema

| Item | Description | Where |
|------|-------------|--------|
| **Run reference schema** | Tables `Assets`, `AuditLog`, and `LicenseUtilization` are not created by Bicep. You must run the SQL script once per database/environment. | Run `infra/core/database/sql-schema.sql` against the deployed Azure SQL database (e.g. via sqlcmd, Azure Data Studio, or a one-off pipeline step). Use the same server/database and admin (or a user with `db_ddladmin`) that the app will use. |

---

## 3. Application configuration / secrets

| Item | Description | Where |
|------|-------------|--------|
| **Cosmos DB** | API needs Cosmos endpoint and database name for CosmosClient, ICosmosDbContext, and DatabaseInitializer. | In Azure: set via App Service app settings from Bicep outputs (`AZURE_COSMOS_ENDPOINT`, `AZURE_COSMOS_DATABASE_NAME`). When not using managed identity (e.g. emulator or key-based auth), set `AZURE_COSMOS_KEY`. Optional: `CosmosDb:MaxConnectionLimit`, `CosmosDb:RequestTimeout` for tuning. For local dev: set in `appsettings.Development.json` or env, or leave empty to skip Cosmos. |
| **Azure SQL connection string** | API uses `ReferenceDataDbContext` and `SqlDbContext` (data access layer) when a connection string is present. | Set `AzureSql:ConnectionString` or `AZURE_SQL_CONNECTION_STRING` (server, database, user, password; consider storing password in Key Vault and referencing from App Service). Optional: `AzureSql:MaxPoolSize`, `AzureSql:ConnectionTimeout` for tuning. Not set by Bicep. |
| **Redis** | Idempotency and caching use Redis when a connection string is set. | Set `Redis:ConnectionString` or `AZURE_REDIS_CONNECTION_STRING` if using Azure Cache for Redis. Not created by current Bicep; add Redis resource or set post-deploy. |
| **Key Vault** | API gets `AZURE_KEY_VAULT_ENDPOINT` from Bicep. Actual secrets (e.g. SQL password, Redis connection string) must be stored and optionally referenced by the app. | Populate Key Vault with secrets and, if desired, configure App Service to pull them as app settings. |
| **Cosmos init on non-Dev** | DatabaseInitializer runs only in Development or when `INITIALIZE_COSMOS_ON_STARTUP` is set. | To ensure Cosmos database/containers exist in non-Dev (e.g. staging), set `INITIALIZE_COSMOS_ON_STARTUP=true` in app settings or run a one-off script. |

---

## 4. Tooling and validation

| Item | Description | Where |
|------|-------------|--------|
| **Bicep CLI** | Optional; used to validate and build Bicep templates locally. | Install [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install); run `bicep build main.bicep` from `infra/` to validate. |
| **azd (Azure Developer CLI)** | If using `azd` for deploy, ensure env vars and secrets (e.g. SQL password, principal ID) are set in the azd environment. | `.azure/` and `azd env set` (or equivalent) for each environment. |

---

## 5. Blueprint / product follow-ups (not deployment-blocking)

From [BLUEPRINT-RECONCILIATION.md](./BLUEPRINT-RECONCILIATION.md); needed for full blueprint alignment but not required to deploy:

| Item | Description |
|------|-------------|
| **SpecFlow / ATDD** | Add acceptance tests in Gherkin with SpecFlow when feature acceptance criteria are defined. |
| **Rate limiting** | Add per blueprint security requirements. |
| **Audit trail** | Implement audit for state changes at feature level. |
| **Cache key convention** | Use `{feature}:{entity}:{id}` for new feature caches; idempotency already uses its own prefix. |

---

## 6. Known / optional items

| Item | Description |
|------|-------------|
| **NU1903 (Caching.Memory)** | Package vulnerability warning for `Microsoft.Extensions.Caching.Memory` 8.0.0; documented as accepted in BLUEPRINT-RECONCILIATION. Upgrade when a fixed 8.x version is available and aligned with the rest of the stack. |
| **Redis in Bicep** | Azure Cache for Redis is not in the current Bicep; add a module and wire connection string to the API if you want Redis in infra. |
| **SQL connection string in Bicep** | Bicep does not output a full SQL connection string (it contains the password). Build the connection string in the pipeline or app config from server FQDN, database name, and a secret (e.g. from Key Vault). |

---

## Quick checklist before first deploy

- [ ] Set `principalId` (e.g. `AZURE_PRINCIPAL_ID`) for Key Vault and Cosmos.
- [ ] Set `sqlAdministratorLoginPassword` securely (parameters or pipeline); do not commit.
- [ ] Set `environmentName` and `location` (e.g. via `main.parameters.json` or azd).
- [ ] After deploy: run `infra/core/database/sql-schema.sql` on the SQL database.
- [ ] Configure API app settings: Cosmos (often from outputs), SQL connection string, and optionally Redis and Key Vault references.
