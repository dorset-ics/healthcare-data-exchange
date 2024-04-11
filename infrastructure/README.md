# DEX Infrastructure as Code

This repository contains the Terraform code for the DEX infrastructure.
The infrastructure is aligned with the [DEX Infrastructure Design](../docs/design/architecture.md).
The Terraform code can be deployed from your local machine or by triggering the [CD pipeline](../.pipelines/cd-pipeline.yaml).

## Deploying the infrastructure from your local machine

### Prerequisites

* [Terraform](https://www.terraform.io/downloads.html) installed
* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed
* Azure Service Principal with permissions to create resources in the target subscription (see [here](../docs/infrastructure/identity/roles.md) for list of required permissions)
* [WSL](https://learn.microsoft.com/en-us/windows/wsl/install) installed (Windows only)

#### WSL

On Windows, a WSL distribution may have been installed by another application such as Docker. In some cases this distribution may not be suitable to execute the Terraform, and a fresh distribution must be installed and then set as the default.

The following commands may help:

* List the WSL distributions installed: `wsl -l`
* Install a fresh WSL distribution: `wsl --install`
* Set the default WSL distribution: `wsl -s <distribution-name>`

### Authenticate to Azure

Authenticate to Azure with the Azure CLI, using the service principle credentials:

```bash
az login --service-principal -u <service-principal-id> -p <service-principal-secret> --tenant <tenant-id>
```

### Setup the environment

Setup the required environment variables for terraform:

Bash:

```bash
export ARM_SUBSCRIPTION_ID=<subscription-id>
export ARM_CLIENT_ID=<service-principal-id>
export ARM_CLIENT_SECRET=<service-principal-secret>
export ARM_TENANT_ID=<tenant-id>
```

Powershell:

```powershell
$env:ARM_SUBSCRIPTION_ID = "<subscription-id>"
$env:ARM_CLIENT_ID = "<service-principal-id>"
$env:ARM_CLIENT_SECRET = "<service-principal-secret>"
$env:ARM_TENANT_ID = "<tenant-id>"
```

### Prepare variables

Create a file called `<env>.tfvars` within the infrastructure direction, containing the values used by the tf deployment.

For example:

> NOTE: When running locally, set `<env>` to something unique such as your username.

```powershell
env                                 = "<env>"
location                            = "UK South"
log_analytics_sku                   = "PerGB2018"
app_plan_sku                        = "P3v3"
vnet_address_space                  = ["192.168.0.0/16"]
app_gateway_subnet_address_prefixes = ["192.168.0.0/24"]
services_subnet_address_prefixes    = ["192.168.1.0/24"]
app_plan_subnet_address_prefixes    = ["192.168.10.0/24"]
apim_subnet_address_prefixes        = ["192.168.100.0/24"]
image_tag_suffix                    = "latest"
app_registration_owners             = ["<app_registration_owners>"]
azure_cli_client_id                 = "<azure_cli_client_id>"
sp_client_id                        = "<sp_client_id>"
```

For convenience, a tfvars template file can be found in the repository which can be amended and renamed.

### Setup

Terraform state is stored in an Azure storage account. The storage account is created manually and the connection details are provided to Terraform via the `backend-config` parameters.

```bash
terraform init -backend=true -backend-config="resource_group_name=rg-dex" -backend-config="storage_account_name=sadextfstate" -backend-config="container_name=terraform" -backend-config="key=<env>-terraform.tfstate"
```

### Apply

Apply the Terraform code to create the infrastructure. The `-auto-approve` flag is used to automatically approve the plan, you can remove this flag to review the plan before applying.

```bash
terraform apply -auto-approve -var-file="<env>.tfvars"
```

### Destroy

Destroy the infrastructure. The `-auto-approve` flag is used to automatically approve the plan, you can remove this flag to review the plan before applying.

```bash
terraform destroy -auto-approve -var-file="<env>.tfvars"
```