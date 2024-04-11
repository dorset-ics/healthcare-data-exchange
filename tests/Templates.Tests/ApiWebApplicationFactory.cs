using DotNetEnv;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Templates.Tests.DataProviders;


namespace Templates.Tests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public ApiWebApplicationFactory()
    {
        Env.Load(TestPaths.EnvFilePath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Integration");
        builder.ConfigureTestServices(RemoveQuartzHostedService);
    }
    private static void RemoveQuartzHostedService(IServiceCollection services) =>
        services.Remove(services.Where(s => !s.IsKeyedService).Single(s => s.ImplementationType == typeof(QuartzHostedService)));
}