using Core.Ndop.Abstractions;
using Infrastructure.Common.Configuration;
using Infrastructure.Common.Utilities;
using Infrastructure.Ndop.Configuration;
using Infrastructure.Ndop.Mesh.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NEL.MESH.Clients;

namespace Infrastructure.Ndop;

public static class DependencyInjection
{
    public static IServiceCollection AddNdopInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddConfiguration(configuration)
            .AddNdopMeshClient(configuration)
            .AddLogging();
    }

    private static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var ndopConfiguration = configuration.GetSection(NdopConfiguration.SectionKey).Get<NdopConfiguration>()
                               ?? throw new Exception("Ndop section has not been configured.");

        services.AddSingleton(ndopConfiguration);

        return services;
    }

    private static IServiceCollection AddNdopMeshClient(this IServiceCollection services, IConfiguration configuration)
    {
        var meshConfig = GetMeshConfiguration(configuration);

        services.AddKeyedTransient<IMeshClient>("Ndop", (provider, _) =>
        {
            var loggingFactory = provider.GetRequiredService<ILoggerFactory>();
            return new MeshClient(meshConfig, loggingFactory);
        });

        services.AddTransient<INdopMeshClient, NdopMeshClient>();

        return services;
    }

    private static NEL.MESH.Models.Configurations.MeshConfiguration GetMeshConfiguration(IConfiguration configuration)
    {
        var meshConfiguration = configuration.GetSection(MeshConfiguration.SectionKey).Get<MeshConfiguration>()
                                ?? throw new Exception("Mesh section has not been configured.");

        var ndopConfiguration = configuration.GetSection(NdopConfiguration.SectionKey).Get<NdopConfiguration>()
                                ?? throw new Exception("Ndop section has not been configured.");

        var nelMeshConfig = new NEL.MESH.Models.Configurations.MeshConfiguration
        {
            MailboxId = ndopConfiguration.Mesh.MailboxId,
            Key = ndopConfiguration.Mesh.Key,
            Url = meshConfiguration.Url,
            MaxChunkSizeInMegabytes = meshConfiguration.MaxChunkSizeInMegabytes,
            Password = ndopConfiguration.Mesh.MailboxPassword
        };
        if (!meshConfiguration.Authentication.IsEnabled)
        {
            return nelMeshConfig;
        }

        nelMeshConfig.RootCertificate = CertificateUtilities.ParseCertificate(meshConfiguration.Authentication.RootCertificate);
        nelMeshConfig.ClientCertificate = CertificateUtilities.ParseCertificate(meshConfiguration.Authentication.ClientCertificate);
        nelMeshConfig.IntermediateCertificates = [CertificateUtilities.ParseCertificate(meshConfiguration.Authentication.SubCertificate)];

        return nelMeshConfig;
    }
}