using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Infrastructure.Pds.Fhir.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Common.Authentication;

public class JwtHandler(PdsAuthConfiguration authConfig)
{
    public string GenerateJwt()
    {
        var signingCredentials = GetSigningCredentials();
        var now = DateTime.UtcNow;
        var header = new JwtHeader(signingCredentials);
        var claims = new List<Claim> { new("jti", Guid.NewGuid().ToString()), new(JwtRegisteredClaimNames.Sub, authConfig.ClientId) };

        var token = new JwtSecurityToken(header,
            new JwtPayload(
                authConfig.ClientId,
                $"{authConfig.TokenUrl}/oauth2/token",
                claims,
                now,
                issuedAt: now,
                expires: now.AddMinutes(5))
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private SigningCredentials GetSigningCredentials()
    {
        return authConfig.UseCertificateStore && authConfig.CertificateThumbprint != null
            ? GetSigningCredentialsFromStore(authConfig.CertificateThumbprint, authConfig.Kid)
            : GetSigningCredentialsFromConfig(authConfig.Kid);
    }

    private SigningCredentials GetSigningCredentialsFromStore(string thumbprint, string kid)
    {
        var privateCertsPath = Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_CERTS_PATH");
        var certificatePath = Path.Join(privateCertsPath, $"{thumbprint}.p12");

        var certificateInBytes = File.ReadAllBytes(certificatePath);
        var cert = new X509Certificate2(bytes);

        return new SigningCredentials(
            new X509SecurityKey(cert, kid),
            SecurityAlgorithms.RsaSha512
        );
    }

    private SigningCredentials GetSigningCredentialsFromConfig(string kid)
    {
        var rsa = GenerateRsaFromPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = kid };

        return new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha512) { CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false } };
    }

    private RSA GenerateRsaFromPrivateKey()
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(authConfig.Certificate);
        return rsa;
    }
}