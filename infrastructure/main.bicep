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

// Deploy API to the shared App Service Plan
module apiService 'modules/compute/appservice.bicep' = {
  name: 'apiDeployment'
  params: {
    appName: 'api-${uniqueId}'
    serverFarmId: appServicePlan.outputs.id
    location: location
    keyVaultName: keyVault.outputs.vaultName
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
    ]
  }
}

// Deploy Token Range Service to the shared App Service Plan
module tokenRangeService 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeServiceDeployment'
  params: {
    appName: 'token-range-service-${uniqueId}'
    serverFarmId: appServicePlan.outputs.id
    location: location
    keyVaultName: keyVault.outputs.vaultName
  }
}

// Deploy Go hello world service
module goService 'modules/compute/appservice.bicep' = {
  name: 'goServiceDeployment'
  params: {
    appName: 'go-hello-${uniqueId}'
    serverFarmId: appServicePlan.outputs.id
    location: location
    keyVaultName: keyVault.outputs.vaultName
    // Use custom Docker container instead of directly using golang image
    linuxFxVersion: ''  // Will be set by the deployment
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

module postgres 'modules/storage/postgresql.bicep' = {
  name: 'postgresDeployment'
  params: {
    name: 'postgresql-${uniqueString(resourceGroup().id)}'
    location: location
    administratorLogin: 'adminuser'
    administratorPassword: pgSqlPassword
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
}
