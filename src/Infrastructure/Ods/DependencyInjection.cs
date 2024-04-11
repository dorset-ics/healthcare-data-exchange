using Core.Ods.Abstractions;
using Infrastructure.Ods.Clients;
using Infrastructure.Ods.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Ods;

public static class DependencyInjection
{
    public static IServiceCollection AddOdsInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddConfiguration(configuration)
            .AddOdsClient();
    }

    private static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var odsCsvDownloadConfiguration = configuration.GetSection(OdsCsvDownloadConfiguration.SectionKey).Get<OdsCsvDownloadConfiguration>()
                               ?? throw new Exception("Ods CSV download section has not been configured.");

        services.AddSingleton(odsCsvDownloadConfiguration);

        return services;
    }

    private static IServiceCollection AddOdsClient(this IServiceCollection services)
    {
        services.AddHttpClient("OdsCsvDownloadHttpClient");

        services.AddTransient<IOdsCsvDownloadClient, OdsCsvDownloadClient>();

        return services;
    }
}