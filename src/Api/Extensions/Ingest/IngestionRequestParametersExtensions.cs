using Api.Models.Ingest;
using Core.Ingestion.Models;

namespace Api.Extensions.Ingest;

public static class IngestionRequestParametersExtensions
{
    public static IngestionRequest ToIngestionRequest(this IngestionRequestParameters ingestionRequestParameters, string message)
    {
        ArgumentNullException.ThrowIfNull(ingestionRequestParameters);

        return new IngestionRequest(
            ingestionRequestParameters.OrganisationCode!,
            ingestionRequestParameters.SourceDomain!,
            ingestionRequestParameters.IngestionDataType,
            message);
    }
}