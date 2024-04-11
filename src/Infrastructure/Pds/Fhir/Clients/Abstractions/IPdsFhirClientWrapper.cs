using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Infrastructure.Pds.Fhir.Clients.Abstractions;

public interface IPdsFhirClientWrapper
{
    Task<T?> ReadAsync<T>(string resourceLocation) where T : Resource;
    Task<Bundle> SearchAsync<T>(SearchParams searchParams) where T : Resource;
}