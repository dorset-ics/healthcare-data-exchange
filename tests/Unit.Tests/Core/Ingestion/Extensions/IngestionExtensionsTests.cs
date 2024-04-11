using Core.Ingestion.Enums;
using Core.Ingestion.Extensions;
using Core.Ingestion.Models;

namespace Unit.Tests.Core.Ingestion.Extensions;


public class IngestionExtensionsTests
{
    [Fact]
    public void AsHL7v2IngestionRequest_WithValidIngestionRequest_ReturnsHL7v2IngestionRequest()
    {
        const string message = "MSH|^~\\&|AGYLEED|R0D02|INTEGRATION-ENGINE|RDZ|20231127125907||ADT^A01|667151|P|2.4|||AL|NE";
        const string org = "R0D";
        const string domain = "emergency-care";
        var ingestionRequest = new IngestionRequest(org, domain, IngestionDataType.HL7v2, message);

        var hl7v2IngestionRequest = ingestionRequest.ToConvertDataRequest();
        hl7v2IngestionRequest.Input.ShouldBe(message);
        hl7v2IngestionRequest.TemplateInfo.Name.ShouldBe("r0d_emergency-care_hl7v2_adta01");
    }
}