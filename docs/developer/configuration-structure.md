# Configuration Structure

There are five main configuration files in the `api` project. These are:

1. appsettings.json
1. appsettings.Local.json
1. appsettings.Integration.json
1. appsettings.Development.json
1. appsettings.Production.json

The `appsettings.json` file contains the default/common configuration for the application, and the contains placeholders for the environment specific configuration.
The other files are used to override the default configuration based on the environment the application is running in. The `appsettings.Local.json` file is used for local development, and the `appsettings.Integration.json` file is used for integration testing. The `appsettings.Development.json` file is used for development environment, and the `appsettings.Production.json` file is used for production and staging.
The following setting are being overridden by terraform:

- "Pds__Fhir__Authentication__Certificate"
- "DataHubFhirServer__Authentication__Scope"
- "ApplicationInsightsAgent_EXTENSION_VERSION"
- "AzureStorageConnectionString"
- "AzureTableStorageCache__Endpoint"
- "DataHubFhirServer__BaseUrl"
- "DataHubFhirServer__TemplateImage"
- "Ndop__Mesh__MailboxId"
- "Ndop__Mesh__MailboxPassword"
- "Ndop__Mesh__RootCertificate"
- "Ndop__Mesh__ClientCertificate"
- "Ndop__Mesh__SubCertificate"s