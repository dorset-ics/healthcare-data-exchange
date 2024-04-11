using System.Diagnostics.CodeAnalysis;
using Infrastructure.Common.HealthCheck;

namespace Infrastructure.Pds.Fhir.HealthCheck;

[ExcludeFromCodeCoverage]
public class PdsFhirHealthCheck(IHttpClientFactory clientFactory) : BaseHealthCheck(clientFactory)
{
    protected override string ClientName => "PdsFhirClient";
    protected override string HealthCheckEndpoint => "/personal-demographics/FHIR/R4/_ping";

}