using Core.Pds.Validators;

namespace Unit.Tests.Core.Pds.Validators;

public class PdsMeshCsvValidatorTests
{
    private readonly PdsMeshCsvValidator _validator = new();


    [Fact]
    public void ShouldValidateValidInput()
    {
        var validInput =
            "MPTREQ_20210802171059,SPINE_MPT_RESPONSE,1,00\n\n02403456-031f-11e7-a926-080027a2de00,9990554412,XXTESTPATIENT-THHX,DONOTUSE,,1,19380803,,C/O NHS DIGITAL TEST DATA MANAGER,SOLUTION ASSURANCE 1 TREVELYAN SQ.,BOAR LANE,LEEDS,WEST YORKSHIRE,LS1 6AE,Y90001,,,,,,0723444447,temp.dummy@nhs.net,N,,00,9990554412,1,100,0,0,0,0,0";

        _validator.Validate(validInput).IsValid.ShouldBeTrue();
    }


    [Fact]
    public void ShouldFailValidationWhenNoResponseHeaderRecord()
    {
        var invalidInput =
            "02403456-031f-11e7-a926-080027a2de00,9990554412,XXTESTPATIENT-THHX,DONOTUSE,,1,19380803,,C/O NHS DIGITAL TEST DATA MANAGER,SOLUTION ASSURANCE 1 TREVELYAN SQ.,BOAR LANE,LEEDS,WEST YORKSHIRE,LS1 6AE,Y90001,,,,,,0723444447,temp.dummy@nhs.net,N,,00,9990554412,1,100,0,0,0,0,0";

        var validationResult = _validator.Validate(invalidInput);

        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldContain(error =>
            error.ErrorMessage == "CSV must be in the right format.");
    }

    [Fact]
    public void ShouldFailValidationWhenCsvIsEmpty()
    {
        var invalidInput = string.Empty;

        var validationResult = _validator.Validate(invalidInput);

        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldContain(error =>
            error.ErrorMessage == "CSV content cannot be empty.");
    }
}