using Core.Ingestion.Enums;
using Core.Ingestion.Models;
using Core.Ingestion.Validators;

namespace Unit.Tests.Core.Ingestion.Validators;

public class IngestionRequestValidatorTest
{
    private readonly IngestionRequestValidator _validator = new();

    [Fact]
    public void GivenValidIngestionDataType_WhenValidating_ThenValidationShouldPass()
    {
        var validInput = new IngestionRequest
        (
            OrganisationCode: "123",
            SourceDomain: "Test",
            IngestionDataType: IngestionDataType.HL7v2,
            Message: "MSH|^~\\&|CaMIS|RDZ|ASCRIBE|INTEGRATION-ENGINE|202312111258||ADT^A13^ADT_A01|8431184|P|2.4|||\"\"|\"\"|GBR|ASCII|EN||ITKv1.0\n"
        );

        var result = _validator.Validate(validInput);
        result.Errors.Count.ShouldBe(0);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenNoParameters_WhenValidating_ThenValidationShouldFail()
    {
        var invalidInput = new IngestionRequest(string.Empty, string.Empty, IngestionDataType.HL7v2, null!);

        var validationResult = _validator.Validate(invalidInput);

        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldContain(error => error.ErrorMessage == "Organisation code cannot be empty or whitespaces when provided.");
        validationResult.Errors.ShouldContain(error => error.ErrorMessage == "Source domain cannot be empty or whitespaces when provided.");
        validationResult.Errors.ShouldContain(error => error.ErrorMessage == "The hl7 message cannot be null when ingestion data type is hl7v2.");
    }

    [Fact]
    public void GivenEmptyOrganisationCode_WhenValidating_ThenValidationShouldFail()
    {
        var invalidInput = new IngestionRequest(
            OrganisationCode: string.Empty, SourceDomain: "Test", IngestionDataType: IngestionDataType.HL7v2, Message: "MSH|"
        );

        _validator.Validate(invalidInput).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GivenWhitespaceOrganisationCode_WhenValidating_ThenValidationShouldFail()
    {
        var invalidInput = new IngestionRequest(
            OrganisationCode: "     ", SourceDomain: "Test", IngestionDataType: IngestionDataType.HL7v2, Message: "MSH|"
        );
        _validator.Validate(invalidInput).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GivenEmptySourceDomain_WhenValidating_ThenValidationShouldFail()
    {
        var invalidInput = new IngestionRequest(
            OrganisationCode: "123", SourceDomain: string.Empty, IngestionDataType: IngestionDataType.HL7v2, Message: "MSH|"
        );

        _validator.Validate(invalidInput).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GivenWhitespaceSourceDomain_WhenValidating_ThenValidationShouldFail()
    {
        var invalidInput = new IngestionRequest(
            OrganisationCode: "123", SourceDomain: "     ", IngestionDataType: IngestionDataType.HL7v2, Message: "MSH|"
        );
        _validator.Validate(invalidInput).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GivenEmptyIngestionMessage_WhenValidating_ThenValidationShouldFail()
    {
        var invalidInput = new IngestionRequest(
            OrganisationCode: "123", SourceDomain: "Test", IngestionDataType: IngestionDataType.HL7v2, Message: string.Empty
        );

        _validator.Validate(invalidInput).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GivenUndefinedIngestionDataType_WhenValidating_ThenValidationShouldFail()
    {
        var invalidInput = new IngestionRequest(
            OrganisationCode: "123", SourceDomain: "Test", IngestionDataType: (IngestionDataType)999, Message: string.Empty
        );
        var validationResult = _validator.Validate(invalidInput);

        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.Count.ShouldBe(1);
        validationResult.Errors[0].ErrorMessage.ShouldContain("Data Type must be a valid Ingestion Data Type.");
    }
}