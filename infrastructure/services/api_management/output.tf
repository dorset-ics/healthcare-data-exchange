output "api_management_host_name" {
  value = "${azurerm_api_management.apim.name}.azure-api.net"
}