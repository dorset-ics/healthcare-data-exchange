using Core.Common.Strategies;
using Core.Common.Utilities;
using Core.Ingestion.Abstractions;
using Core.Ingestion.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Ingestion;

public static class DependencyInjection
{
    public static IServiceCollection AddIngestion(this IServiceCollection services)
        => services
            .AddIngestionStrategies()
            .AddServices()
            .AddUtilities();

    private static IServiceCollection AddIngestionStrategies(this IServiceCollection services) =>
        services.AddKeyedScoped<IIngestionStrategy, HL7v2IngestionStrategy>(IngestionDataType.HL7v2);

    private static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddScoped<IIngestionService, IngestionService>();

    private static IServiceCollection AddUtilities(this IServiceCollection services)
        => services
            .AddScoped<IFhirResourceEnhancer, FhirResourceEnhancer>();
}