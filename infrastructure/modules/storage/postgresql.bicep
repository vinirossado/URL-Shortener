param name string
param location string
param keyVaultName string
param administratorLogin string
@secure()
param administratorPassword string

resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: name
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Bustable'
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

  resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
    name: keyVaultName
  }

  resource postgresqlDbConnectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
    parent: keyVault
    name: 'Postgres--ConnectionString'
    properties: {
      value: '${postgresqlServer.fullyQualifiedDomainName};database=ranges;User Id=${postgresqlServer.properties.administratorLogin};password=${postgresqlServer.properties.administratorLoginPassword}'
    }
  }
}

output serverId string = postgresqlServer.id
