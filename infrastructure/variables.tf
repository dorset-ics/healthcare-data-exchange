variable "env" {
  type = string
}

variable "location" {
  type = string
}

variable "vnet_address_space" {
  type = list(string)
}

variable "app_gateway_subnet_address_prefixes" {
  type = list(string)
}

variable "services_subnet_address_prefixes" {
  type = list(string)
}

variable "app_plan_subnet_address_prefixes" {
  type = list(string)
}

variable "apim_subnet_address_prefixes" {
  type = list(string)
}

variable "runner_ip" {
  type    = string
  default = ""
}

variable "log_analytics_sku" {
  type = string
}

variable "app_plan_sku" {
  type = string
}

variable "image_tag_suffix" {
  type    = string
  default = "latest"
}

variable "app_registration_owners" {
  type = list(string)
}

variable "azure_cli_client_id" {
  type = string
}

variable "sp_client_id" {
  type = string
}