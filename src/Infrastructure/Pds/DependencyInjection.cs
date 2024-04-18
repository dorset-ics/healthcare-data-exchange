using System.IO.Abstractions;
using Core.Pds.Abstractions;
using Core.Pds.Validators;
using FluentValidation;
using Hl7.Fhir.Rest;
using Infrastructure.Common.Authentication;
using Infrastructure.Common.Configuration;
using Infrastructure.Common.Handlers;
using Infrastructure.Common.Utilities;
using Infrastructure.Pds.Configuration;
using Infrastructure.Pds.Fhir.Clients;
using Infrastructure.Pds.Fhir.Clients.Abstractions;
using Infrastructure.Pds.Mesh.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NEL.MESH.Clients;

namespace Infrastructure.Pds;

public static class DependencyInjection
{
    public static IServiceCollection AddPdsInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddConfiguration(configuration)
            .AddPdsFhirClient(configuration)
            .AddPdsMeshClient(configuration)
            .AddFluentValidators();
    }

    private static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var pdsConfiguration = configuration.GetSection(PdsConfiguration.SectionKey).Get<PdsConfiguration>()
                               ?? throw new Exception("Pds section has not been configured.");

        services.AddSingleton(pdsConfiguration);

        return services;
    }

    private static IServiceCollection AddPdsMeshClient(this IServiceCollection services, IConfiguration configuration)
    {
        var meshConfiguration = configuration.GetSection(MeshConfiguration.SectionKey).Get<MeshConfiguration>()
                                ?? throw new Exception("Mesh section has not been configured.");

        var pdsConfiguration = configuration.GetSection(PdsConfiguration.SectionKey).Get<PdsConfiguration>()
                               ?? throw new Exception("Pds section has not been configured.");

        var meshConfig = new NEL.MESH.Models.Configurations.MeshConfiguration
        {
            MailboxId = pdsConfiguration.Mesh.MailboxId,
            Key = pdsConfiguration.Mesh.Key,
            Url = meshConfiguration.Url,
            MaxChunkSizeInMegabytes = meshConfiguration.MaxChunkSizeInMegabytes,
            Password = pdsConfiguration.Mesh.MailboxPassword
        };

        if (meshConfiguration.Authentication.IsEnabled)
        {
            meshConfig.RootCertificate = CertificateUtilities.ParseCertificate(meshConfiguration.Authentication.RootCertificate);
            meshConfig.ClientCertificate = CertificateUtilities.ParseCertificate(meshConfiguration.Authentication.ClientCertificate);
            meshConfig.IntermediateCertificates = [CertificateUtilities.ParseCertificate(meshConfiguration.Authentication.SubCertificate)];
        }

        services.AddKeyedTransient<IMeshClient>("Pds", (provider, _) =>
        {
            var loggingFactory = provider.GetRequiredService<ILoggerFactory>();
            return new MeshClient(meshConfig, loggingFactory);
        });
        services.AddTransient<IPdsMeshClient, PdsMeshClient>();

        return services;
    }

    private static IServiceCollection AddPdsFhirClient(this IServiceCollection services, IConfiguration configuration)
    {
        var pdsConfiguration = configuration.GetSection(PdsConfiguration.SectionKey).Get<PdsConfiguration>()
                               ?? throw new Exception("Pds section has not been configured.");
        if (pdsConfiguration.Fhir.Authentication is { IsEnabled: true, Certificate: null or "" })
            throw new Exception("Pds FhirCertificate is not set or empty");

        services.AddSingleton(pdsConfiguration.Fhir.Authentication);
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<JwtHandler>();
        services.AddHttpClient<ITokenFactory, TokenFactory>(client =>
        {
            client.BaseAddress = new Uri(pdsConfiguration.Fhir.Authentication.TokenUrl);
        });


        services.AddHttpClient("PdsFhirClient")
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var tokenFactory = provider.GetRequiredService<ITokenFactory>();
                var handler = new HttpClientHandler
                {
                    CheckCertificateRevocationList = true
                };
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return new HttpClientAuthenticationHandler(tokenFactory, handler, loggerFactory.CreateLogger<HttpClientAuthenticationHandler>(), pdsConfiguration.Fhir.Authentication.IsEnabled);
            })
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(pdsConfiguration.Fhir.BaseUrl));

        services.AddTransient<IPdsFhirClientWrapper>(provider =>
        {
            var httpClient = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("PdsFhirClient");
            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = false,
                PreferredParameterHandling = SearchParameterHandling.Lenient
            };

            var fhirClient = new FhirClient(pdsConfiguration.Fhir.BaseUrl, httpClient, settings);
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new PdsFhirClientWrapper(fhirClient, loggerFactory.CreateLogger<PdsFhirClientWrapper>());
        });
        services.AddTransient<IPdsFhirClient, PdsFhirClient>();
        return services;
    }

    private static IServiceCollection AddFluentValidators(this IServiceCollection services) =>
        services.AddValidatorsFromAssemblyContaining<PdsMeshCsvValidator>();
}