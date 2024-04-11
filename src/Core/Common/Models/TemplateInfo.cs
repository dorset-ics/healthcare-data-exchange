namespace Core.Common.Models;

public record TemplateInfo(string OrganisationCode, string Domain, string DataType, string ResourceType)
{
    public string Name => $"{OrganisationCode}_{Domain}_{DataType}_{ResourceType}".ToLower();

    public static TemplateInfo ForPdsMeshPatient() => new("x26", "pds-mesh", "json", "patient");
    public static TemplateInfo ForNdopMeshConsent() => new("x26", "ndop-Mesh", "json", "consent");
    public static TemplateInfo ForOdsCsvDownloadCountry(string country) => new("x26", $"ods-csv-download", "json", country);
}
