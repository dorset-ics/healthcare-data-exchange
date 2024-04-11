using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.Common.Utilities;

public class CertificateUtilities
{
    public static X509Certificate2 ParseCertificate(string certificateValue)
    {
        try
        {
            return new X509Certificate2(Convert.FromBase64String(
                certificateValue
                    .Replace("-----BEGIN CERTIFICATE-----", "")
                    .Replace("-----END CERTIFICATE-----", "")
                    .Replace("\n", "")
                    .Replace("\r", "")));
        }
        catch (Exception e)
        {
            throw new ArgumentException("Error parsing certificate value for NDOP MESH configuration", e);
        }
    }
}