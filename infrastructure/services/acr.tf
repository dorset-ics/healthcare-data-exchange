resource "azurerm_container_registry" "acr" {
  name                = "acrdex${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name
  admin_enabled       = true
  sku                 = "Premium"

  identity {
    type = "SystemAssigned"
  }

}