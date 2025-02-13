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
                         --scopes /subscriptions/{Azure_Subscription_Id} \
                         --sdk-auth
```


### Apply to Custom Contributor Role

```bash
az ad sp create-for-rbac --name "GitHub-Actions-SP" \
                         --role 'infra_deploy' \
                         --scopes /subscriptions/{Azure_Subscription_Id} \
                         --sdk-auth
```
### Perform the Plan (What-If) 

```bash
az deployment group what-if --resource-group urlshortner-dev /
                            --template-file infrastructure/main.bicep
```

### Deploy the changes

```bash
az deployment group create --resource-group urlshortner-dev / 
                           --template-file infrastructure/main.bicep
```
#### Configure a federated identity credential on an app
#### TODO: Add doc for federated identity.
