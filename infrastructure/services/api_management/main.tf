resource "azurerm_api_management" "apim" {
  name                = local.apim_name
  resource_group_name = var.resource_group_name
  location            = var.location

  publisher_name  = "Dorset Health"
  publisher_email = "email@email.com"

  sku_name = local.apim_sku_name

  public_ip_address_id          = azurerm_public_ip.apim.id
  public_network_access_enabled = true

  virtual_network_type = "Internal"
  virtual_network_configuration {
    subnet_id = var.apim_subnet_id
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_monitor_diagnostic_setting" "apim_diagnostic_setting" {
  name                       = "apim_diagnostic_setting"
  target_resource_id         = azurerm_api_management.apim.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "GatewayLogs"
  }

  enabled_log {
    category = "WebSocketConnectionLogs"
  }

  metric {
    category = "AllMetrics"
  }
}