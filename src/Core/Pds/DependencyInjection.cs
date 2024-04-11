using Core.Common.Abstractions.Converters;
using Core.Common.Results;
using Core.Pds.Abstractions;
using Core.Pds.Converters;
using Core.Pds.Models;
using Core.Pds.Validators;
using FluentValidation;
using Hl7.Fhir.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Pds;

public static class DependencyInjection
{
    public static IServiceCollection AddPds(this IServiceCollection services)
        => services
            .AddConverters()
            .AddFluentValidators()
            .AddServices();

    private static IServiceCollection AddConverters(this IServiceCollection services)
        => services
            .AddTransient<IConverter<Bundle, Result<PdsMeshBundleToCsvConversionResult>>, PdsMeshBundleToCsvConverter>()
            .AddTransient<IConverter<string, Result<string>>, PdsMeshCsvToJsonConverter>();

    private static IServiceCollection AddFluentValidators(this IServiceCollection services)
        => services
            .AddValidatorsFromAssemblyContaining<PdsMeshCsvValidator>()
            .AddValidatorsFromAssemblyContaining<PdsSearchParametersValidator>();

    private static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddScoped<IPdsService, PdsService>();
}