resource "azurerm_public_ip" "apim" {
  name                = local.public_ip_name
  location            = var.location
  resource_group_name = var.resource_group_name
  domain_name_label   = local.apim_dns_label
  allocation_method   = "Static"
  sku                 = "Standard"
}