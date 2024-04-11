resource "azurerm_public_ip" "agw_pip" {
  name                = "agw-pip-dex-${var.env}"
  resource_group_name = var.resource_group_name
  location            = var.location
  allocation_method   = "Static"
  domain_name_label   = "agw-pip-dex-${var.env}"
  sku                 = "Standard"
}

resource "azurerm_application_gateway" "agw" {
  name                = "agw-dex-${var.env}"
  resource_group_name = var.resource_group_name
  location            = var.location
  firewall_policy_id  = azurerm_web_application_firewall_policy.agw.id

  sku {
    name     = "WAF_v2"
    tier     = "WAF_v2"
    capacity = 1
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.agw-identity.id]
  }

  gateway_ip_configuration {
    name      = "agw-ip-conf-dex-${var.env}"
    subnet_id = var.app_gateway_subnet_id
  }

  frontend_port {
    name = local.frontend_port_name
    port = local.use_https ? 443 : 80
  }

  frontend_ip_configuration {
    name                 = local.frontend_ip_configuration_name
    public_ip_address_id = azurerm_public_ip.agw_pip.id
  }

  backend_address_pool {
    name = local.backend_address_pool_name
    fqdns = [
      module.api_management.api_management_host_name
    ]
  }

  backend_http_settings {
    name                                = local.http_setting_name
    host_name                           = module.api_management.api_management_host_name
    pick_host_name_from_backend_address = false
    port                                = 443
    protocol                            = "Https"
    cookie_based_affinity               = "Disabled"
    request_timeout                     = 60
    probe_name                          = local.backend_probe_name
  }

  http_listener {
    name                           = local.listener_name
    frontend_ip_configuration_name = local.frontend_ip_configuration_name
    frontend_port_name             = local.frontend_port_name
    protocol                       = local.use_https ? "Https" : "Http"
    ssl_certificate_name           = local.use_https ? local.ssl_certificate_name : null
  }

  request_routing_rule {
    name                       = local.request_routing_rule_name
    rule_type                  = "Basic"
    http_listener_name         = local.listener_name
    backend_address_pool_name  = local.backend_address_pool_name
    backend_http_settings_name = local.http_setting_name
    priority                   = 1
  }

  probe {
    protocol                                  = "Https"
    name                                      = local.backend_probe_name
    host                                      = module.api_management.api_management_host_name
    port                                      = 443
    path                                      = "/status-0123456789abcdef"
    interval                                  = 30
    timeout                                   = 120
    unhealthy_threshold                       = 0
    pick_host_name_from_backend_http_settings = false
    minimum_servers                           = 0
  }

  dynamic "ssl_certificate" {
    for_each = local.use_https ? [1] : []
    content {
      name                = local.ssl_certificate_name
      key_vault_secret_id = azurerm_key_vault_secret.dex_certificate_private.id
    }
  }

}

resource "azurerm_monitor_diagnostic_setting" "app_gtw_diagnostic_setting" {
  name                       = "app_gtw_diagnostic_setting"
  target_resource_id         = azurerm_application_gateway.agw.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics.id

  enabled_log {
    category = "ApplicationGatewayAccessLog"
  }

  enabled_log {
    category = "ApplicationGatewayPerformanceLog"
  }

  enabled_log {
    category = "ApplicationGatewayFirewallLog"
  }

  metric {
    category = "AllMetrics"
  }
}