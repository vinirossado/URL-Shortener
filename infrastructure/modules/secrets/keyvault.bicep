param vaultName string
param location string = resourceGroup().location

resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  name: vaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    tenantId: subscription().tenantId
    // accessPolicies: [
    //   {
    //     tenantId: subscription().tenantId
    //     objectId: '00000000-0000-0000-0000-000000000000'
    //     permissions: {
    //       keys: [
    //         'get'
    //         'list'
    //       ]
    //       secrets: [
    //         'get'
    //         'list'
    //       ]
    //     }
    //   }
    // ]
  }
}

output id string = keyVault.id
output name string = keyVault.name
 