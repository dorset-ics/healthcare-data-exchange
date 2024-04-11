resource "azurerm_log_analytics_workspace" "log_analytics" {
  name                = "log-dex-${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.log_analytics_sku
  retention_in_days   = 30
}

resource "azurerm_application_insights" "app_insights" {
  name                = "appi-dex-${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.log_analytics.id
  application_type    = "web"
}