# Mesh

The platform uses Mesh for a number of purposes such as extract of PDS bulk data and National Data Opt Out flags.

This article describes how the platform uses Mesh and the steps required to setup and configure a new workflow.

[See this link for full details about Mesh.](https://digital.nhs.uk/services/message-exchange-for-social-care-and-health-mesh)

## Mesh Api

The platform integrates with the Mesh Api which has a number of capabilities to send, track and receive messages

[See this link for full details about the Mesh Api.](https://digital.nhs.uk/developer/api-catalogue/message-exchange-for-social-care-and-health-api)

### NEL Mesh Client

The solution uses the open source North East London Mesh client - a dotnet sdk to facilitate connecting and interacting with Mesh.

[See this link to view the NEL Mesh client on github.](https://github.com/NHSISL/MeshClient)

## Mailboxes and Workflows

All interactions with Mesh require a mailbox which consists of an inbox and an outbox. Messages are sent by uploading messages to the outbox, and retrieved by downloading messages from the inbox.

Workflows define the type of data which will sent and received - such as a PDS bulk trace request, or an NDOP consent extract.

[See this link for details about the available workflows.](https://digital.nhs.uk/services/message-exchange-for-social-care-and-health-mesh/workflow-groups-and-workflow-ids)

It's best practice to have a single workflow per mailbox, therefore a new mailbox should be created when setting up a new workflow.

[See this link to access a form to apply for a new Mesh mailbox.](https://digital.nhs.uk/services/message-exchange-for-social-care-and-health-mesh/messaging-exchange-for-social-care-and-health-apply-for-a-mailbox)

## Certificates

A certificate is required for each Mesh mailbox within an environment. They are supplied by NHS Digital, and requested by sending a certificate signing request (CSR) to the service desk.

A mailbox must be created before requesting a certificate as the mailbox ID must be specified when generating a CSR.

:warning: Be aware **Mesh certificates expire after 3 years** and must be renewed periodically.

### How to Request a Mesh Certificate

> You will need to specify a keystore password that will be used throughout the process.

#### Pre-Requisites

1. Java Development Kit [https://openjdk.org/](https://openjdk.org/)
1. OpenSSL [https://www.openssl.org/](https://www.openssl.org/)

#### Steps

1. Generate a keystore for a mailbox:

   `keytool -genkey -alias Meshclient -keyalg RSA -keysize 2048 -keystore C:\temp\<mailbox-id>.keystore -dname "CN=<mailbox-id>.<ods-code>.api.Mesh-client.nhs.uk"`

1. Create a CSR from the keystore

   `keytool -certreq -alias Meshclient -keystore c:\temp\<mailbox-id>.keystore -file c:\temp\<mailbox-id>.csr`

1. Send the CSR via email to NHS Digital service desk

   * For the Mesh test environment email: `support.digitalservices@nhs.net`
   * For the Mesh prod environment email: `ssd.nationalservicedesk@nhs.net`
   * In the body of the email, provide the following information:
     * Name
     * Organisation
     * Mailbox ID
     * The certificate Common Name (CN): `<mailbox-id>.<ods-code>.api.Mesh-client.nhs.uk`
     * Reason for the Certificate (such as, New Mesh client, Current certificate has or is about to expire)
   * When a response has been received, save the certificate into `c:\temp\<mailbox-id.crt`

1. Export private key from keystore and convert it to an unencrypted PEM key

   `keytool -importkeystore -srckeystore C:\temp\<mailbox-id>.keystore -destkeystore C:\temp\<mailbox-id>.p12 -deststoretype PKCS12 -srcalias Meshclient`

   `openssl pkcs12 -in C:\temp\<mailbox-id>.p12" -nodes -nocerts -out C:\temp\<mailbox-id>-private-key.pem`

1. Create a PFX file containing the certificate and the private key

   `openssl pkcs12 -in c:\temp\<mailbox-id>.crt -inkey c:\temp\<mailbox-id>-private-key.pem -export -out c:\temp\<mailbox-id>.pfx`

1. In some cases you may to convert the PFX to a base 64 string (e.g. to use within as a configuration value in appsettings), in which case PowerShell can be used

   `$fileContentBytes = get-content C:\temp\<mailbox-id>.pfx -Encoding Byte[System.Convert]::ToBase64String($fileContentBytes) | Out-File c:\temp\<mailbox-id>-pfx.txt`

1. Store the PFX file and keystore password in Azure Key Vault
