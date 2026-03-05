// WO-7: Azure SQL Database with DTU/vCore, service tier (Standard), max size, zone redundancy, short-term backup retention.
param sqlServerName string
param databaseName string
param location string = resourceGroup().location
param tags object = {}
@description('SKU tier, e.g. Standard, Basic')
param skuTier string = 'Standard'
@description('SKU name, e.g. S0, Basic')
param skuName string = 'S0'
@description('Max size in bytes (e.g. 268435456000 for 250 GB)')
param maxSizeBytes int = 268435456000
@description('Zone redundant replica')
param zoneRedundant bool = false
@description('Short-term backup retention in days (PITR)')
param backupRetentionDays int = 7

resource sqlServer 'Microsoft.Sql/servers@2024-11-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2024-11-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    createMode: 'Default'
    maxSizeBytes: maxSizeBytes
    zoneRedundant: zoneRedundant
    requestedBackupStorageRedundancy: 'Local'
  }
}

resource backupPolicy 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2024-11-01-preview' = {
  parent: sqlDatabase
  name: 'default'
  properties: {
    retentionDays: backupRetentionDays
  }
}

output name string = sqlDatabase.name
output resourceId string = sqlDatabase.id
