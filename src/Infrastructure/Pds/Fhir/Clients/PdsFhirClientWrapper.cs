using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.Pds.Fhir.Clients.Abstractions;

namespace Infrastructure.Pds.Fhir.Clients;

public class PdsFhirClientWrapper(FhirClient fhirClient) : IPdsFhirClientWrapper
{
    public Task<T?> ReadAsync<T>(string resourceLocation) where T : Resource
    {
         return fhirClient.ReadAsync<T>(resourceLocation);
    }

    public async Task<Bundle> SearchAsync<T>(SearchParams searchParams) where T : Resource
    {
         return await fhirClient.SearchAsync<T>(searchParams) ?? new Bundle { Entry = [] };
    }
}
