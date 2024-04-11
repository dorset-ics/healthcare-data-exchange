using Infrastructure.Ndop.Mesh.Configuration;

namespace Infrastructure.Ndop.Configuration;

public record NdopConfiguration(NdopMeshConfiguration Mesh)
{
    public const string SectionKey = "Ndop";
};