# Setup web app
resource "azurerm_service_plan" "app_service_plan" {
  name                = "asp-dex-${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.app_plan_sku
}

resource "azurerm_linux_web_app" "web_app" {
  name                          = "app-dex-${var.env}"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  service_plan_id               = azurerm_service_plan.app_service_plan.id
  public_network_access_enabled = false
  https_only                    = true

  site_config {
    container_registry_use_managed_identity       = true
    container_registry_managed_identity_client_id = azurerm_user_assigned_identity.identity.client_id
    always_on                                     = true

    application_stack {
      docker_image_name   = "${azurerm_container_registry.acr.login_server}/api:${var.image_tag_suffix}"
      docker_registry_url = "https://${azurerm_container_registry.acr.login_server}"
    }
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                    = azurerm_application_insights.app_insights.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING"             = azurerm_application_insights.app_insights.connection_string
    "ASPNETCORE_ENVIRONMENT"                            = lookup(local.env_mapping, var.env, "Development")
    "Pds__Fhir__Authentication__UseCertificateStore"    = true
    "Pds__Fhir__Authentication__CertificateThumbprint"  = azurerm_key_vault_certificate.pds_fhir_certificate_private.thumbprint
    "DataHubFhirServer__Authentication__Scope"          = "${var.fhir_url}/.default"
    "ASPNETCORE_URLS"                                   = "http://+:80"
    "ApplicationInsightsAgent_EXTENSION_VERSION"        = "~3"
    "AzureStorageConnectionString"                      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.azure_storage_connection_string.id})"
    "AzureTableStorageCache__Endpoint"                  = azurerm_storage_account.dex_storage_account.primary_table_endpoint
    "DataHubFhirServer__BaseUrl"                        = var.fhir_url
    "DataHubFhirServer__TemplateImage"                  = "${azurerm_container_registry.acr.login_server}/api:${var.image_tag_suffix}"
    "Mesh__Authentication__RootCertificate"             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.nhs_root_certificate.id})"
    "Mesh__Authentication__ClientCertificate"           = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.ndop_mesh_client_certificate_private.id})"
    "Mesh__Authentication__SubCertificate"              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.nhs_sub_certificate.id})"
    "Ndop__Mesh__MailboxId"                             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.ndop_mesh_mailbox_id.id})"
    "Ndop__Mesh__MailboxPassword"                       = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.ndop_mesh_mailbox_password.id})"
    "Pds__Mesh__MailboxId"                              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.pds_mesh_mailbox_id.id})"
    "Pds__Mesh__MailboxPassword"                        = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.pds_mesh_mailbox_password.id})"
  }

  key_vault_reference_identity_id = azurerm_user_assigned_identity.identity.id
  virtual_network_subnet_id       = var.app_plan_subnet_id

  identity {
    type         = "SystemAssigned, UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.identity.id]
  }

  logs {
    application_logs {
      file_system_level = "Information"
    }

    http_logs {
      file_system {
        retention_in_days = 7
        retention_in_mb   = 100
      }
    }
  }

  depends_on = [
    azurerm_key_vault.kv
  ]
}

# Setup private-link for web app
resource "azurerm_private_endpoint" "web-app-endpoint" {
  name                = "pe-dex-${var.env}-app"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.services_subnet_id

  private_dns_zone_group {
    name                 = "private-dns-zone-group"
    private_dns_zone_ids = [var.app_zone_id]
  }

  private_service_connection {
    name                           = "psc-dex-${var.env}-app"
    private_connection_resource_id = azurerm_linux_web_app.web_app.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

resource "azurerm_monitor_diagnostic_setting" "web_app_diagnostic_setting" {
  name                       = "web_app_diagnostic_setting"
  target_resource_id         = azurerm_linux_web_app.web_app.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics.id

  enabled_log {
    category = "AppServiceHTTPLogs"
  }

  enabled_log {
    category = "AppServiceConsoleLogs"
  }

  enabled_log {
    category = "AppServiceAppLogs"
  }

  enabled_log {
    category = "AppServiceAuditLogs"
  }

  enabled_log {
    category = "AppServiceIPSecAuditLogs"
  }

  enabled_log {
    category = "AppServicePlatformLogs"
  }

  metric {
    category = "AllMetrics"
  }
}
