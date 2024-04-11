using System.Text.Json;
using System.Text.RegularExpressions;
using Core.Common.Results;
using Core.Ods.Converters;
using Core.Ods.Enums;
using Core.Ods.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Unit.Tests.Core.Ods.Converters;

public class OdsCsvToJsonConverterTests
{
    private const string BaseSamplePath = "Core/Ods/Converters/Samples";
    private readonly OdsCsvToJsonConverter _odsCsvToJsonConverter;
    private readonly IValidator<string> _validatorMock;


    public OdsCsvToJsonConverterTests()
    {
        var loggerMock = Substitute.For<ILogger<OdsCsvToJsonConverter>>();
        _validatorMock = Substitute.For<IValidator<string>>();
        _odsCsvToJsonConverter = new OdsCsvToJsonConverter(loggerMock, _validatorMock);
    }

    [Fact]
    public void Convert_WhenSourceIsNull_ShouldThrow()
    {
        var result = _odsCsvToJsonConverter.Convert(null!);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public void Convert_WhenSourceIsInvalid_ShouldThrow()
    {
        var ingestData =
            OdsCsvIngestionData.GetDataBySource("invalid source", OdsCsvDownloadSource.EnglandAndWales);
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("source", "Invalid source")
        }));

        var result = _odsCsvToJsonConverter.Convert(ingestData);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ValidationException>();
    }

    [Fact]
    public async void Convert_WhenCsvIsEmpty_ShouldThrow()
    {
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult());
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "emptycsv.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var ingestData =
            OdsCsvIngestionData.GetDataBySource(fileContent, OdsCsvDownloadSource.EnglandAndWales);

        var result = _odsCsvToJsonConverter.Convert(ingestData);

        result.Value.ShouldBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ApplicationException>();
        result.Exception.Message.Contains("CSV conversion to JSON failed");
    }

    [Fact]
    public async Task Convert_WhenEnglandAndWalesSourceIsValid_ShouldReturnJson()
    {
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult());
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "etrust.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var ingestData =
            OdsCsvIngestionData.GetDataBySource(fileContent, OdsCsvDownloadSource.EnglandAndWales);

        var result = _odsCsvToJsonConverter.Convert(ingestData);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<string>();
        result.Value.Contains("\"organisations\":").ShouldBeTrue();

        AssertCsvValuesConvertedToJson(ingestData, result);
    }

    [Fact]
    public async Task Convert_WhenScotlandSourceIsValid_ShouldReturnJson()
    {
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult());
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "hospitals.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var ingestData =
            OdsCsvIngestionData.GetDataBySource(fileContent, OdsCsvDownloadSource.Scotland);

        var result = _odsCsvToJsonConverter.Convert(ingestData);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<string>();
        result.Value.Contains("\"organisations\":").ShouldBeTrue();

        AssertCsvValuesConvertedToJson(ingestData, result);
    }

    [Fact]
    public async Task Convert_WhenNorthernIrelandSourceIsValid_ShouldReturnJson()
    {
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult());
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath,
            "niorg.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var ingestData =
            OdsCsvIngestionData.GetDataBySource(fileContent, OdsCsvDownloadSource.NorthernIreland);

        var result = _odsCsvToJsonConverter.Convert(ingestData);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<string>();
        result.Value.Contains("\"organisations\":").ShouldBeTrue();

        AssertCsvValuesConvertedToJson(ingestData, result);
    }

    [Fact]
    public async Task Convert_WhenSourceContainsHeaderRow_ShouldNotConvertHeaderRow()
    {
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult());
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "withheaders.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var ingestData = OdsCsvIngestionData.GetDataBySource(fileContent, OdsCsvDownloadSource.Scotland);

        var result = _odsCsvToJsonConverter.Convert(ingestData);

        var headers = ingestData.Headers.Split(",");

        foreach (var header in headers)
            result.Value.Contains($"\"{header}\":\"{header}\"").ShouldBeFalse();
    }

    private static void AssertCsvValuesConvertedToJson(OdsCsvIngestionData ingestData, Result<string> result)
    {
        // Split on \n (LF) as all sample files are LF
        var lines = ingestData.CsvData.Split("\n");
        var headers = ingestData.Headers.Split(",").ToList();

        foreach (var line in lines)
        {
            if (line == ingestData.Headers)
                continue;

            foreach (var header in headers)
            {
                // Empty headers are not converted into JSON
                if (string.IsNullOrWhiteSpace(header))
                    continue;

                // Split the line on a column delimiter, taking into account commas in the CSV value
                var value = new Regex("((?<=\")[^\"]*(?=\"(,|$)+)|(?<=,|^)[^,\"]*(?=,|$))").Matches(line)[headers.IndexOf(header)].Value;

                // Serialize to JSON, to take into account encoding - e.g. "&" -> "\\u0026"
                // Headers are converted into JSON properties with spaces removed
                var jsonHeader = JsonSerializer.Serialize(header.Replace(" ", string.Empty));
                var jsonValue = JsonSerializer.Serialize(value);

                result.Value.Contains($"{jsonHeader}:{jsonValue}", StringComparison.CurrentCultureIgnoreCase)
                    .ShouldBeTrue($"Could not find the following JSON property: '{jsonHeader}:{jsonValue}'");
            }
        }
    }
}