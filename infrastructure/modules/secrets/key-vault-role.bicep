param keyVaultName string
param principalType string = 'ServicePrincipal'
param roleDefinitionId string = '4633458b-17de-408a-b874-0445c86b69e6'

param principalIds array

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for principalId in principalIds: {
    scope: keyVault
    name: guid(keyVault.id, principalId, roleDefinitionId)
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
      principalId: principalId
      principalType: principalType
    }
  }
]

