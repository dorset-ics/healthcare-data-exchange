using Core.Common.Models;
using Core.Ingestion.Models;
using Core.Ingestion.Utilities;

namespace Core.Ingestion.Extensions;

public static class IngestionExtensions
{
    public static ConvertDataRequest ToConvertDataRequest(this IngestionRequest ingestionRequest)
    {
        var (orgCode, domain, dataType, message) = ingestionRequest;
        var ingestionDataType = new TemplateInfo(orgCode, domain, dataType.ToString(), HL7v2Utility.GetMessageType(message));
        return new ConvertDataRequest(message, ingestionDataType);
    }
}