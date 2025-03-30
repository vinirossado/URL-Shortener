param location string = resourceGroup().location
var uniqueId = uniqueString(resourceGroup().id)
@secure()
param pgSqlPassword string

var keyVaultName = 'kv-${uniqueId}'
var appServicePlanName = 'plan-api-${uniqueId}' // Define a single App Service Plan

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: keyVaultName
    location: location
  }
}

// Create a single App Service Plan
module appServicePlan 'modules/compute/appserviceplan.bicep' = {
  name: 'appServicePlanDeployment'
  params: {
    appServicePlanName: appServicePlanName
    location: location
  }
}

module cosmosDb 'modules/storage/cosmos-db.bicep' = {
  name: 'cosmosDbDeployment'
  params: {
    name: 'cosmos-database-${uniqueId}'
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

// Then deploy the API service that depends on Cosmos DB
module apiService 'modules/compute/appservice.bicep' = {
  name: 'apiDeployment'
  params: {
    appName: 'api-${uniqueId}'
    serverFarmId: appServicePlan.outputs.id
    location: location
    keyVaultName: keyVaultName
    linuxFxVersion: 'DOTNETCORE|9.0' // .NET 9
    appSettings: [
      {
        name: 'DatabaseName'
        value: 'urls'
      }
      {
        name: 'ContainerName'
        value: 'items'
      }
      {
        name: 'CosmosDB--DatabaseId'
        value: 'urls'
      }
      {
        name: 'CosmosDB--ContainerId'
        value: 'items'
      }
    ]
  }
  dependsOn: [
    cosmosDb
  ]
}

// Deploy Token Range Service to the shared App Service Plan
module tokenRangeService 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeServiceDeployment'
  params: {
    appName: 'token-range-service-${uniqueId}'
    serverFarmId: appServicePlan.outputs.id
    location: location
    keyVaultName: keyVaultName
  }
}

module keyVaultRoleAssignment 'modules/secrets/key-vault-role.bicep' = {
  name: 'keyVaultRoleAssignmentDeployment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      apiService.outputs.principalId
      tokenRangeService.outputs.principalId
      goService.outputs.principalId
    ]
    // Using Key Vault Secrets User built-in role (4633458b-17de-408a-b874-0445c86b69e6)
//     roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6'
  }
  dependsOn: [
    keyVault
    cosmosDb 
  ]
}

// Give Token Range service access to Key Vault secrets
// module tokenRangeKeyVaultAccess 'modules/secrets/key-vault-role.bicep' = {
//   name: 'tokenRangeKeyVaultAccessDeployment'
//   params: {
//     keyVaultName: keyVaultName
//     principalIds: [
//       tokenRangeService.outputs.principalId
//       apiService.outputs.principalId
//     ]
//     // Using Key Vault Secrets User built-in role (4633458b-17de-408a-b874-0445c86b69e6)
//     roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6'
//   }
//   dependsOn: [
//     cosmosDb // Make sure Cosmos DB has created its connection string in Key Vault
//   ]
// }

// Deploy Go hello world service
module goService 'modules/compute/appservice.bicep' = {
  name: 'goServiceDeployment'
  params: {
    appName: 'go-hello-${uniqueId}'
    serverFarmId: appServicePlan.outputs.id
    location: location
    keyVaultName: keyVaultName
    // Use custom Docker container instead of directly using golang image
    linuxFxVersion: '' // Will be set by the deployment
    isContainer: true
    dockerRegistryUrl: 'https://index.docker.io/v1'
    appSettings: [
      {
        name: 'WEBSITES_PORT'
        value: '80'
      }
    ]
  }
}

// Give Go service access to Key Vault secrets
module goKeyVaultAccess 'modules/secrets/key-vault-role.bicep' = {
  name: 'goKeyVaultAccessDeployment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      goService.outputs.principalId
    ]
    // Using Key Vault Secrets User built-in role (4633458b-17de-408a-b874-0445c86b69e6)
    roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6'
  }
  dependsOn: [
    cosmosDb // Make sure Cosmos DB has created its connection string in Key Vault
    apiService
  ]
}

// Move these to the end since they were at the wrong place
module postgres 'modules/storage/postgresql.bicep' = {
  name: 'postgresDeployment'
  params: {
    name: 'postgresql-${uniqueString(resourceGroup().id)}'
    location: location
    administratorLogin: 'adminuser'
    administratorPassword: pgSqlPassword
    keyVaultName: keyVaultName
  }
}
