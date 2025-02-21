param keyVaultName string
param principalType string = 'ServicePrincipal'
param roleDefinitionId string = 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
param principalIds array

resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' existing = {
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

