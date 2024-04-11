#!/bin/bash
# To generate a unique ID for an organisation before submitting it, please see /docs/templates/OrganisationID.md
set -e

fhirServerUrl=$1
fhirServerAuthToken=$2

if [ -n "$fhirServerAuthToken" ]; then
  echo "Using auth token"
  IS_PUBLIC=false
else
  echo "No auth token"
  IS_PUBLIC=true
fi

# Loop over JSON ICS organisations
for json_file in */data/*.json; do
  # Check if the file exist
  if [ -f "$json_file" ]; then
    echo "Processing file: $json_file"

    # Extract the organisation name from the JSON file
    organisation_identifier=$(jq -r '.id' "$json_file")
    echo $(curl -X PUT -H "Content-Type: application/json" -d "@$json_file" "$fhirServerUrl/Organization/$organisation_identifier"  ${IS_PUBLIC:+ -H "authorization: Bearer $fhirServerAuthToken"})

    echo "-----------------------------------------"
  else
    echo "File not found: $json_file"
  fi
done