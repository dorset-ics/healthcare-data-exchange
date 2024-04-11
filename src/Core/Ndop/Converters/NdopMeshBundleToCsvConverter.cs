using System.Globalization;
using Core.Common.Abstractions.Converters;
using Core.Common.Extensions;
using Core.Common.Results;
using Core.Ndop.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace Core.Ndop.Converters;

public class NdopMeshBundleToCsvConverter(ILogger<NdopMeshBundleToCsvConverter> logger) : IConverter<Bundle, Result<NdopMeshBundleToCsvConversionResult>>
{
    public Result<NdopMeshBundleToCsvConversionResult> Convert(Bundle source)
    {
        if (source == null)
        {
            logger.LogError("Error converting NDOP bundle to CSV: {source} argument is null", nameof(source));

            return new ArgumentNullException(nameof(source));
        }

        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false
        };

        using TextWriter writer = new StringWriter();
        using var csv = new CsvWriter(writer, csvConfig);

        var records = source.Entry
            .Select(e => e.Resource as Patient)
            .Where(p => p != null)
            .Select(p => p!.GetNhsNumber());

        csv.WriteRecords(records.Select(n => new { NHSNumber = n, BlankValue = "" }));

        var csvString = writer.ToString()!;

        return new NdopMeshBundleToCsvConversionResult(csvString, records);
    }
}