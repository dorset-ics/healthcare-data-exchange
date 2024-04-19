locals {
  apim_name      = "apim-dex-${var.env}-${var.random_id}"
  apim_sku       = var.env == "prd" || var.env == "stg" ? "Premium" : "Developer"
  apim_sku_name  = "${local.apim_sku}_${var.apim_instance_count}"
  apim_dns_label = lower(replace(local.apim_name, "-", ""))
  public_ip_name = "apim-ip-dex-${var.env}-${var.random_id}"

  open_api_specification = jsondecode(file("${path.module}/dex-swagger.json"))

  operations = flatten([
    for endpoint in local.open_api_specification.paths : [
      for endpointType in endpoint : {
        operationId = endpointType.operationId,
        policy = templatefile("${path.module}/policies/dex_operation_policy.tftpl", {
          tags     = endpointType.tags,
          fhir_url = var.fhir_url
        })
      }
    ]
  ])
}