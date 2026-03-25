# Deployment TODO

Everything that is still required or recommended before and after deploying the csharp-cosmos stack.

---

## 1. Infrastructure (Bicep / parameters)

| Item | Description | Where |
|------|-------------|--------|
| **SQL admin password** | Azure SQL Server requires `sqlAdministratorLoginPassword`. Empty default will cause SQL deployment to fail. | Set in `infra/main.parameters.json` (e.g. reference a secret or use a secure pipeline variable) or pass at deploy time. Do not commit the password. |
| **SQL admin login** | Optional override; default is `sqladmin`. | `main.bicep` param `sqlAdministratorLogin`; add to `main.parameters.json` if needed. |
| **Principal ID** | Required for Key Vault access (including the phase-1 policies used for SQL TDE and Cosmos CMK keys) and Cosmos RBAC. Must be the object (principal) ID of the user or app that needs access. If empty, the first Key Vault access policy may be invalid—set this in every environment that deploys CMK/TDE. | `main.parameters.json`: `principalId` → `${AZURE_PRINCIPAL_ID}`. Ensure this is set in your deployment pipeline or `azd` environment. |
| **Environment name and location** | Required for all deployments. | `main.parameters.json`: `environmentName` → `${AZURE_ENV_NAME}`, `location` → `${AZURE_LOCATION}`. |
| **Customer-managed keys (WO-8)** | When `deployCustomerManagedKeys` is `true` (default in `main.bicep`), Key Vault enables purge protection, creates RSA keys for SQL TDE BYOK and Cosmos CMK, and wires SQL encryption protector + Cosmos account CMK. If deployment fails (e.g. Cosmos serverless + CMK constraints in a region), set `deployCustomerManagedKeys` to `false` in parameters and redeploy, then enable CMK manually per [Azure Cosmos DB CMK](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-setup-customer-managed-keys). | `main.bicep` param `deployCustomerManagedKeys`; optional override in `main.parameters.json`. |

---

## 2. Post-deploy: SQL schema

| Item | Description | Where |
|------|-------------|--------|
| **Run reference schema** | Tables `Assets`, `AuditLog`, and `LicenseUtilization` are not created by Bicep. You must run the SQL script once per database/environment. | Run `infra/core/database/sql-schema.sql` against the deployed Azure SQL database (e.g. via sqlcmd, Azure Data Studio, or a one-off pipeline step). Use the same server/database and admin (or a user with `db_ddladmin`) that the app will use. |
| **Run row-level security (WO-8)** | After `sql-schema.sql`, apply RLS for tenant isolation (`TenantID` on all three tables, security policy, predicate function). Required for production RLS; skip only in local/dev scenarios where you intentionally do not enforce RLS. | Run `infra/core/database/sql-rls.sql` **after** the baseline schema exists. If the database was created from an older `sql-schema.sql` without `TenantID` on `AuditLog` / `LicenseUtilization`, this script adds columns, backfills from `Assets`, and creates the policy. The API sets `SESSION_CONTEXT('TenantId')` via `SqlSessionContextInterceptor` when JWT includes `tenant_id` or `AzureSql:DefaultTenantId` is set. |

---

## 3. Application configuration / secrets

| Item | Description | Where |
|------|-------------|--------|
| **Cosmos DB** | API needs Cosmos endpoint and database name for CosmosClient, ICosmosDbContext, and DatabaseInitializer. | In Azure: set via App Service app settings from Bicep outputs (`AZURE_COSMOS_ENDPOINT`, `AZURE_COSMOS_DATABASE_NAME`). When not using managed identity (e.g. emulator or key-based auth), set `AZURE_COSMOS_KEY`. Optional: `CosmosDb:MaxConnectionLimit`, `CosmosDb:RequestTimeout` for tuning. For local dev: set in `appsettings.Development.json` or env, or leave empty to skip Cosmos. |
| **Cosmos TLS / certificate pinning (WO-8)** | Optional HTTPS certificate pinning for Cosmos endpoints. | Set `CosmosDb:EnableCertificatePinning` to `true` and list allowed server cert SHA1 thumbprints in `CosmosDb:PinnedCertificateThumbprints` (array in JSON). When enabled, pinning is strict; Azure rotation may require updating thumbprints. |
| **Azure SQL connection string** | API uses `ReferenceDataDbContext` and `SqlDbContext` (data access layer) when a connection string is present. | Set `AzureSql:ConnectionString` or `AZURE_SQL_CONNECTION_STRING` (server, database, user, password; consider storing password in Key Vault and referencing from App Service). The host sets `Encrypt=true` and `TrustServerCertificate=false` in code. Optional: `AzureSql:MaxPoolSize`, `AzureSql:ConnectionTimeout`, **`AzureSql:DefaultTenantId`** (fallback when no JWT `tenant_id`—e.g. health probes; align with RLS and real tenant data). Not set by Bicep. |
| **PII field encryption (WO-8)** | Optional AES-256-GCM helper for PII stored in Cosmos documents. | When `PiiEncryption:EncryptionKeyBase64` is set (32-byte key, Base64), `IPiiFieldProtector` is registered. **Do not** use placeholder keys in production; use a cryptographically random key from Key Vault or a secure secret store. For local dev `appsettings.Development.json`, replace any sample key before sharing the repo. |
| **Redis** | Idempotency and caching use Redis when a connection string is set. | Set `Redis:ConnectionString` or `AZURE_REDIS_CONNECTION_STRING` if using Azure Cache for Redis. Not created by current Bicep; add Redis resource or set post-deploy. |
| **Key Vault** | API gets `AZURE_KEY_VAULT_ENDPOINT` from Bicep. Actual secrets (e.g. SQL password, Redis connection string) must be stored and optionally referenced by the app. | Populate Key Vault with secrets and, if desired, configure App Service to pull them as app settings. |
| **Cosmos init on non-Dev** | DatabaseInitializer runs only in Development or when `INITIALIZE_COSMOS_ON_STARTUP` is set. | To ensure Cosmos database/containers exist in non-Dev (e.g. staging), set `INITIALIZE_COSMOS_ON_STARTUP=true` in app settings or run a one-off script. |

