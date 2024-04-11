using System.IdentityModel.Tokens.Jwt;
using System.IO.Abstractions;
using System.Security.Claims;
using System.Security.Cryptography;
using Infrastructure.Pds;
using Infrastructure.Pds.Configuration;
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
        var rsa = GenerateRsaFromPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = authConfig.Kid };

        return new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha512) { CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false } };
    }

    private RSA GenerateRsaFromPrivateKey()
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(authConfig.Certificate);
        return rsa;
    }
}