param location string = resourceGroup().location
param appServicePlanName string
param appName string
param keyVaultName string
param appSettings array = []

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  sku: {
    tier: 'Basic'
    name: 'B1'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      appSettings: concat(
        [
          {
            name: 'keyVaultName'
            value: keyVaultName
          }
        ],
        appSettings
      )
    }
  }
}

resource webAppConfig 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: webApp
  name: 'web'
  properties: {
    scmType: 'GitHub'
  }
}

output appServiceId string = webApp.id
output principalId string = webApp.identity.principalId
