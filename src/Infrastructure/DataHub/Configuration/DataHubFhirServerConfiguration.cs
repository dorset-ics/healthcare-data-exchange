namespace Infrastructure.Configuration;

public record DataHubFhirServerConfiguration(string BaseUrl, string TemplateImage, DataHubAuthConfiguration Authentication)
{
    public const string SectionKey = "DataHubFhirServer";
}

public record DataHubAuthConfiguration(bool IsEnabled, string Scope);