using Core.Ingestion.Enums;

namespace Core.Ingestion.Models
{
    public record IngestionRequest(
        string OrganisationCode,
        string SourceDomain,
        IngestionDataType IngestionDataType,
        string Message
    )
    {
    }
}