# GitHub Workflows

This repository contains two GitHub Workflows:

1. App deploy
   - Build and deploy the web application to existing Azure resources
   - Defined in `/.github/workflows/deploy-app.yml`
2. Infrastructure deploy
   - Deploy Azure resources
   - Defined in `/.github/workflows/deploy-infra.yml`

These two workflows are used to automatically update the app when there are changes to the main branch in respectively the app code or the infrastructure code.

**NOTE: As of 02.01.2025 the infrastructure workflow is not set up properly with Azure and all resources have been deployed manually!**

## App deploy

The application build and deploy workflow performs the following steps:

1. Build
   1. Checkout repository (fetch code)
   2. Set up .NET (Use correct dotnet version)
   3. Build project (`dotnet build`)
   4. Generate EF Core migration script
   5. Build published version (`dotnet publish`)
   6. Upload EF Core migration script for deployment job
   7. Upload Web app artifact for deployment job
2. Deploy
   1. Download Web app artifact
   2. Download EF Core migration script
   3. Run migration script
   4. Do app settings variable substitution
   5. Deploy Web app to Azure App Service

## Infrastructure deploy

This workflow has not yet been competed, but I have got it set up with authentication with a User Assigned Managed Identity in Azure. This was a bit of a hassle to set up here are the steps:

1. Create a *User Assigned Managed Identity*:
   1. Go to you subscription, then show all the resources in that subscription. 
   2. Click "Create" and create a "User Assigned Managed Identity".
   3. Select your subscription, your resource group, region and a name for your managed identity.
2. Give your MI proper access:
   1. Go to role assignments in Access Control in your subscription.
   2. Click on add, and give your UAMI the "Owner" role.
3. Configure *Federated Credentials*  
   *This step will give your GH Workflow permission to log in as your MI.*  
   1. Go back to resources in your subscription and open your MI.
      - You have to go via resources, if not you will go to the Enterprise Application connected to your MI, and not your MI.
   2. Go to "Federated Credentials" under "Settings"
   3. Click "Add" and use the scenario: "Configure a GitHub issued token to impersonate this app and deploy to Azure"
   4. Fill the form:
      - Issuer: Use default value if your repo is on GitHub.
      - Organisation: The organisation you repo is in OR your GH username if the repo is not connected to an organisation.
      - Repository: Your repository name
      - Entity: In what way your workflow should gain permission. Most common here is to use Environment. That way you can have different Managed Identities for each environment.
      - Environment/Branch/Tag: The name of your environment, branch, or tag.
      - Name: Come up with a name for your Federated Credential
   5. Add secrets to your GitHub Repo:
      - If you created an environment in the last go to your repository in GH, then:  
      Settings > Environments > [env name]  
      Then add your secrets as *Environment secrets*
      - If you didn't, go to your repository in GH, then:  
      Settings > Secrets and Variables > Actions  
      Then add your secrets as *Repository secrets*
      - Add the following secrets:
        - AZURE_CLIENT_ID: The Client ID of your managed identity (can also be found as the Application ID in the corresponding Enterprise Application).
        - AZURE_SUBSCRIPTION_ID: The ID of the subscription containing your UAMI.
        - AZURE_TENANT_ID: The Entra ID Tenant conaining your subscription and your UAMI. This can be found by going to "Tenant properties" in the Azure Portal.
   6. To use these secrets and log in to Azure as your MI you can use the base GH Workflow below, which logs in and verifies that you have authenticated with Azure by printing your account info.

```yaml
name: Azure authenticated workflow

on:
  workflow_dispatch:

jobs:
  deploy:
    name: Login and check authentication
    runs-on: ubuntu-latest
    environment: <your-env-name-here>
    steps:
      - uses: actions/checkout@v4
      - name: Azure login
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Azure CLI script
        uses: azure/cli@v2
        with:
          azcliversion: latest
          inlinescript: |
            az account show
```

