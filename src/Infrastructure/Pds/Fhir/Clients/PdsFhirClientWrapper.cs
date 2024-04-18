using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.Pds.Fhir.Clients.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Pds.Fhir.Clients;

public class PdsFhirClientWrapper(FhirClient fhirClient, ILogger<PdsFhirClientWrapper> logger) : IPdsFhirClientWrapper
{
    public Task<T?> ReadAsync<T>(string resourceLocation) where T : Resource
    {
        return fhirClient.ReadAsync<T>(resourceLocation);
    }

    public async Task<Bundle> SearchAsync<T>(SearchParams searchParams) where T : Resource
    {
        try
        {
            return await fhirClient.SearchAsync<T>(searchParams) ?? new Bundle { Entry = [] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error searching in FHIR for {searchParams}");
            return new Bundle { Entry = [] };
        }
    }
}