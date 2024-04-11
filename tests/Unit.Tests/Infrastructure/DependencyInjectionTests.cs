using Core.Common.Abstractions.Clients;
using Core.Ndop.Abstractions;
using Core.Ods.Abstractions;
using Core.Pds.Abstractions;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unit.Tests.Infrastructure;

public class DependencyInjectionTests
{

    [Fact]
    public void AddInfrastructure_ShouldAddServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Local.json")
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var pdsFhirClient = serviceProvider.GetService<IPdsFhirClient>();
        var pdsMeshClient = serviceProvider.GetService<IPdsMeshClient>();
        var odsClient = serviceProvider.GetService<IOdsCsvDownloadClient>();
        var ndopMeshClient = serviceProvider.GetService<INdopMeshClient>();
        var dataHubFhirClient = serviceProvider.GetService<IDataHubFhirClient>();

        pdsFhirClient.ShouldNotBeNull();
        pdsMeshClient.ShouldNotBeNull();
        odsClient.ShouldNotBeNull();
        ndopMeshClient.ShouldNotBeNull();
        dataHubFhirClient.ShouldNotBeNull();
    }

    [Fact]
    public void AddMeshClient_ShouldThrowException_WhenMeshConfigurationIsMissingFromAppSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("Infrastructure/TestAppSettings/no-mesh-appsettings.json")
            .Build();
        var services = new ServiceCollection();

        var exception = Should.Throw<Exception>(() => services.AddMeshHttpClient(configuration));
        exception.Message.ShouldBe("Mesh is not configured.");
    }
}