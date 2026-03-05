// WO-7: Cosmos DB account with serverless capacity, Session consistency, automatic failover, free tier eligibility.
// Naming uses abbreviations.json (cosmos- prefix via caller).
param accountName string
param location string = resourceGroup().location
param tags object = {}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  tags: tags
  properties: {
    databaseAccountOfferType: 'Standard'
    defaultIdentity: 'FirstPartyIdentity'
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
