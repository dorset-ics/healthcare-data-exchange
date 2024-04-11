resource "azurerm_role_assignment" "acrpull_role_assignment" {
  principal_id         = azurerm_user_assigned_identity.identity.principal_id
  role_definition_name = "AcrPull"
  scope                = azurerm_container_registry.acr.id
}

resource "azurerm_role_assignment" "storage_table_role_assignment" {
  principal_id         = azurerm_linux_web_app.web_app.identity[0].principal_id
  role_definition_name = "Storage Table Data Contributor"
  scope                = azurerm_storage_account.dex_storage_account.id
}
