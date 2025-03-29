param name string
param location string
param kind string
param databaseName string
param locationName string
param keyVaultName string
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

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-08-15' = {
  name: name
  location: location
  kind: kind
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: locationName
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    // Either remove this line if you don't need VNet integration
    isVirtualNetworkFilterEnabled: false
    // Or if you need VNet integration, specify allowed IP ranges or VNets:
    /*
    isVirtualNetworkFilterEnabled: true
    ipRules: [
      {
        ipAddressOrRange: '0.0.0.0/0'  // Allow all IPs - replace with your specific ranges
      }
    ]
    virtualNetworkRules: []  // Add your VNet rules here if needed
    */
    publicNetworkAccess: 'Enabled'  // Make sure public access is enabled
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-08-15' = {
  parent: cosmosDbAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

// Using a more widely supported API version
resource cosmosDbContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-08-15' = [
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

output cosmosDbId string = cosmosDbAccount.id
