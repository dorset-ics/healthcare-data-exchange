resource "azurerm_container_registry" "acr" {
  name                = "acrdex${var.env}"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  admin_enabled       = true
  sku                 = "Premium"

  identity {
    type = "SystemAssigned"
  }
}