using System.Net;
using Infrastructure.AzureTableStorageCache;
using Infrastructure.Common.Configuration;
using Infrastructure.Common.HealthCheck;
using Infrastructure.DataHub;
using Infrastructure.DataHub.HealthCheck;
using Infrastructure.Ndop;
using Infrastructure.Ndop.Mesh.HealthCheck;
using Infrastructure.Ods;
using Infrastructure.Pds;
using Infrastructure.Pds.Fhir.HealthCheck;
using Infrastructure.Pds.Mesh.HealthCheck;
using Infrastructure.Terminology;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddDataHubFhirInfrastructure(configuration)
            .AddMeshHttpClient(configuration)
            .AddPdsInfrastructure(configuration)
            .AddOdsInfrastructure(configuration)
            .AddNdopInfrastructure(configuration)
            .AddTerminologyInfrastructure()
            .AddAzureTableStorageCacheInfrastructure(configuration)
            .AddInfrastructureHealthChecks();
    }


    private static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DataHubFhirHealthCheck>("DataHub FHIR Health Check", tags: ["FHIR", "DataHub", "Api"])
            .AddCheck<PdsFhirHealthCheck>("Pds FHIR Health Check", tags: ["FHIR", "PDS", "Api"])
            .AddCheck<NdopMeshMailboxHealthCheck>("Ndop Mesh Mailbox Health Check", tags: ["Ndop", "Mesh", "Background"])
            .AddCheck<PdsMeshMailboxHealthCheck>("Pds Mesh Mailbox Health Check", tags: ["Pds", "Mesh", "Background"]);

        return services;
    }

    public static IServiceCollection AddMeshHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        var meshConfiguration = configuration.GetSection(MeshConfiguration.SectionKey).Get<MeshConfiguration>()
                                ?? throw new Exception("Mesh is not configured.");

        services.AddHttpClient("MeshHttpClient")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(meshConfiguration.Url));

        return services;
    }
}