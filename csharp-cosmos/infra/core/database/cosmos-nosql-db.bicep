// WO-7: Cosmos DB NoSQL database. Serverless account: no throughput; provisioned: use autoscaleMaxThroughput.
param cosmosAccountName string
param databaseName string
param location string = resourceGroup().location
param tags object = {}
@description('For non-serverless accounts, max throughput for database-level autoscale. Omit or 0 for serverless.')
param autoscaleMaxThroughput int = 0

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: cosmosAccountName
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: databaseName
  location: location
  tags: tags
  properties: {
    resource: {
      id: databaseName
    }
    options: autoscaleMaxThroughput > 0 ? {
      autoscaleSettings: {
        maxThroughput: autoscaleMaxThroughput
      }
    } : {}
  }
}

output databaseName string = databaseName
