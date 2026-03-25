// WO-7: Cosmos DB account with serverless capacity, Session consistency, automatic failover, free tier eligibility.
// WO-8: Optional customer-managed keys via user-assigned identity and Key Vault key URI.
// Naming uses abbreviations.json (cosmos- prefix via caller).
param accountName string
param location string = resourceGroup().location
param tags object = {}
@description('When true, use userAssignedIdentityResourceId and keyVaultKeyUri for CMK (requires Key Vault soft-delete and purge protection).')
param enableCustomerManagedKey bool = false
@description('Full resource ID of the user-assigned identity used for Key Vault access (required when enableCustomerManagedKey is true).')
param userAssignedIdentityResourceId string = ''
@description('Versioned Key Vault key URI for CMK (required when enableCustomerManagedKey is true).')
param keyVaultKeyUri string = ''

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  tags: tags
  identity: enableCustomerManagedKey ? {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityResourceId}': {}
    }
  } : null
  properties: {
    databaseAccountOfferType: 'Standard'
    defaultIdentity: enableCustomerManagedKey ? 'UserAssignedIdentity' : 'FirstPartyIdentity'
    keyVaultKeyUri: enableCustomerManagedKey ? keyVaultKeyUri : null
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        failoverPriority: 0
        isZoneRedundant: false
        locationName: location
      }
    ]
    enableAutomaticFailover: false
    enableFreeTier: true
    disableLocalAuth: true
    disableKeyBasedMetadataWriteAccess: false
    minimalTlsVersion: 'Tls12'
    publicNetworkAccess: 'Enabled'
    networkAclBypass: 'None'
    networkAclBypassResourceIds: []
    ipRules: []
    isVirtualNetworkFilterEnabled: false
    virtualNetworkRules: []
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Local'
      }
    }
  }
}

output name string = cosmosAccount.name
output endpoint string = cosmosAccount.properties.documentEndpoint
output resourceId string = cosmosAccount.id
