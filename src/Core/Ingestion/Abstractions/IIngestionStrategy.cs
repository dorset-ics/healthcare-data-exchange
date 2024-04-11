using Core.Common.Results;
using Core.Ingestion.Models;

namespace Core.Ingestion.Abstractions;

public interface IIngestionStrategy
{
    Task<Result> Ingest(IngestionRequest ingestionRequest);
}