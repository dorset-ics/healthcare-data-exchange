terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.109.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "3.6.2"
    }
  }

  backend "azurerm" {}
}

provider "azurerm" {
  features {}
}

module "terraform_azurerm_environment_configuration" {
  source          = "git::https://github.com/microsoft/terraform-azurerm-environment-configuration.git?ref=0.4.1"
  arm_environment = "public"
}

data "azurerm_client_config" "current" {}

data "external" "agent_ip" {
  program = ["bash", "-c", "curl -s 'https://api.ipify.org?format=json'"]
}

resource "azurerm_resource_group" "rg" {
  location = var.location
  name     = "rg-dex-${var.env}"
}

module "network" {
  source                              = "./network"
  location                            = var.location
  resource_group_name                 = azurerm_resource_group.rg.name
  vnet_address_space                  = var.vnet_address_space
  app_gateway_subnet_address_prefixes = var.app_gateway_subnet_address_prefixes
  services_subnet_address_prefixes    = var.services_subnet_address_prefixes
  app_plan_subnet_address_prefixes    = var.app_plan_subnet_address_prefixes
  apim_subnet_address_prefixes        = var.apim_subnet_address_prefixes
  env                                 = var.env
}

module "services" {
  source                           = "./services"
  location                         = var.location
  resource_group_name              = azurerm_resource_group.rg.name
  vnet_id                          = module.network.core_vnet_id
  app_plan_subnet_id               = module.network.app_plan_subnet_id
  app_gateway_subnet_id            = module.network.app_gateway_subnet_id
  services_subnet_id               = module.network.services_subnet_id
  apim_subnet_id                   = module.network.apim_subnet_id
  env                              = var.env
  log_analytics_sku                = var.log_analytics_sku
  app_plan_sku                     = var.app_plan_sku
  runner_ip                        = var.runner_ip == "" ? [chomp(data.external.agent_ip.result.ip)] : [var.runner_ip]
  app_zone_id                      = module.network.app_service_dns_zone_id
  app_registration_id              = azuread_application.app.client_id
  app_registration_uri             = tolist(azuread_application.app.identifier_uris)[0]
  image_tag_suffix                 = var.image_tag_suffix
  fhir_url                         = module.health-services.fhir_url
  current_object_id                = data.azurerm_client_config.current.object_id
  app_insights_instrumentation_key = module.services.app_insights_instrumentation_key
  vault_zone_id                    = module.network.kv_dns_zone_id
  storage_zone_id                  = module.network.storage_dns_zone_id
  tenant_id                        = data.azurerm_client_config.current.tenant_id
  azure_cli_client_id              = var.azure_cli_client_id
  sp_client_id                     = var.sp_client_id
  health_services_principal_id     = module.health-services.health_services_principal_id
  acr_id                           = azurerm_container_registry.acr.id
  acr_login_server                 = azurerm_container_registry.acr.login_server
}

module "health-services" {
  source                                  = "./health-services"
  location                                = var.location
  resource_group_name                     = azurerm_resource_group.rg.name
  services_subnet_id                      = module.network.services_subnet_id
  env                                     = var.env
  health_zone_id                          = module.network.health_dns_zone_id
  tenant_id                               = data.azurerm_client_config.current.tenant_id
  log_analytics_workspace_id              = module.services.log_analytics_workspace_id
  web_app_system_assigned_identity        = module.services.web_app_system_assigned_identity
  api_management_system_assigned_identity = module.services.api_management_system_assigned_identity
  oci_artifact_login_server               = azurerm_container_registry.acr.login_server
}
