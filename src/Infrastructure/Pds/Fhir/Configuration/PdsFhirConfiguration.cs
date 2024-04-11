namespace Infrastructure.Pds.Fhir.Configuration;

public record PdsFhirConfiguration(string BaseUrl, PdsAuthConfiguration Authentication);

public record PdsAuthConfiguration(bool IsEnabled = true, string TokenUrl = "", string ClientId = "", string Kid = "", string? Certificate = null);