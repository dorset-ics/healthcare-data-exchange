output "app_insights_instrumentation_key" {
  value = azurerm_application_insights.app_insights.instrumentation_key
}

output "app_insights_app_id" {
  value     = azurerm_application_insights.app_insights.app_id
  sensitive = true
}

output "gateway_frontend_address" {
  value = format(var.env == "prd" || var.env == "stg" ? "https://%s" : "http://%s", azurerm_public_ip.agw_pip.fqdn)
}

output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}

output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.log_analytics.id
}

output "web_app_system_assigned_identity" {
  value = azurerm_linux_web_app.web_app.identity[0].principal_id
}
