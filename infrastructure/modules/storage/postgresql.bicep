param name string
param location string
param keyVaultName string
param administratorLogin string
@secure()
param administratorPassword string

resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-11-01-preview' = {
  name: name
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
  resource database 'databases' = {
    name: 'ranges'
  }

  // Uncomment only one of these as needed:
  // Allow only Azure services
  resource firewallRule 'firewallRules' = {
    name: 'allow-azure-services'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }
  
  // For development only - Remove for production
  // resource firewallRulePublicIP 'firewallRules' = {
  //   name: 'allow-public-IPs'
  //   properties: {
  //     startIpAddress: '0.0.0.0'
  //     endIpAddress: '255.255.255.255'
  //   }
  // }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource postgresDbConnectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Postgres--ConnectionString'
  properties: {
    value: 'Server=${postgresqlServer.name}.postgres.database.azure.com;Database=ranges;Port=5432;User Id=${administratorLogin};Password=${administratorPassword};Ssl Mode=Require;' // IMPORTANT: Use an applicaiton user for production
  }
}


output serverId string = postgresqlServer.id
