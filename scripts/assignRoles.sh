#!/bin/bash

# Variables
MANAGED_IDENTITY_NAME="your-managed-identity-name"
RESOURCE_GROUP="your-resource-group"
TENANT_ID=$(az account show --query tenantId -o tsv)

# Get the managed identity's object ID
MANAGED_IDENTITY_ID=$(az identity show --name $MANAGED_IDENTITY_NAME --resource-group $RESOURCE_GROUP --query principalId -o tsv)

# Assign the 'profile' and 'openid' permissions
az ad app permission add --id $MANAGED_IDENTITY_ID --api 00000003-0000-0000-c000-000000000000 --api-permissions 14dad69e-099b-42c9-810b-d002981feec1=Scope 37f7f235-527c-4136-accd-4a02d197296e=Scope

# Grant admin consent for the permissions
az ad app permission grant --id $MANAGED_IDENTITY_ID --api 00000003-0000-0000-c000-000000000000

# Admin consent requires admin privileges
az ad app permission admin-consent --id $MANAGED_IDENTITY_ID