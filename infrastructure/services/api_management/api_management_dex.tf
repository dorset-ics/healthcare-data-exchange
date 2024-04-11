resource "azurerm_api_management_product" "dex" {
  product_id          = "dex-product"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name

  display_name = "DEX"
  description  = "Healthcare Data Exchange"

  subscription_required = false
  published             = true
}

resource "azurerm_api_management_api" "dex" {
  name                = "dex-api"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name

  display_name = "FHIR API"
  revision     = "1"

  subscription_required = false
  protocols             = ["http", "https"]
  path                  = ""
  service_url           = "https://${var.app_host_name}"

  import {
    content_format = "openapi+json"
    content_value  = file("${path.module}/dex-swagger.json")
  }
}

resource "azurerm_api_management_product_api" "dex_dex" {
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name

  product_id = azurerm_api_management_product.dex.product_id
  api_name   = azurerm_api_management_api.dex.name
}

resource "azurerm_api_management_api_policy" "dex" {
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name

  api_name    = azurerm_api_management_api.dex.name
  xml_content = file("${path.module}/policies/dex_policy.xml")
}

resource "azurerm_api_management_api_operation_policy" "dex" {
  for_each = { for op in local.operations : op.operationId => op }

  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  api_name            = azurerm_api_management_api.dex.name

  operation_id = each.value.operationId
  xml_content  = each.value.policy
}

resource "azurerm_api_management_backend" "data_hub" {
  name                = "data-hub"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name

  protocol = "http"
  url      = "https://${var.app_host_name}"
}

resource "azurerm_api_management_backend" "fhir_server" {
  name                = "fhir-server"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name

  protocol = "http"
  url      = var.fhir_url
}

resource "azurerm_api_management_named_value" "dex_app_id" {
  name                = "ApplicationId"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  display_name        = "ApplicationId"
  value               = var.app_registration_id
}

resource "azurerm_api_management_named_value" "dex_app_id_uri" {
  name                = "ApplicationIdUri"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  display_name        = "ApplicationIdUri"
  value               = var.app_registration_uri
}

resource "azurerm_api_management_named_value" "dex_tenant" {
  name                = "TenantId"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  display_name        = "TenantId"
  value               = var.tenant_id
}

resource "azurerm_api_management_named_value" "dex_azure_cli" {
  name                = "AzureCliId"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  display_name        = "AzureCliId"
  value               = var.azure_cli_client_id
}

resource "azurerm_api_management_named_value" "dex_sp_client_id" {
  name                = "SpClientId"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  display_name        = "SpClientId"
  value               = var.sp_client_id
}

resource "azurerm_api_management_named_value" "dex_fhir_url" {
  name                = "FhirUrl"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  display_name        = "FhirUrl"
  value               = var.fhir_url
}