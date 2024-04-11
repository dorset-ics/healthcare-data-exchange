#!/bin/bash

# Get the artifact path parameter
artifact_path=$1

dotnet tool install -g Swashbuckle.AspNetCore.Cli
dotnet build HealthcareDataExchange.sln -c Release
cp -r src/Api/bin/Release/net8.0/* $artifact_path
cd $artifact_path
swagger tofile --output dex-swagger.json Api.dll v1