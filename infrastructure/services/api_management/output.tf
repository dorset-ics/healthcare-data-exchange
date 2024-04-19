output "api_management_host_name" {
  value = "${azurerm_api_management.apim.name}.azure-api.net"
}

output "api_management_system_assigned_identity" {
  value = azurerm_api_management.apim.identity[0].principal_id
}