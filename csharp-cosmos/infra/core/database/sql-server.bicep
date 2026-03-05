// WO-7: Azure SQL Server with Entra authentication, firewall, TLS 1.2 minimum, audit settings.
// Naming uses abbreviations.json (sql- prefix via caller).
param serverName string
param location string = resourceGroup().location
param tags object = {}
param administratorLogin string
@secure()
param administratorLoginPassword string
@description('Optional: Azure AD object ID (sid) and tenant ID for Entra admin. Set both to enable Entra auth.')
param entraAdminObjectId string = ''
param entraAdminTenantId string = ''
@description('Minimum TLS version (e.g. 1.2)')
param minimalTlsVersion string = '1.2'

resource sqlServer 'Microsoft.Sql/servers@2024-11-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    minimalTlsVersion: minimalTlsVersion
    publicNetworkAccess: 'Enabled'
    administrators: (!empty(entraAdminObjectId) && !empty(entraAdminTenantId)) ? {
      administratorType: 'ActiveDirectory'
      login: 'EntraAdmin'
      sid: entraAdminObjectId
      tenantId: entraAdminTenantId
      principalType: 'User'
      azureADOnlyAuthentication: false
    } : null
  }
}

resource auditPolicy 'Microsoft.Sql/servers/auditingSettings@2024-11-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    state: 'Disabled'
    retentionDays: 0
  }
}

resource fwAllowAzure 'Microsoft.Sql/servers/firewallRules@2024-11-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output name string = sqlServer.name
output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
output resourceId string = sqlServer.id
