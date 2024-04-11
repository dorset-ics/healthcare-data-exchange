
namespace Infrastructure.Common.Configuration;

public record MeshConfiguration(
    string Url,
    int MaxChunkSizeInMegabytes,
    MeshAuthConfiguration Authentication)
{
    public const string SectionKey = "Mesh";
}

public record MeshAuthConfiguration(string RootCertificate, string ClientCertificate, string SubCertificate, bool IsEnabled);