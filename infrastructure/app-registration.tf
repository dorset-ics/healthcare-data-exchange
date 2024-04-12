resource "random_string" "random_id" {
  length  = 4
  special = false
  upper   = false
}

resource "azuread_application" "app" {
  display_name    = "sp-dex-${var.env}"
  identifier_uris = ["api://dex-${var.env}-${random_string.random_id.result}"]
  owners          = var.app_registration_owners

  api {
    mapped_claims_enabled          = true
    requested_access_token_version = 2

    oauth2_permission_scope {
      admin_consent_description  = "Allows the user to call the API."
      admin_consent_display_name = "API.Call"
      enabled                    = true
      id                         = "96183846-204b-4b43-82e1-5d2222eb4b9b"
      type                       = "User"
      user_consent_description   = "API.Call"
      user_consent_display_name  = "API.Call"
      value                      = "API.Call"
    }
  }

  app_role {
    allowed_member_types = ["User", "Application"]
    description          = "DataProviders can send data to the platform"
    display_name         = "DataProvider"
    enabled              = true
    id                   = "1b19509b-32b1-4e9f-b71d-4992aa991967"
    value                = "DataProvider"
  }

  app_role {
    allowed_member_types = ["User", "Application"]
    description          = "DataConsumers can request data from the platform"
    display_name         = "DataConsumer"
    enabled              = true
    id                   = "497406e4-012a-4267-bf18-45a1cb148a01"
    value                = "DataConsumer"
  }

  feature_tags {
    enterprise = true
  }

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "df021288-bdef-4463-88db-98f22de89214" # User.Read.All
      type = "Role"
    }

    resource_access {
      id   = "b4e74841-8e56-480b-be8b-910348b18b4c" # User.ReadWrite
      type = "Scope"
    }
  }
}

resource "azuread_application_pre_authorized" "azcli" {
  application_id       = azuread_application.app.id
  authorized_client_id = var.azure_cli_client_id

  permission_ids = flatten([
    for api in azuread_application.app.api : [
      for scope in api.oauth2_permission_scope : scope.id
    ]
  ])
}

resource "azuread_service_principal" "app" {
  client_id                = azuread_application.app.application_id
  owners                   = var.app_registration_owners
  tags = [
    "AppServiceIntegratedApp",
    "WindowsAzureActiveDirectoryIntegratedApp",
  ]
}
