// WO-8: RSA key in Key Vault for SQL TDE BYOK or Cosmos DB CMK.
param vaultName string
param keyName string
param tags object = {}

resource vault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: vaultName
}

resource kvKey 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  parent: vault
  name: keyName
  tags: tags
  properties: {
    attributes: {
      enabled: true
    }
    kty: 'RSA'
    keySize: 2048
    keyOps: [
      'encrypt'
      'decrypt'
      'wrapKey'
      'unwrapKey'
    ]
  }
}

@description('Versioned key URI for CMK / TDE (includes key version).')
output keyUriWithVersion string = kvKey.properties.keyUriWithVersion
