# Certificate Preparation

The Healthcare Data Exchange uses signed JWT authentication to access some application-restricted RESTful APIs. Notably the [Personal Demographics Service](./personal-demographics-service.md).

NHS Digital provide extensive [documentation](https://digital.nhs.uk/developer/guides-and-documentation/security-and-authorisation/application-restricted-restful-apis-signed-jwt-authentication) explaining how this integration works.

An important step in this process is generating and signing a JWT. This happens at runtime. The JWT is then used to authenticate with the PDS.

In the `JwtHandler` we generate an instance of `SigningCredentials` using a `X509Certificate2` object. This certificate is stored in Azure Key Vault and loaded into the application container at runtime. Details of this process can be found [here](https://learn.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code).

## Convert certificate to PKCS12 x509 certificate

Before importing the certificate into Azure Key Vault, it must be converted to a PKCS12 x509 certificate.

The guide, provided by NHS Digital, produces a self-signed certificate and a private key in X.509 PEM format. These components can be temporarily stored in `.pem` files and used to create a PKCS12 x509 certificate.

The following commands can be used to convert the certificate and private key to a PKCS12 x509 certificate:

```bash
openssl pkcs12 -export -out certificate.p12 -inkey privatekey.pem -in certificate.pem
```

This command will prompt for a password to secure the PKCS12 x509 certificate. This password will be required when importing the certificate into Azure Key Vault.
