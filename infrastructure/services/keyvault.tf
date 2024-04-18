resource "azurerm_key_vault" "kv" {
  name                = "kv-dex-${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name
  tenant_id           = var.tenant_id

  sku_name = "standard"
  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    ip_rules       = var.runner_ip
  }
}

resource "azurerm_key_vault_access_policy" "api_user_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = var.tenant_id
  object_id    = azurerm_user_assigned_identity.identity.principal_id

  secret_permissions      = ["Get", "List", "Set", "Delete", "Recover"]
  certificate_permissions = ["Create", "Delete", "Get", "Import", "List", "Purge", "Update"]

  depends_on = [
    azurerm_key_vault_access_policy.terraform_user_access
  ]
}

resource "azurerm_key_vault_access_policy" "agw_user_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = var.tenant_id
  object_id    = azurerm_user_assigned_identity.agw-identity.principal_id

  secret_permissions      = ["Get", "List", "Set", "Delete", "Recover"]
  certificate_permissions = ["Create", "Delete", "Get", "Import", "List", "Purge", "Update"]

  depends_on = [
    azurerm_key_vault_access_policy.terraform_user_access
  ]
}

resource "azurerm_key_vault_access_policy" "terraform_user_access" {
  key_vault_id            = azurerm_key_vault.kv.id
  tenant_id               = var.tenant_id
  object_id               = var.current_object_id
  storage_permissions     = ["Set", "List", "Get"]
  key_permissions         = ["Get", "List", "Update", "Create"]
  secret_permissions      = ["Get", "List", "Set", "Delete", "Purge", "Recover"]
  certificate_permissions = ["Create", "Delete", "Get", "Import", "List", "Purge", "Update"]
}

resource "azurerm_key_vault_access_policy" "keyvault_aad_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = azurerm_user_assigned_identity.identity.tenant_id
  object_id    = data.azuread_service_principal.MicrosoftWebApp.id

  secret_permissions      = ["Get"]
  certificate_permissions = ["Get", "List"]

  depends_on = [
    azurerm_key_vault_access_policy.terraform_user_access
  ]
}

resource "azurerm_private_endpoint" "kv-pe" {
  name                = "pe-dex-${var.env}-kv"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.services_subnet_id

  private_dns_zone_group {
    name                 = "private-dns-zone-group"
    private_dns_zone_ids = [var.vault_zone_id]
  }

  private_service_connection {
    name                           = "psc-dex-${var.env}-kv"
    private_connection_resource_id = azurerm_key_vault.kv.id
    is_manual_connection           = false
    subresource_names              = ["Vault"]
  }
}

data "azurerm_key_vault" "common_kv" {
  name                = "kv-dex"
  resource_group_name = "rg-dex"
}

data "azurerm_key_vault_secret" "pds_fhir_certificate" {
  name         = var.env == "prd" || var.env == "stg" ? "pds-fhir-production-pfx-private" : "pds-fhir-integration-pfx-private"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

data "azurerm_key_vault_secret" "ndop_mesh_mailbox_id" {
  name         = "mesh-ndop-mailbox-id"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

data "azurerm_key_vault_secret" "ndop_mesh_mailbox_password" {
  name         = "mesh-ndop-mailbox-password"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

data "azurerm_key_vault_secret" "nhs_root_certificate" {
  name         = "nhs-integration-env-root-ca-cert"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

data "azurerm_key_vault_secret" "nhs_sub_certificate" {
  name         = "nhs-integration-env-sub-ca-cert"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

data "azurerm_key_vault_secret" "ndop_mesh_client_certificate_private" {
  name         = "mesh-ndop-client-certificate-private"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

data "azurerm_key_vault_secret" "pds_mesh_mailbox_id" {
  name         = "mesh-pds-mailbox-id"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

data "azurerm_key_vault_secret" "pds_mesh_mailbox_password" {
  name         = "mesh-pds-mailbox-password"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

resource "azurerm_key_vault_certificate" "pds_fhir_certificate_private" {
  name         = "pds-fhir-certificate-pfx-private"
  key_vault_id = azurerm_key_vault.kv.id
  certificate {
    contents = data.azurerm_key_vault_secret.pds_fhir_certificate.value
  }

  depends_on = [
    azurerm_key_vault_access_policy.terraform_user_access
  ]
}

resource "azurerm_key_vault_secret" "ndop_mesh_client_certificate_private" {
  name         = "mesh-ndop-client-certificate-private"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.ndop_mesh_client_certificate_private.value

  depends_on = [
    azurerm_key_vault_access_policy.terraform_user_access
  ]
}

resource "azurerm_key_vault_secret" "nhs_root_certificate" {
  name         = "nhs-integration-env-root-ca-cert"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.nhs_root_certificate.value
}

resource "azurerm_key_vault_secret" "ndop_mesh_mailbox_id" {
  name         = "mesh-ndop-mailbox-id"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.ndop_mesh_mailbox_id.value
}

resource "azurerm_key_vault_secret" "ndop_mesh_mailbox_password" {
  name         = "mesh-ndop-mailbox-password"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.ndop_mesh_mailbox_password.value
}

resource "azurerm_key_vault_secret" "pds_mesh_mailbox_id" {
  name         = "mesh-pds-mailbox-id"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.pds_mesh_mailbox_id.value
}

resource "azurerm_key_vault_secret" "pds_mesh_mailbox_password" {
  name         = "mesh-pds-mailbox-password"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.pds_mesh_mailbox_password.value
}

resource "azurerm_key_vault_secret" "nhs_sub_certificate" {
  name         = "nhs-integration-env-sub-ca-cert"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.nhs_sub_certificate.value
}

data "azurerm_key_vault_secret" "dex_certificate" {
  name         = "dex-certificate-private"
  key_vault_id = data.azurerm_key_vault.common_kv.id
}

resource "azurerm_key_vault_secret" "dex_certificate_private" {
  name         = "dex-certificate-private"
  key_vault_id = azurerm_key_vault.kv.id
  value        = data.azurerm_key_vault_secret.dex_certificate.value

  depends_on = [
    azurerm_key_vault_access_policy.terraform_user_access
  ]
}

resource "azurerm_key_vault_secret" "azure_storage_connection_string" {
  name         = "azure-storage-connection-string"
  key_vault_id = azurerm_key_vault.kv.id
  value        = azurerm_storage_account.dex_storage_account.primary_connection_string
}


