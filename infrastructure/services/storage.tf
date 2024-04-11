resource "azurerm_storage_account" "dex_storage_account" {
  name                            = lower("sadex${var.env}${local.random_id}")
  resource_group_name             = var.resource_group_name
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  account_kind                    = "StorageV2"
  allow_nested_items_to_be_public = false
}

# Setup private-link for storage account
resource "azurerm_private_endpoint" "storage_endpoint" {
  name                = "pe-dex-${var.env}-storage"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.services_subnet_id

  private_dns_zone_group {
    name                 = "private-dns-zone-group"
    private_dns_zone_ids = [var.storage_zone_id]
  }
  private_service_connection {
    name                           = "psc-dex-${var.env}-storage"
    private_connection_resource_id = azurerm_storage_account.dex_storage_account.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}
