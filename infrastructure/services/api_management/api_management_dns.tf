resource "azurerm_private_dns_zone" "apim_dns_zone" {
  name                = "azure-api.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_a_record" "apim_dns_zone" {
  name                = local.apim_name
  zone_name           = azurerm_private_dns_zone.apim_dns_zone.name
  resource_group_name = var.resource_group_name

  ttl     = 36000
  records = [azurerm_api_management.apim.private_ip_addresses[0]]
}

resource "azurerm_private_dns_zone_virtual_network_link" "apim_dns_zone" {
  name                  = "dns-link-dex-${var.env}-apim"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.apim_dns_zone.name
  virtual_network_id    = var.vnet_id
}