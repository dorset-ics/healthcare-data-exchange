resource "azurerm_user_assigned_identity" "identity" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "id-dex-${var.env}"
}

resource "azurerm_user_assigned_identity" "agw-identity" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "id-dex-${var.env}-agw"
}

# Grant the azure application service access to our keyvault to read the certificate data
# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_service_certificate#:~:text=If%20using%20key_vault_secret_id,every%20AAD%20Tenant%3A
# https://azure.github.io/AppService/2016/05/24/Deploying-Azure-Web-App-Certificate-through-Key-Vault.html 
data "azuread_service_principal" "MicrosoftWebApp" {
  client_id = local.web_app_service_resource_principal
}