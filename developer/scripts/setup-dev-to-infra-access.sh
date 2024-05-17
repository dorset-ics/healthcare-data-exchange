#!/bin/bash

# Setup environment variables
# export CLIENT_ID=<client (service principle) id>
# export SUBSCRIPTION_ID=<subscription id>
# export TENANT_ID=<tenant id>
# export CLIENT_SECRET=<client (service principle) secret>
# export env=<dev/staging/production>
#

waf_policy_name="agw-dex-$env-waf"
rg_name="rg-dex-$env"
app_name="app-dex-$env"

# get local dev machine ip
runner_ip=$(curl -s 'https://api.ipify.org?format=json' | jq -r '.ip')

# login to azure
az login --service-principal --username $CLIENT_ID --password $CLIENT_SECRET --tenant $TENANT_ID
az account set --subscription $SUBSCRIPTION_ID

# add local ip to waf rule
az network application-gateway waf-policy custom-rule match-condition add --policy-name $waf_policy_name --resource-group $rg_name --name AllowAllForTrustedIP --match-variables RemoteAddr --operator IPMatch --values $runner_ip

# add local ip to webapp access restriction to scm site (kudu)
az webapp config access-restriction add -g $rg_name -n $app_name --rule-name local-$env --action Allow --ip-address $runner_ip --priority 3 --scm-site true