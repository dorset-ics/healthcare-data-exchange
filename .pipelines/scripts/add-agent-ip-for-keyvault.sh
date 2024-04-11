#!/bin/bash

# Get the environment parameter
env=$1

# Define the key vault name
kv_name="kv-dex-$env"

# Set the Azure subscription
subscriptionId=$(az account show --query 'id' -o tsv)
echo "Selected subscription: $subscriptionId"
az account set --subscription $subscriptionId

kv_exists=$(az keyvault show --name $kv_name --query id -o tsv)

if [ -n "$kv_exists" ]; then
    agent_ip=$(curl -s 'https://api.ipify.org?format=json' | jq -r '.ip')

    az keyvault network-rule add --name $kv_name --ip-address $agent_ip
    echo "Added network rule to key vault $kv_name for agent IP $agent_ip"
else
    echo "Key vault $kv_name does not exist."
fi