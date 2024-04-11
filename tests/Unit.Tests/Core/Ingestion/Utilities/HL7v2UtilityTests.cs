using Core.Ingestion.Utilities;

namespace Unit.Tests.Core.Ingestion.Utilities;

public class HL7v2UtilityTests
{
    [Fact]
    public void Test_GetMessageType_ShouldReturnMessageType()
    {
        var message = "MSH|^~\\&|AGYLEED|R0D02|INTEGRATION-ENGINE|RDZ|20231127125907||ADT^A01|667151|P|2.4|||AL|NE";

        var messageType = HL7v2Utility.GetMessageType(message);
        messageType.ShouldBe("ADTA01");
    }
}