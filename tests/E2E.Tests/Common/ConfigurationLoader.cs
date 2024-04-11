using Microsoft.Extensions.Configuration;

namespace E2E.Tests.Common;

public static class ConfigurationLoader
{
    private static IConfigurationRoot Configuration { get; } = GetConfiguration();

    public static CommonMeshSettings GetCommonMeshSettings()
    {
        return Configuration
                   .GetSection(CommonMeshSettings.SectionKey)
                   .Get<CommonMeshSettings>()
               ?? throw new Exception("CommonMeshSettings section has not been configured.");
    }

    public static Dictionary<MeshClientName, MeshClientsSettings> GetMeshClientsSettings()
    {
        var meshClientsSettings = Configuration.GetSection(MeshProviders.SectionKey).Get<MeshProviders>();
        if (meshClientsSettings is null) throw new Exception("MeshProviders section has not been configured.");
        return meshClientsSettings.ToDictionary(
            item => Enum.Parse<MeshClientName>(item.Key),
            item => item.Value
        );
    }

    private static IConfigurationRoot GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("mesh.json", optional: true, reloadOnChange: true)
            .Build();
    }
}