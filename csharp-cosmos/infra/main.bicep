targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

// Optional parameters to override the default azd resource naming conventions. Update the main.parameters.json file to provide values. e.g.,:
// "resourceGroupName": {
//      "value": "myGroupName"
// }
param apiServiceName string = ''
param applicationInsightsDashboardName string = ''
param applicationInsightsName string = ''
param appServicePlanName string = ''
param cosmosAccountName string = ''
param keyVaultName string = ''
param logAnalyticsName string = ''
param resourceGroupName string = ''
param webServiceName string = ''
param apimServiceName string = ''
param sqlServerName string = ''
param sqlDatabaseName string = ''
param sqlAdministratorLogin string = 'sqladmin'
@secure()
param sqlAdministratorLoginPassword string = ''

@description('Flag to use Azure API Management to mediate the calls between the Web frontend and the backend API')
param useAPIM bool = false

@description('API Management SKU to use if APIM is enabled')
param apimSku string = 'Consumption'

@description('Id of the user or app to assign application roles')
param principalId string = ''

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

// The application frontend
module web './app/web-appservice-avm.bicep' = {
  name: 'web'
  scope: rg
  params: {
    name: !empty(webServiceName) ? webServiceName : '${abbrs.webSitesAppService}web-${resourceToken}'
    location: location
    tags: tags
    appServicePlanId: appServicePlan.outputs.resourceId
    appInsightResourceId: monitoring.outputs.applicationInsightsResourceId
    linuxFxVersion: 'node|20-lts'
  }
}

// The application backend
module api './app/api-appservice-avm.bicep' = {
  name: 'api'
  scope: rg
  params: {
    name: !empty(apiServiceName) ? apiServiceName : '${abbrs.webSitesAppService}api-${resourceToken}'
    location: location
    tags: tags
    kind: 'app'
    appServicePlanId: appServicePlan.outputs.resourceId
    siteConfig: {
      alwaysOn: true
      linuxFxVersion: 'dotnetcore|8.0'
    }
    appSettings: {
      AZURE_KEY_VAULT_ENDPOINT: keyVault.outputs.uri
      AZURE_COSMOS_DATABASE_NAME: cosmosDb.outputs.databaseName
      AZURE_COSMOS_ENDPOINT: cosmosAccount.outputs.endpoint
      API_ALLOW_ORIGINS: web.outputs.SERVICE_WEB_URI
      SCM_DO_BUILD_DURING_DEPLOYMENT: false
    }
    appInsightResourceId: monitoring.outputs.applicationInsightsResourceId
    allowedOrigins: [ web.outputs.SERVICE_WEB_URI ]
  }
}

