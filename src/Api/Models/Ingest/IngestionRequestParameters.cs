using Core.Ingestion.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Models.Ingest;

public class IngestionRequestParameters
{
    [FromHeader(Name = "organisation-code")] public string? OrganisationCode { get; set; }
    [FromHeader(Name = "source-domain")] public string? SourceDomain { get; set; }
    [FromHeader(Name = "data-type")] public IngestionDataType IngestionDataType { get; set; }
}
