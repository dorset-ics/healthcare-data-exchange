using Azure.Data.Tables;
using Azure.Identity;
using Infrastructure.AzureTableStorageCache.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.AzureTableStorageCache;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureTableStorageCacheInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var azureTableStorageCacheConfiguration = configuration.GetSection(AzureTableStorageCacheConfiguration.SectionKey).Get<AzureTableStorageCacheConfiguration>()
                                                  ?? throw new Exception("Azure Table Storage Cache is not configured.");
        services.AddConfiguration(azureTableStorageCacheConfiguration)
            .AddTableClient(azureTableStorageCacheConfiguration)
            .AddSingleton<IDistributedCache, AzureTableStorageCache>();
        return services;

    }

    private static IServiceCollection AddConfiguration(this IServiceCollection services, AzureTableStorageCacheConfiguration configuration)
    {
        services.AddSingleton(configuration);
        return services;
    }

    private static IServiceCollection AddTableClient(this IServiceCollection services, AzureTableStorageCacheConfiguration configuration)
    {
        services.AddSingleton(_ => configuration.Endpoint.IsNullOrEmpty() ?
            new TableClient(configuration.ConnectionString, configuration.TableName) :
            new TableClient(new Uri(configuration.Endpoint!), configuration.TableName, new DefaultAzureCredential()));
        return services;
    }


}