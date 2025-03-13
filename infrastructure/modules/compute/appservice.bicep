param location string = resourceGroup().location
param appName string
param keyVaultName string
param appSettings array = []
param serverFarmId string
param linuxFxVersion string = 'DOTNETCORE|9.0' // Default to .NET 9, but can be overridden

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: serverFarmId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      appSettings: concat(
        [
          { 
            name: 'KeyVaultName'
            value: keyVaultName
          }
        ],
        appSettings
      )
    }
  }
}

resource webAppConfig 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'web'
  properties: {
    scmType: 'GitHub'
  }
}

output appServiceId string = webApp.id
output principalId string = webApp.identity.principalId
