// WO-8: User-assigned managed identity for Cosmos DB customer-managed key access to Key Vault.
param name string
param location string = resourceGroup().location
param tags object = {}

resource umi 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: name
  location: location
  tags: tags
}

output id string = umi.id
output principalId string = umi.properties.principalId
output name string = umi.name
