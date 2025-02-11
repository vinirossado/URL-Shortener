# URL-Shortener

## IaC


### Log in into Azure
```bash
az login
```

### Create Resource Group
```bash
az group create --name urlshortner-dev --location westeurope
```

### Create User for GH Actions
```bash 
az ad sp create-for-rbac --name "GitHub-Actions-SP" \
                         --role contributor \
                         --scopes /subscriptions/5f7636b2-6fe8-4471-b398-e9a55637cc2b \
                         --sdk-auth
```

### Perform the Plan (WhatIf) 

```bash
az deployment group what-if --resource-group urlshortner-dev --template-file infrastructure/main.bicep
```
#### Configure a federated identity credential on an app