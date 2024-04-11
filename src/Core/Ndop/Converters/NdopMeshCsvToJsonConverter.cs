using System.Globalization;
using System.Text.Json;
using Core.Common.Abstractions.Converters;
using Core.Common.Results;
using Core.Ndop.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Ndop.Converters;

public class NdopMeshCsvToJsonConverter(ILogger<NdopMeshCsvToJsonConverter> logger) : IConverter<NdopMeshConversionRequest, Result<string>>
{
    public Result<string> Convert(NdopMeshConversionRequest source)
    {
        if (source == null)
        {
            logger.LogError("Error converting NDOP bundle to CSV: {source} argument is null", nameof(source));
            return new ArgumentNullException(nameof(source));
        }

        var (csv, requestIdsSentToMesh) = source;
        var optedInRecords = ConvertCsvToNdopMeshRecordResponse(csv);
        if (!optedInRecords.Any())
        {
            logger.LogWarning("CSV {csv} conversion to JSON failed", source);
            return new ApplicationException("CSV conversion to JSON failed");
        }

        var optedInNhsNumbers = optedInRecords.Select(record => record.NhsNumber).ToList();
        var enrichedRequestIds = requestIdsSentToMesh
            .Select(nhsNumber => new NdopMeshEnrichedRecordResponse(NhsNumber: nhsNumber, IsOptedOut: !optedInNhsNumbers.Contains(nhsNumber))).ToList();

        return JsonSerializer.Serialize(new { consents = enrichedRequestIds });
    }

    private List<NdopMeshRecordResponse> ConvertCsvToNdopMeshRecordResponse(string source)
    {
        using var csvReader = new CsvReader(new StringReader(source), new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = false });
        return csvReader.GetRecords<NdopMeshRecordResponse>().ToList();
    }
}