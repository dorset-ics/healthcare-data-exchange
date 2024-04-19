output "fhir_url" {
  value = local.fhir_url
}

output "health_services_principal_id" {
  value = azurerm_healthcare_fhir_service.fhir.identity[0].principal_id
}