output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "key_vault_name" {
  value = module.services.key_vault_name
}

output "current_object_id" {
  value = data.azurerm_client_config.current.object_id
}

output "gateway_frontend_address" {
  value = module.services.gateway_frontend_address
}

output "fhir_url" {
  value = module.health-services.fhir_url
}

output "app_registration_uri" {
  value = tolist(azuread_application.app.identifier_uris)[0]
}