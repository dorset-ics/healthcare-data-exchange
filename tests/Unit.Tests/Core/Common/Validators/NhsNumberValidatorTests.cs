using Core.Common.Models;
using Core.Common.Validators;

namespace Unit.Tests.Core.Common.Validators;

public class NhsNumberValidatorTests
{
    private readonly NhsNumberValidator _sut = new();

    [Theory]
    [InlineData("3353393188")]
    [InlineData("8086897257")]
    [InlineData("8546168504")]
    [InlineData("1362739782")]
    [InlineData("8759655151")]
    [InlineData("2606440870")]
    [InlineData("2729588175")]
    [InlineData("2625601841")]
    [InlineData("9999000584")]
    [InlineData("6927047344")]
    public void GivenValidNhsNumber_WhenValidating_ThenReturnsTrue(string nhsNumber)
    {

        var result = _sut.Validate(new NhsNumber(nhsNumber));

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("3363493188")]
    [InlineData("8086897267")]
    [InlineData("7544966705")]
    [InlineData("1142739982")]
    [InlineData("8755654351")]
    [InlineData("2616540970")]
    [InlineData("2739988165")]
    [InlineData("2915101111")]
    [InlineData("9989011984")]
    [InlineData("6327147677")]
    public void GivenInvalidNhsNumber_WhenValidating_ThenReturnsFalse(string nhsNumber)
    {
        var result = _sut.Validate(new NhsNumber(nhsNumber));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorMessage.ShouldBe("NHS Number is not valid.");
    }

    [Theory]
    [InlineData("632714777")]
    [InlineData("6327147 abc")]
    public void GivenInvalidCharacterSet_WhenValidating_ThenReturnsFalse(string nhsNumber)
    {

        var result = _sut.Validate(new NhsNumber(nhsNumber));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorMessage.ShouldBe("NHS Number must be 10 digits long and contain only numbers.");
    }

    [Fact]
    public void GivenEmptyNhsNumber_WhenValidating_ThenReturnsFalse()
    {
        var nhsNumber = string.Empty;

        var result = _sut.Validate(new NhsNumber(nhsNumber));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorMessage.ShouldBe("NHS Number cannot be empty.");
    }
    [Fact]
    public void GivenNullNhsNumber_WhenValidating_ThenReturnsFalse()
    {
        string nhsNumber = null!;

        var result = _sut.Validate(new NhsNumber(nhsNumber));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorMessage.ShouldBe("NHS Number cannot be null.");
    }
}