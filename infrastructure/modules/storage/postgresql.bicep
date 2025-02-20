param name string
param location string
param keyVaultName string
param administratorLogin string
@secure()
param administratorPassword string

resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
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
      publicNetworkAccess: 'Disabled'
    }
  }
  resource database 'databases' = {
    name: 'ranges'
  }

  resource firewallRule 'firewallRules' = {
    name: 'allow-all-azure-internal-IPs'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }
  resource firewallRulePublicIP 'firewallRules' = {
    name: 'allow-public-IPs'
    properties: {
      startIpAddress: '88.196.181.157'
      endIpAddress: '88.196.181.157'
    }
  }
    resource firewallRulePostgresqlIP 'firewallRules' = {
      name: 'allow-public-postgres-IPs'
      properties: {
        startIpAddress: '104.46.44.181'
        endIpAddress: '104.46.44.181'
      }
    }
}

resource cosmosDbConnectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Postgres--ConnectionString'
  properties: {
    value: 'Server=${postgresqlServer.name}.postgres.database.azure.com;Database=ranges;Port=5432;User Id=${administratorLogin};Password=${administratorPassword};Ssl Mode=Require;' // IMPORTANT: Use an applicaiton user for production
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

output serverId string = postgresqlServer.id
