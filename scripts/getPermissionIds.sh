#!/bin/bash

# Step 1: Login to Azure CLI
#az login

# Step 2: Get the service principal ID for Microsoft Graph
graphSpId=$(az ad sp list --display-name "Microsoft Graph" --query "[0].appId" --output tsv)

# Step 3: Query the OAuth2 permissions
permissions=$(az ad sp show --id $graphSpId --query "oauth2Permissions" --output json)

# Step 4: Filter the permissions for `openid` and `profile`
echo $permissions | jq '.[] | select(.value == "openid" or .value == "profile") | {id, value}'