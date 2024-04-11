using Core.Common.Results;
using Core.Ingestion.Models;

namespace Core.Ingestion.Abstractions;

public interface IIngestionService
{
    Task<Result> Ingest(IngestionRequest ingestionRequest);
}