using Core.Common.Abstractions.Clients;
using DotNetEnv;
using Integration.Tests.DataProviders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Integration.Tests;

internal sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public ApiWebApplicationFactory()
    {
        Env.Load(TestPaths.EnvFilePath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Integration");

        builder.ConfigureTestServices(services =>
        {
            RemoveQuartzHostedService(services);
            var dataHubFhirClient = services.BuildServiceProvider().GetRequiredService<IDataHubFhirClient>();
            SeedDataProvider.RegisterSeedData(dataHubFhirClient);
        });
    }

    private static void RemoveQuartzHostedService(IServiceCollection services) =>
        services.Remove(services.Where(s => !s.IsKeyedService).Single(s => s.ImplementationType == typeof(QuartzHostedService)));
}