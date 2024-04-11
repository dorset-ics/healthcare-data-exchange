using System.Text.Json;
using Core.Pds.Converters;
using Core.Pds.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Core.Pds.Converters;

public class PdsMeshCsvToJsonConverterTests
{
    private const string BaseSamplePath = "Core/Pds/Converters/Samples";
    private readonly PdsMeshCsvToJsonConverter _pdsMeshCsvToJsonConverter;
    private readonly IValidator<string> _validatorMock;

    public PdsMeshCsvToJsonConverterTests()
    {
        var loggerMock = Substitute.For<ILogger<PdsMeshCsvToJsonConverter>>();
        _validatorMock = Substitute.For<IValidator<string>>();
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult());
        _pdsMeshCsvToJsonConverter = new PdsMeshCsvToJsonConverter(loggerMock, _validatorMock);
    }

    [Fact]
    public void Convert_WhenBundleIsNull_ShouldThrow()
    {
        var result = _pdsMeshCsvToJsonConverter.Convert(null!);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task Convert_WhenResponseContainsOnePatient_ShouldReturnValidJsonWithOnePatient()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseSinglePatient.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        var response = JsonSerializer.Deserialize<Dictionary<string, List<PdsMeshRecordResponse>>>(result.Value);
        var records = response?["patients"];
        records.ShouldNotBeNull();
        records.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Convert_WhenResponseContainsMultiplePatients_ShouldReturnValidJsonWithMultiplePatients()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath,
            "MeshResponseMultiplePatients.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);

        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        var response = JsonSerializer.Deserialize<Dictionary<string, List<PdsMeshRecordResponse>>>(result.Value);
        var records = response?["patients"];
        records.ShouldNotBeNull();
        records.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Convert_WhenResponseIsEmpty_ShouldThrow()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseNoPatients.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ApplicationException>();
    }

    [Fact]
    public void GivenValidationError_WhenConvert_ThenReturnValidationResult()
    {
        _validatorMock.Validate("test").Returns(new ValidationResult(failures: [new ValidationFailure()]));
        var result = _pdsMeshCsvToJsonConverter.Convert("test");

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ValidationException>();
    }
}