using Core.Common.Results;
using Core.Ingestion.Enums;
using Core.Ingestion.Models;

namespace Api.ResponseMappers;

public class ResponseMapperFactory : IResponseMapperFactory
{
    public Result<IResponseMapper> Create(IngestionRequest ingestionRequest)
    {
        return ingestionRequest.IngestionDataType switch
        {
            IngestionDataType.HL7v2 => new HL7v2ResponseMapper(ingestionRequest),
            _ => new NotSupportedException($"IngestionDataType {ingestionRequest.IngestionDataType.ToString()} is not supported")
        };
    }
}