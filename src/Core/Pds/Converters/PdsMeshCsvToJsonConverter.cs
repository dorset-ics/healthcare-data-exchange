using System.Globalization;
using System.Text.Json;
using Core.Common.Abstractions.Converters;
using Core.Common.Extensions;
using Core.Common.Results;
using Core.Pds.Models;
using Core.Pds.Utilities;
using CsvHelper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ValidationException = FluentValidation.ValidationException;

namespace Core.Pds.Converters;

public class PdsMeshCsvToJsonConverter(ILogger<PdsMeshCsvToJsonConverter> logger, IValidator<string> validator)
    : IConverter<string, Result<string>>
{
    public Result<string> Convert(string source)
    {
        if (source == null)
        {
            logger.LogError("Error converting NDOP bundle to CSV: {source} argument is null", nameof(source));
            return new ArgumentNullException(nameof(source));
        }

        var validationResult = validator.Validate(source);
        if (!validationResult.IsValid)
            return new ValidationException(string.Join("\n", validationResult.Errors.Select(error => error.ErrorMessage).ToList()));

        var headers = PdsMeshUtilities.GetPdsMeshRecordResponseHeaderLine();

        var csvContent = GetCsvContent(source, headers);

        var records = ConvertCsvToPdsMeshRecordResponse(csvContent);

        if (!records.Any())
        {
            logger.LogWarning("CSV {csv} conversion to JSON failed", source);

            return new ApplicationException("CSV conversion to JSON failed");
        }

        return JsonSerializer.Serialize(new { patients = records });
    }

    private string GetCsvContent(string source, string headerLine)
    {
        var csvLines = source.SplitLines().Skip(1);

        if (csvLines.First() == headerLine)
            return source;

        var csvContent = csvLines.Prepend(headerLine);

        return string.Join(Environment.NewLine, csvContent);
    }

    private List<PdsMeshRecordResponse> ConvertCsvToPdsMeshRecordResponse(string source)
    {
        var sourceReader = new StringReader(source);
        using var csvReader = new CsvReader(sourceReader, CultureInfo.CurrentCulture);
        var records = csvReader.GetRecords<PdsMeshRecordResponse>();

        return records.ToList();
    }
}