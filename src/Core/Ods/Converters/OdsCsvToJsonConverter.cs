using System.Globalization;
using System.Text.Json;
using Core.Common.Abstractions.Converters;
using Core.Common.Extensions;
using Core.Common.Results;
using Core.Ods.Models;
using CsvHelper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ValidationException = FluentValidation.ValidationException;

namespace Core.Ods.Converters;

public class OdsCsvToJsonConverter(ILogger<OdsCsvToJsonConverter> logger, IValidator<string> validator)
    : IConverter<OdsCsvIngestionData, Result<string>>
{
    public Result<string> Convert(OdsCsvIngestionData? source)
    {
        if (source == null)
        {
            logger.LogError($"Error converting ODS file to CSV: {nameof(source)} is null");
            return new ArgumentNullException(nameof(source));
        }

        var validationResult = validator.Validate(source.CsvData);
        if (!validationResult.IsValid)
        {
            return new ValidationException(string.Join(Environment.NewLine,
                validationResult.Errors.Select(error => error.ErrorMessage).ToList()));
        }


        var csvContent = AppendCsvHeader(source.CsvData, source.Headers);
        var records = ConvertCsvToOrgRecordResponse(csvContent, source.ClassType);


        if (!records.Any())
        {
            logger.LogWarning("CSV {csv} conversion to JSON failed", source.CsvData);
            return new ApplicationException("CSV conversion to JSON failed");
        }

        return JsonSerializer.Serialize(new { organisations = records });
    }

    private string AppendCsvHeader(string source, string headerLine)
    {
        var csvLines = source.SplitLines();

        if (csvLines.First() == headerLine)
            return source;

        var csvContent = csvLines.Prepend(headerLine);

        return string.Join(Environment.NewLine, csvContent);
    }

    private List<object?> ConvertCsvToOrgRecordResponse(string content, Type classType)
    {
        var sourceReader = new StringReader(content);
        using var csvReader = new CsvReader(sourceReader, CultureInfo.CurrentCulture);
        var records = csvReader.GetRecords(classType);

        return records.ToList();
    }
}