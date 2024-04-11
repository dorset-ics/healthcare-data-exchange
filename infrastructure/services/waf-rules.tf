## WAF policy for App Gateway
resource "azurerm_web_application_firewall_policy" "agw" {
  name                = "agw-dex-${var.env}-waf"
  resource_group_name = var.resource_group_name
  location            = var.location

  policy_settings {
    enabled            = true
    mode               = "Prevention"
    request_body_check = false
  }

  managed_rules {
    managed_rule_set {
      type    = "OWASP"
      version = "3.2"
    }
  }
}

