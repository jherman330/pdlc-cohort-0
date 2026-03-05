// WO-7: Cosmos DB container with partition key /AssetID, optional TTL, and composite indexes.
param cosmosAccountName string
param cosmosDatabaseName string
param containerName string
param partitionKeyPath string = '/AssetID'
param defaultTtlSeconds int = -1
param location string = resourceGroup().location
param tags object = {}
@description('Composite index definitions for query patterns, e.g. [ [ { path: \'/AssetID\', order: \'ascending\' }, { path: \'/Timestamp\', order: \'descending\' } ] ]')
param compositeIndexes array = []

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: cosmosAccountName
}

resource sqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' existing = {
  name: cosmosDatabaseName
  parent: cosmosAccount
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: sqlDatabase
  name: containerName
  location: location
  tags: tags
  properties: {
    options: {}
    resource: {
      id: containerName
      partitionKey: {
        kind: 'Hash'
        paths: [
          partitionKeyPath
        ]
      }
      defaultTtl: defaultTtlSeconds > 0 ? defaultTtlSeconds : null
      indexingPolicy: length(compositeIndexes) > 0 ? {
        indexingMode: 'consistent'
        automatic: true
        compositeIndexes: compositeIndexes
      } : {
        indexingMode: 'consistent'
        automatic: true
      }
    }
  }
}
