using Core.Common.Abstractions.Converters;
using Core.Common.Results;
using Core.Ndop.Abstractions;
using Core.Ndop.Converters;
using Core.Ndop.Models;
using Hl7.Fhir.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Ndop;

public static class DependencyInjection
{
    public static IServiceCollection AddNdop(this IServiceCollection services)
        => services
            .AddConverters()
            .AddServices();

    private static IServiceCollection AddConverters(this IServiceCollection services)
        => services
            .AddTransient<IConverter<Bundle, Result<NdopMeshBundleToCsvConversionResult>>,
                NdopMeshBundleToCsvConverter>()
            .AddTransient<IConverter<NdopMeshConversionRequest, Result<string>>,
                NdopMeshCsvToJsonConverter>();


    private static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddScoped<INdopService, NdopService>();
}