// Give the API access to KeyVault
module accessKeyVault 'br/public:avm/res/key-vault/vault:0.5.1' = {
  name: 'accesskeyvault'
  scope: rg
  params: {
    name: keyVault.outputs.name
    enableRbacAuthorization: false
    enableVaultForDeployment: false
    enableVaultForTemplateDeployment: false
    enablePurgeProtection: false
    sku: 'standard'
    accessPolicies: [
      {
        objectId: principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
      {
        objectId: api.outputs.SERVICE_API_IDENTITY_PRINCIPAL_ID
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
  }
}

// WO-7: Cosmos DB account (serverless), NoSQL database, and core collections with AssetID partition strategy
module cosmosAccount './core/database/cosmos-account.bicep' = {
  name: 'cosmos-account'
  scope: rg
  params: {
    accountName: !empty(cosmosAccountName) ? cosmosAccountName : '${abbrs.documentDBDatabaseAccounts}${resourceToken}'
    location: location
    tags: tags
  }
}

module cosmosDb './core/database/cosmos-nosql-db.bicep' = {
  name: 'cosmos-nosql-db'
  scope: rg
  params: {
    cosmosAccountName: cosmosAccount.outputs.name
    databaseName: 'App'
    location: location
    tags: tags
    autoscaleMaxThroughput: 0
  }
}

module containerAssetInventory './core/database/cosmos-container.bicep' = {
  name: 'cosmos-container-asset-inventory'
  scope: rg
  params: {
    cosmosAccountName: cosmosAccount.outputs.name
    cosmosDatabaseName: cosmosDb.outputs.databaseName
    containerName: 'AssetInventory'
    partitionKeyPath: '/AssetID'
    defaultTtlSeconds: -1
    location: location
    tags: tags
    compositeIndexes: [
      [
        { path: '/AssetID', order: 'ascending' }
        , { path: '/LastUpdated', order: 'descending' }
      ]
    ]
  }
}

module containerLicenseAllocations './core/database/cosmos-container.bicep' = {
  name: 'cosmos-container-license-allocations'
  scope: rg
  params: {
    cosmosAccountName: cosmosAccount.outputs.name
    cosmosDatabaseName: cosmosDb.outputs.databaseName
    containerName: 'LicenseAllocations'
    partitionKeyPath: '/AssetID'
    defaultTtlSeconds: -1
    location: location
    tags: tags
  }
}

module containerEvents './core/database/cosmos-container.bicep' = {
  name: 'cosmos-container-events'
  scope: rg
  params: {
    cosmosAccountName: cosmosAccount.outputs.name
    cosmosDatabaseName: cosmosDb.outputs.databaseName
    containerName: 'Events'
    partitionKeyPath: '/AssetID'
    defaultTtlSeconds: 2592000
    location: location
    tags: tags
    compositeIndexes: [
      [
        { path: '/AssetID', order: 'ascending' }
        , { path: '/EventTimestamp', order: 'descending' }
      ]
    ]
  }
}

// Give the API access to Cosmos using a separate role assignment
module apiCosmosRoleAssignment './app/cosmos-role-assignment.bicep' = {
  name: 'api-cosmos-role'
  scope: rg
  params: {
    cosmosAccountName: cosmosAccount.outputs.name
    apiPrincipalId: api.outputs.SERVICE_API_IDENTITY_PRINCIPAL_ID
  }
}

// WO-7: Azure SQL Server and Database for reference tables (Assets, AuditLog, LicenseUtilization)
module sqlServer './core/database/sql-server.bicep' = {
  name: 'sql-server'
  scope: rg
  params: {
    serverName: !empty(sqlServerName) ? sqlServerName : '${abbrs.sqlServers}${resourceToken}'
    location: location
    tags: tags
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorLoginPassword
    minimalTlsVersion: '1.2'
  }
}

module sqlDatabase './core/database/sql-database.bicep' = {
  name: 'sql-database'
  scope: rg
  params: {
    sqlServerName: sqlServer.outputs.name
    databaseName: !empty(sqlDatabaseName) ? sqlDatabaseName : '${abbrs.sqlServersDatabases}${resourceToken}'
    location: location
    tags: tags
    skuTier: 'Standard'
    skuName: 'S0'
    backupRetentionDays: 7
  }
}


// Create an App Service Plan to group applications under the same payment plan and SKU
module appServicePlan 'br/public:avm/res/web/serverfarm:0.1.1' = {
  name: 'appserviceplan'
  scope: rg
  params: {
    name: !empty(appServicePlanName) ? appServicePlanName : '${abbrs.webServerFarms}${resourceToken}'
    sku: {
      name: 'B3'
      tier: 'Basic'
    }
    location: location
    tags: tags
    reserved: true
    kind: 'Linux'
  }
}

// Create a keyvault to store secrets
module keyVault 'br/public:avm/res/key-vault/vault:0.5.1' = {
  name: 'keyvault'
  scope: rg
  params: {
    name: !empty(keyVaultName) ? keyVaultName : '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
    tags: tags
    enableRbacAuthorization: false
    enableVaultForDeployment: false
    enableVaultForTemplateDeployment: false
    enablePurgeProtection: false
    sku: 'standard'
  }
}

// Monitor application with Azure Monitor
module monitoring 'br/public:avm/ptn/azd/monitoring:0.1.0' = {
  name: 'monitoring'
  scope: rg
  params: {
    applicationInsightsName: !empty(applicationInsightsName) ? applicationInsightsName : '${abbrs.insightsComponents}${resourceToken}'
    logAnalyticsName: !empty(logAnalyticsName) ? logAnalyticsName : '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsDashboardName: !empty(applicationInsightsDashboardName) ? applicationInsightsDashboardName : '${abbrs.portalDashboards}${resourceToken}'
    location: location
    tags: tags
  }
}

// Creates Azure API Management (APIM) service to mediate the requests between the frontend and the backend API
module apim 'br/public:avm/res/api-management/service:0.2.0' = if (useAPIM) {
  name: 'apim-deployment'
  scope: rg
  params: {
    name: !empty(apimServiceName) ? apimServiceName : '${abbrs.apiManagementService}${resourceToken}'
    publisherEmail: 'noreply@microsoft.com'
    publisherName: 'n/a'
    location: location
    tags: tags
    sku: apimSku
    skuCount: 0
    zones: []
    customProperties: {}
    loggers: [
      {
        name: 'app-insights-logger'
        credentials: {
          instrumentationKey: monitoring.outputs.applicationInsightsInstrumentationKey
        }
        loggerDescription: 'Logger to Azure Application Insights'
        isBuffered: false
        loggerType: 'applicationInsights'
        targetResourceId: monitoring.outputs.applicationInsightsResourceId
      }
    ]
  }
}

//Configures the API settings for an api app within the Azure API Management (APIM) service.
module apimApi 'br/public:avm/ptn/azd/apim-api:0.1.0' = if (useAPIM) {
  name: 'apim-api-deployment'
  scope: rg
  params: {
    apiBackendUrl: api.outputs.SERVICE_API_URI
    apiDescription: 'Backend API'
    apiDisplayName: 'Backend API'
    apiName: 'api'
    apiPath: 'api'
    name: useAPIM ? useAPIM ? apim.outputs.name : '' : ''
    webFrontendUrl: web.outputs.SERVICE_WEB_URI
    location: location
    apiAppName: api.outputs.SERVICE_API_NAME
  }
}

// Data outputs
output AZURE_COSMOS_ENDPOINT string = cosmosAccount.outputs.endpoint
output AZURE_COSMOS_DATABASE_NAME string = cosmosDb.outputs.databaseName
output AZURE_SQL_SERVER_FQDN string = sqlServer.outputs.fullyQualifiedDomainName
output AZURE_SQL_DATABASE_NAME string = sqlDatabase.outputs.name

// App outputs
output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.applicationInsightsConnectionString
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.outputs.uri
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output API_BASE_URL string = useAPIM ? apimApi.outputs.serviceApiUri : api.outputs.SERVICE_API_URI
output REACT_APP_WEB_BASE_URL string = web.outputs.SERVICE_WEB_URI
output USE_APIM bool = useAPIM
output SERVICE_API_ENDPOINTS array = useAPIM ? [ apimApi.outputs.serviceApiUri, api.outputs.SERVICE_API_URI ]: []
