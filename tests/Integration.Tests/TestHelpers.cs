using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.DataHub.Clients.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests
{
    internal static class TestHelpers
    {
        internal static async Task ResetFhirResource<T>(this IDataHubFhirClientWrapper fhirClient) where T : Resource, new()
        {
            var existingResources = await fhirClient.SearchResourceByParams<T>(new SearchParams());

            while (existingResources != null)
            {
                foreach (var existingResource in existingResources.Entry)
                    await fhirClient.DeleteAsync($"{existingResource.Resource.TypeName}/{existingResource.Resource.Id}");

                existingResources = await fhirClient.ContinueAsync(existingResources);
            }
        }
    }
}
