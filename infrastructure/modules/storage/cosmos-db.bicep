param name string
param location string
param kind string
param databaseName string
param locationName string
param keyVaultName string
param allowedIpAddresses array = [
  '161.69.65.54'
  '88.196.181.157'
  '20.105.216.48'
]

param containers array = [
  {
    name: 'items'
    partitionKey: '/PartitionKey'
  }
  {
    name: 'byUser'
    partitionKey: '/PartitionKey'
  }
]

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: name
  location: location
  kind: kind
  properties: {
    ipRules: [for ip in allowedIpAddresses: {
      ipAddressOrRange: ip
    }]
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: locationName
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    isVirtualNetworkFilterEnabled: true
    publicNetworkAccess: 'Enabled'
    networkAclBypass: 'AzureServices'
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosDbAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource cosmosDbContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [
  for container in containers: {
    parent: cosmosDbDatabase
    name: container.name
    properties: {
      resource: {
        id: container.name
        partitionKey: {
          paths: [
            container.partitionKey
          ]
          kind: 'Hash'
        }
        indexingPolicy: {
          automatic: true
          indexingMode: 'consistent'
          includedPaths: [
            {
              path: '/*'
            }
          ]
          excludedPaths: [
            {
              path: '/"_etag"/?'
            }
          ]
        }
        defaultTtl: -1
      }
    }
  }
]

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource cosmosDbConnectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'CosmosDb__ConnectionString'
  properties: {
    value: cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString
  }
}

resource cosmosDbPrimaryKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'CosmosDb__PrimaryKey'
  properties: {
    value: cosmosDbAccount.listKeys().primaryMasterKey
  }
}

resource cosmosDbEndpoint 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'CosmosDb__Endpoint'
  properties: {
    value: cosmosDbAccount.properties.documentEndpoint
  }
}

output cosmosDbId string = cosmosDbAccount.id
// output cosmosDbName string = cosmosDbAccount.name
// output cosmosDbEndpoint string = cosmosDbAccount.properties.documentEndpoint
