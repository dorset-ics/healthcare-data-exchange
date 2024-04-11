using Core.Common.Results;
using Core.Ingestion.Models;

namespace Api.ResponseMappers;

public interface IResponseMapperFactory
{
    Result<IResponseMapper> Create(IngestionRequest ingestionRequest);
}