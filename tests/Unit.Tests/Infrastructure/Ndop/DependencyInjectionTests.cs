using Core.Ndop.Abstractions;
using Infrastructure.Ndop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unit.Tests.Infrastructure.Ndop;

public class DependencyInjectionTests
{
    [Fact]
    public void AddNdopInfrastructure_ShouldThrowException_WhenNdopConfigurationIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        Should.Throw<Exception>(() => services.AddNdopInfrastructure(configuration));
    }

    [Fact]
    public void AddNdopInfrastructure_ShouldAddServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Local.json")
            .Build();
        var services = new ServiceCollection();

        services.AddNdopInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var meshClient = serviceProvider.GetService<INdopMeshClient>();

        meshClient.ShouldNotBeNull();
    }

    [Fact]
    public void AddNdopInfrastructure_ShouldParseCertificatesWhenAuthIsEnabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("Infrastructure/TestAppSettings/ndop-mesh-appsettings.json")
            .Build();
        var services = new ServiceCollection();

        services.AddNdopInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var meshClient = serviceProvider.GetService<INdopMeshClient>();

        meshClient.ShouldNotBeNull();
    }
}