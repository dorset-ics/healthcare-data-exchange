using Core.Common.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Terminology;

public static class DependencyInjection
{
    public static IServiceCollection AddTerminologyInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddTransient<ITerminologyService, FileTerminologyService>();

        return services;
    }
}