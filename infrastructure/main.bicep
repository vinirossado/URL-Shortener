param location string = resourceGroup().location
@secure()
param pgSqlPassword string
var uniqueId = uniqueString(resourceGroup().id)
var keyVaultName = 'kv-${uniqueId}'
var appServicePlanName = 'plan-api-${uniqueId}' // Definir um único App Service Plan

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: keyVaultName
    location: location
  }
}

module appServicePlan 'modules/compute/appservice-plan.bicep' = {
  name: 'appServicePlanDeployment'
  params: {
    appServicePlanName: appServicePlanName
    location: location
  }
}

module apiService 'modules/compute/appservice.bicep' = {
  name: 'apiDeployment'
  params: {
    appName: 'api-${uniqueId}'
    appServicePlanName: appServicePlanName // Usando o mesmo App Service Plan
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
  dependsOn: [
    appServicePlan
  ]
}

module tokenRangeService 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeServiceDeployment'
  params: {
    appName: 'token-range-service-${uniqueId}'
    appServicePlanName: appServicePlanName // Usando o mesmo App Service Plan
    location: location
    keyVaultName: keyVault.outputs.name
  }
  dependsOn: [
    appServicePlan
  ]
}
