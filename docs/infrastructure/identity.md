# Identity

The concept of identity enables the platform to recognise an entity which needs to perform a task on the platform. An entity could be could be a real person, another external system, or a component of the platform itself.

An identity can be authenticated (e.g. via a username and password) so the platform knows who the entity is, and authorised so the platform can ensure the entity has the permissions required to perform the task it wants to perform.

An identity will have an account, and the account will be assigned a number of roles which define the permissions it holds to perform tasks.

## Accounts

Accounts fall into two categories:

* An account assigned to a real person, which could be used by an end user to consume data or an administrator to manage the platform.

* An unattended account (a.k.a a service principal), which is not assigned to a real person, but used by another system or component of the platform itself which needs to perform a task.

### User Accounts

User accounts will be assigned for the following purposes:

* DeX Administrators

  Those who administer the platform. This is a highly privileged type of user and will be restricted to just a handful of individuals.

* DeX Developers

  Those who develop on the platform. This is also a privileged role, however restricted to creating, changing and configuring elements of the solution.

* DeX Consumers

  Those who query data from the platform.

### Service Principals

The following service principals have been created for the following purposes:

* `sp-dex-deployment`

  The account used to deploy the platform to Azure including the creation of services and the deployment/configuration of the solution.

  This is a highly privileged account which must be guarded.

* `sp-dex-dev`

  The account used by developers to access resources in Azure that are used during development, such as the development instance of Azure Container Registry.

  This account holds limited permissions and only has access to development related resources/data.

* `sp-dex-provider-<provider-code>`

  Each data provider will have their own account to push data into the platform, which is suffixed with their provider code (in the case of an NHS organisation, this will be their ODS code).

  The provider account will be limited to pushing data into the platform, and further to this will be restricted to specific domains of data, specific data types and specific message types.

## Roles

Roles grant a number of permissions to those which they are assigned and authorise them to perform a task.

There are a number of in-built roles within Azure, however custom roles bespoke to the platform have been created which grant the specific permissions to perform a certain function.

As well as permissions, a role can be associated with a number of claims, which the platform can use to further authorise an action. For example, a user may be in a role that provides the permission to push data to the platform, but the role must authorise a claim to push data with a specific organisation code.

### Custom Roles

This section details the custom roles which have been created and their associated permissions/claims.

#### Dex Administrator

Effectively the same as having the in-built "Owner" role over the Dex Azure subscription.

#### Dex Developer

Effectively the same as having the in-built "Contributor" role over the Dex Azure subscription.

#### Dex Provider

Each provider will have their Dex provider role, suffixed with their provider code (e.g. an ODS code for NHS organisations).

The role will have the permissions to push data to the platform via the $ingest endpoint of the facade application.

The role will have the following claims, which will be specific to the organisation and which data they are permitted to push:

* Organisation Code
* Source Domains (list)
* Input data types (list)

#### Dex Deployment

Contains the permissions required to deploy the Dex solution into a dedicated resource group within the Dex Azure subscription.

## Role Assignments

This section details the roles which have been assigned to certain accounts.

| Account                           | Role(s) (scope -> service) |
|-----------------------------------|--------------------------|
| `sp-dex-dev`                      | - ACR Push (Resource -> acrdexoss)<br>- ACR Pull (Resource -> acrdexoss) |
| `sp-dex-deployment`               | - Key Vault Secrets Officer (Resource -> kv-dex)<br>- Storage Account Key Operator Service Role (Resource -> sadextfstate)<br>- FHIR Data Contributor (Subscription -> Dorset Data Exchange) |
| `sp-dex-provider-<provider-code>` | - Dex Provider (Resource -> app-dex-\<env\>) |
