// WO-8: Transparent Data Encryption with customer-managed key in Azure Key Vault.
param sqlServerName string
@description('Full versioned URI of the Key Vault key (e.g. .../keys/name/version).')
param azureKeyVaultKeyUri string
@description('Logical name for the server key resource.')
param serverKeyName string = 'kv-tde-key'

resource sqlServer 'Microsoft.Sql/servers@2024-11-01-preview' existing = {
  name: sqlServerName
}

resource sqlServerKey 'Microsoft.Sql/servers/keys@2024-11-01-preview' = {
  parent: sqlServer
  name: serverKeyName
  properties: {
    serverKeyType: 'AzureKeyVault'
    uri: azureKeyVaultKeyUri
  }
}

resource encryptionProtector 'Microsoft.Sql/servers/encryptionProtector@2024-11-01-preview' = {
  parent: sqlServer
  name: 'current'
  properties: {
    serverKeyName: sqlServerKey.name
    serverKeyType: 'AzureKeyVault'
    autoRotationEnabled: false
  }
}
