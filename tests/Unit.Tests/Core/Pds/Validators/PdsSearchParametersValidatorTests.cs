using Core.Pds.Models;
using Core.Pds.Validators;

namespace Unit.Tests.Core.Pds.Validators;

public class PdsSearchParametersValidatorTests
{
    private readonly PdsSearchParametersValidator _validator = new();


    [Fact]
    public void ShouldFailValidationWhenNoParametersProvided()
    {
        var invalidInput = new PdsSearchParameters();

        var validationResult = _validator.Validate(invalidInput);

        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldContain(error => error.ErrorMessage == "At least one parameter must be provided.");
    }

    [Fact]
    public void ShouldFailValidationWhenFhirStandardPropertyIsEmpty()
    {
        var invalidInput = new PdsSearchParameters
        {
            FamilyName = string.Empty,
        };

        var validationResult = _validator.Validate(invalidInput);

        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldContain(error => error.ErrorMessage == "FamilyName cannot be empty or whitespaces.");
    }

    [Fact]
    public void ShouldFailValidationWhenFhirStandardPropertyIsEmptyAndNhsNumberNotProvided()
    {
        var invalidInput = new PdsSearchParameters
        {
            FamilyName = string.Empty,
        };

        var validationResult = _validator.Validate(invalidInput);

        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldContain(error => error.ErrorMessage == "FamilyName cannot be empty or whitespaces.");
    }
}