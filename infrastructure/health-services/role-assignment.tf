resource "azurerm_role_assignment" "fhir_reader_role_assignment_web_app" {
  principal_id         = var.web_app_system_assigned_identity
  role_definition_name = "FHIR Data Contributor"
  scope                = azurerm_healthcare_fhir_service.fhir.id
}

resource "azurerm_role_assignment" "fhir_reader_role_assignment_apim" {
  principal_id         = var.api_management_system_assigned_identity
  role_definition_name = "FHIR Data Contributor"
  scope                = azurerm_healthcare_fhir_service.fhir.id
}
