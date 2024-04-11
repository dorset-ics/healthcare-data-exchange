using Core.Pds.Abstractions;
using Core.Pds.Validators;
using Infrastructure.Pds;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unit.Tests.Infrastructure.Pds;

public class DependencyInjectionTests
{
    [Fact]
    public void AddPdsInfrastructure_ShouldThrowException_WhenPdsConfigurationIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        Should.Throw<Exception>(() => services.AddPdsInfrastructure(configuration));
    }

    [Fact]
    public void AddPdsInfrastructure_ShouldAddServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Local.json")
            .Build();

        var services = new ServiceCollection();

        services.AddPdsInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var pdsFhirClient = serviceProvider.GetService<IPdsFhirClient>();
        var pdsMeshClient = serviceProvider.GetService<IPdsMeshClient>();
        var validator = serviceProvider.GetService<PdsMeshCsvValidator>();

        pdsFhirClient.ShouldNotBeNull();
        pdsMeshClient.ShouldNotBeNull();
        validator.ShouldNotBeNull();
    }
}