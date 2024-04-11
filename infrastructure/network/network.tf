resource "azurerm_virtual_network" "core_vnet" {
  name                = "vnet-dex-${var.env}"
  address_space       = var.vnet_address_space
  location            = var.location
  resource_group_name = var.resource_group_name
}

resource "azurerm_subnet" "app_gateway_subnet" {
  name                 = "snet-dex-${var.env}-agw"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.core_vnet.name
  address_prefixes     = var.app_gateway_subnet_address_prefixes
}

resource "azurerm_subnet" "services_subnet" {
  name                 = "snet-dex-${var.env}-services"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.core_vnet.name
  address_prefixes     = var.services_subnet_address_prefixes
}

resource "azurerm_subnet" "app_plan_subnet" {
  name                 = "snet-dex-${var.env}-app-plan"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.core_vnet.name
  address_prefixes     = var.app_plan_subnet_address_prefixes


  delegation {
    name = "delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "apim_subnet" {
  name                 = "snet-dex-${var.env}-apim"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.core_vnet.name
  address_prefixes     = var.apim_subnet_address_prefixes
}