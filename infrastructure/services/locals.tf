locals {
  backend_address_pool_name          = "dex-${var.env}-beap"
  backend_probe_name                 = "dex-${var.env}-apim-probe"
  frontend_port_name                 = "dex-${var.env}-feport"
  frontend_ip_configuration_name     = "dex-${var.env}-feip"
  http_setting_name                  = "dex-${var.env}-be-htst"
  listener_name                      = "dex-${var.env}-httplstn"
  request_routing_rule_name          = "dex-${var.env}-rqrt"
  redirect_configuration_name        = "dex-${var.env}-rdrcfg"
  random_id                          = random_string.random_id.result
  web_app_service_resource_principal = "abfa0a7c-a6b6-4736-8310-5855508787cd"
  ssl_certificate_name               = "appGatewaySslCert"
  use_https                          = var.env == "prd" || var.env == "stg" ? true : false
  env_mapping = {
    "dev" = "Development"
    "prd" = "Production"
    "stg" = "Staging"
  }
}