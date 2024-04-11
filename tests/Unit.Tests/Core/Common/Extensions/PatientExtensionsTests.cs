using Core.Common.Extensions;
using Hl7.Fhir.Model;

namespace Unit.Tests.Core.Common.Extensions;

public class PatientExtensionsTests
{
    [Fact]
    public void NhsNumber_WithValidNhsNumber_ReturnsNhsNumber()
    {
        var patient = new Patient
        {
            Identifier =
            [
                new Identifier
                {
                    System = "https://fhir.nhs.uk/Id/nhs-number",
                    Value = "1234567890"
                }
            ]
        };
        var nhsNumber = patient.GetNhsNumber();
        nhsNumber.ShouldBe("1234567890");
    }

    [Fact]
    public void NhsNumber_WithNullIdentifier_ReturnsNull()
    {
        var patient = new Patient
        {
            Identifier = null
        };
        var nhsNumber = patient.GetNhsNumber();
        nhsNumber.ShouldBeNull();
    }

    [Fact]
    public void NhsNumber_WithOtherIdentifier_ReturnsNull()
    {
        var patient = new Patient
        {
            Identifier =
            [
                new Identifier
                {
                    System = "hello-world",
                    Value = "1234567890"
                }
            ]
        };
        var nhsNumber = patient.GetNhsNumber();
        nhsNumber.ShouldBeNull();
    }

    [Fact]
    public void NhsNumber_WithEmptyNhsNumber_ReturnsEmptyString()
    {
        var patient = new Patient
        {
            Identifier =
            [
                new Identifier
                {
                    System = "https://fhir.nhs.uk/Id/nhs-number",
                    Value = "123"
                }
            ]
        };
        var nhsNumber = patient.GetNhsNumber();
        nhsNumber.ShouldBe("123");
    }
}