---

## 4. Tooling and validation

| Item | Description | Where |
|------|-------------|--------|
| **Bicep CLI** | Optional; used to validate and build Bicep templates locally. | Install [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install); run `az bicep build --file main.bicep` from `infra/` to validate. |
| **azd (Azure Developer CLI)** | If using `azd` for deploy, ensure env vars and secrets (e.g. SQL password, principal ID) are set in the azd environment. | `.azure/` and `azd env set` (or equivalent) for each environment. |
| **CI and pre-commit (WO-8)** | After code changes, run the full test suite (`dotnet test` on `src/api/Todo.Api.sln`) and `pre-commit run --all-files` before merge; ensure pipeline passes before marking WO-8 complete. | Repository root; see `.pre-commit-config.yaml` if present. |

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
| **Cosmos CMK and serverless (WO-8)** | If deployment fails with CMK enabled, confirm [Azure limitations](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-setup-customer-managed-keys) for your account type/region and consider `deployCustomerManagedKeys: false` in parameters until CMK is validated manually. |

---

## 7. WO-8 security follow-up checklist

Use this after the WO-8 infrastructure and security changes are merged.

| Task | Action |
|------|--------|
| **Principal ID** | Ensure `principalId` is set for Key Vault access policies (phase 1 and 2) so deployment and runtime identities are not missing from the vault. |
| **SQL RLS** | Run `infra/core/database/sql-rls.sql` on each environment’s database after `sql-schema.sql` (or after confirming columns/policies are applied). |
| **CI / pre-commit** | Run `dotnet test` on `Todo.Api.sln` and `pre-commit run --all-files`; fix any failures before release. |
| **Cosmos CMK deploy** | If deploy fails with default `deployCustomerManagedKeys: true`, adjust parameters or enable CMK in the portal per Microsoft docs; verify serverless/SKU compatibility. |
| **PII encryption key** | Replace development sample `PiiEncryption:EncryptionKeyBase64` with a production secret (Key Vault or managed secret store); never commit real production keys. |
| **JWT tenant claims** | Bootstrap users and login flows should emit `tenant_id` where RLS applies; align `AzureSql:DefaultTenantId` with your test/prod tenant for probes and non-user paths. |

---

## Quick checklist before first deploy

- [ ] Set `principalId` (e.g. `AZURE_PRINCIPAL_ID`) for Key Vault and Cosmos.
- [ ] Set `sqlAdministratorLoginPassword` securely (parameters or pipeline); do not commit.
- [ ] Set `environmentName` and `location` (e.g. via `main.parameters.json` or azd).
- [ ] After deploy: run `infra/core/database/sql-schema.sql` on the SQL database.
- [ ] After schema: run `infra/core/database/sql-rls.sql` if using tenant RLS (WO-8).
- [ ] Review `deployCustomerManagedKeys` and CMK/TDE requirements; if deploy fails, see §1 and §6.
- [ ] Configure API app settings: Cosmos (often from outputs), SQL connection string, optional **`AzureSql:DefaultTenantId`**, **`PiiEncryption`** (production key), optional Cosmos pinning, and optionally Redis and Key Vault references.
- [ ] Run tests and pre-commit (`dotnet test` on `Todo.Api.sln`, `pre-commit run --all-files`).
