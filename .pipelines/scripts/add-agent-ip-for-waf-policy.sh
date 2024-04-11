#!/bin/bash

# Get the environment parameter
env=$1

# Define the WAF policy name
waf_policy_name="agw-dex-$env-waf"
# Define the resource group name
rg_name="rg-dex-$env"

# Set the Azure subscription
subscriptionId=$(az account show --query 'id' -o tsv)
echo "Selected subscription: $subscriptionId"
az account set --subscription $subscriptionId

waf_policy_exists=$(az network application-gateway waf-policy show --name $waf_policy_name --resource-group $rg_name --query id -o tsv)

if [ -n "$waf_policy_exists" ]; then
    runner_ip=$(curl -s 'https://api.ipify.org?format=json' | jq -r '.ip')

    # Create the custom rule
    az network application-gateway waf-policy custom-rule create --policy-name $waf_policy_name --resource-group $rg_name --name AllowRunnerIP --priority 3 --rule-type MatchRule --action Allow

    # Add the match condition to the custom rule
    az network application-gateway waf-policy custom-rule match-condition add --policy-name $waf_policy_name --resource-group $rg_name --name AllowRunnerIP --match-variables RemoteAddr --operator IPMatch --values $runner_ip

    echo "Added runner IP $runner_ip to WAF policy $waf_policy_name"
else
    echo "WAF policy $waf_policy_name does not exist."
fi