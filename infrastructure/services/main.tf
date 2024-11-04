terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.8.0"
    }
  }
}

module "api_management" {
  source                           = "./api_management"
  location                         = var.location
  resource_group_name              = var.resource_group_name
  vnet_id                          = var.vnet_id
  apim_subnet_id                   = var.apim_subnet_id
  env                              = var.env
  app_insights_id                  = azurerm_application_insights.app_insights.id
  app_insights_instrumentation_key = azurerm_application_insights.app_insights.instrumentation_key
  app_host_name                    = azurerm_linux_web_app.web_app.default_hostname
  fhir_url                         = var.fhir_url
  app_registration_id              = var.app_registration_id
  app_registration_uri             = var.app_registration_uri
  tenant_id                        = var.tenant_id
  random_id                        = random_string.random_id.result
  log_analytics_workspace_id       = azurerm_log_analytics_workspace.log_analytics.id
  azure_cli_client_id              = var.azure_cli_client_id
  sp_client_id                     = var.sp_client_id
}

resource "random_string" "random_id" {
  length  = 4
  special = false
  upper   = false
}