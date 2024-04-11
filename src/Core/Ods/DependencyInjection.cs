using Core.Common.Abstractions.Converters;
using Core.Common.Results;
using Core.Ods.Abstractions;
using Core.Ods.Converters;
using Core.Ods.Models;
using Core.Ods.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Ods;

public static class DependencyInjection
{
    public static IServiceCollection AddOds(this IServiceCollection services)
        => services
            .AddConverters()
            .AddServices()
            .AddUtilities();

    private static IServiceCollection AddConverters(this IServiceCollection services)
        => services
            .AddTransient<IConverter<OdsCsvIngestionData, Result<string>>, OdsCsvToJsonConverter>();

    private static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddScoped<IOdsService, OdsService>();

    private static IServiceCollection AddUtilities(this IServiceCollection services)
        => services
            .AddScoped<IOdsCsvIngestionStrategy, OdsCsvIngestionStrategy>();
}