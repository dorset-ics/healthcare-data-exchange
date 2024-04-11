using Core.Common.Results;
using Core.Ingestion.Abstractions;
using Core.Ingestion.Enums;
using Core.Ingestion.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Ingestion;

public class IngestionService([FromKeyedServices(IngestionDataType.HL7v2)] IIngestionStrategy hl7V2IngestionStrategy) : IIngestionService
{
    public async Task<Result> Ingest(IngestionRequest ingestionRequest)
    {
        return ingestionRequest.IngestionDataType switch
        {
            IngestionDataType.HL7v2 => await hl7V2IngestionStrategy.Ingest(ingestionRequest),
            _ => new InvalidOperationException($"Ingestion data type {ingestionRequest.IngestionDataType} is not supported")
        };
    }
}