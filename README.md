# URL-Shortener

## IaC


### Log in into Azure
```bash
az login
```

### Create Resource Group
```bash
az group create --name urlshortener-dev --location westeurope
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
az ad sp create-for-rbac --name "GitHub-Actions-SP" --role 'infra_deploy' --scopes /subscriptions/{Azure_Subscription_Id} --sdk-auth
```

https://learn.microsoft.com/en-us/azure/role-based-access-control/troubleshooting?tabs=bicep

#### Configure a federated identity credential on an app

https://learn.microsoft.com/en-gb/entra/workload-id/workload-identity-federation-create-trust?pivots=identity-wif-apps-methods-azp#configure-a-federated-identity-credential-on-an-app

## Get Azure Publish Profile

```bash
az webapp deployment list-publishing-profiles --name api-k4mcxdyfbnuxo --resource-group urlshortener-dev --xml
```