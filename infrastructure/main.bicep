param location string = resourceGroup().location
@secure()
param pgSqlPassword string
var uniqueId = uniqueString(resourceGroup().id)
var keyVaultName = 'kv-${uniqueId}'
// var vnetName = 'vnet-${uniqueId}'
// var apiSubnetName = 'subnet-api-${uniqueId}'

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment' // The name of the module not resource
  params: {
    vaultName: keyVaultName
    location: location

  }
}

module apiService 'modules/compute/appservice.bicep' = {
  name: 'apiDeployment'
  params: {
    appName: 'api-${uniqueId}'
    appServicePlanName: 'plan-api-${uniqueId}'
    location: location
    keyVaultName: keyVault.outputs.name
    appSettings: [
      {
        name: 'DatabaseName'
        value: 'urls'
      }
      {
        name: 'ContainerName'
        value: 'items'
      }
    ]
  }
}

module tokenRangeService 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeServiceDeployment' 
  params: {
    appName: 'token-range-service-${uniqueId}'
    appServicePlanName: 'plan-token-range-${uniqueId}'
    location: location
    keyVaultName: keyVault.outputs.name
    // appSettings: [
    //   {
    //     name: 'DatabaseName'
    //     value: 'urls'
    //   }
    //   {
    //     name: 'ContainerName'
    //     value: 'byUser'
    //   }
    // ]
  }
  dependsOn: [
    keyVault
  ]
}
module postgres 'modules/storage/postgresql.bicep' = {
  name: 'postgresDeployment'
  params: {
    name: 'postgresql-${uniqueString(resourceGroup().id)}'
    location: location
    administratorLogin: 'adminuser'
    administratorPassword: pgSqlPassword
    keyVaultName: keyVault.outputs.name
  }
}

module cosmosDb 'modules/storage/cosmos-db.bicep' = {
  name: 'cosmosDbDeployment'
  params: {
    name: 'cosmos-db-${uniqueId}'
    location: location
    kind: 'GlobalDocumentDB'
    databaseName: 'urls'
    locationName: 'Spain Central'
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
}

module keyVaultRoleAssignment 'modules/secrets/key-vault-role.bicep' = {
  name: 'keyVaultRoleAssignmentDeployment'
  params: {
    keyVaultName: keyVault.outputs.name
    principalIds: [
      apiService.outputs.principalId
      tokenRangeService.outputs.principalId
    ]
  }
  dependsOn:[
      keyVault
  ]
}
