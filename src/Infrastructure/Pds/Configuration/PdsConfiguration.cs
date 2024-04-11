using Infrastructure.Pds.Fhir.Configuration;
using Infrastructure.Pds.Mesh.Configuration;

namespace Infrastructure.Pds.Configuration;

public record PdsConfiguration(PdsFhirConfiguration Fhir, PdsMeshConfiguration Mesh)
{
    public const string SectionKey = "Pds";
};