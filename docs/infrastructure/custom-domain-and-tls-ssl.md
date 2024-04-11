# Custom Domains and TLS/SSL

## Custom Domain

DNS records for DEX are managed by NHS England. See [HSCN Domain Name System (DNS)](https://digital.nhs.uk/services/health-and-social-care-network/hscn-technical-guidance/dns) for an overview of the domain name system on HSCN and the domain name system change request process.

The table below lists the environment-specific domains that have been provisioned by NHS England, and their respective records:

| Environment | Hostname | CNAME |
| ------------| -------- | ----- |
| Dev | dex-dev.nhsdorset.nhs.uk | agw-pip-dex-dev.uksouth.cloudapp.azure.com |
| Staging | dex-staging.nhsdorset.nhs.uk | agw-pip-dex-staging.uksouth.cloudapp.azure.com |
| Production | dex.nhsdorset.nhs.uk | agw-pip-dex-prod.uksouth.cloudapp.azure.com |

> The CNAME record comes from the DNS label attached the App Gateway's public IP address.

## TLS/SSL

TODO: Document process for provisioning, management, and rotating certs.

### Storing the Certificate in Key Vault

When deploying the infrastructure using Terraform, the certificate is copied from the common Key Vault to an environment-specific Vault.

The certificate should be located in the common Key Vault with the following name: "dex-{{env}}-certificate-private".

Follow [this tutorial](https://learn.microsoft.com/en-us/azure/key-vault/certificates/tutorial-import-certificate?tabs=azure-portal) to import a certificate in Key Vault using the portal.

### TLS/SSL in App Service's Container Instance

The DEX API runs as a [custom container](https://learn.microsoft.com/en-us/azure/app-service/quickstart-custom-container) on the Azure App Service infrastructure.

App Service terminates TLS/SSL at the front ends. That means that TLS/SSL requests never get to the app. It is not required, or recommended, to implement any support for TLS/SSL into your app or the container. See [here](https://learn.microsoft.com/en-us/azure/app-service/configure-custom-container#detect-https-session) for details.

## Internal Traffic

Our design requires all internal traffic to be encrypted using TLS/SSL. This is achieved by using Azure managed endpoints for the App Gateway, API Management, and the FHIR Server.

> Transport Layer Security (TLS) is a widely adopted security protocol designed to secure connections and communications between servers and clients.

| Source         | Target         | Approach |
| -------------- | -------------- | -------- |
| App Gateway    | API Management | Gateway targets an Azure managed endpoint (*.azure-api.net). |
| API Management | Web API        | Backend targets an Azure managed endpoint (*.azurewebsites.net). |
| API Management | FHIR Server    | Backend targets an Azure managed endpoint (*.fhir.azurehealthcareapis.com). |

Microsoft is responsible for provisioning, maintaining, and rotating TLS/SSL certificates for Azure managed endpoints.