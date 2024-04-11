locals {
  health_services_ws_name = "hsdex${var.env}"
  fhir_service_name       = "fhirdex${var.env}"
  fhir_url                = "https://hsdex${var.env}-fhirdex${var.env}.fhir.azurehealthcareapis.com"
  authority               = "https://login.microsoftonline.com/${var.tenant_id}"
  fhir_kind               = "fhir-R4"
}