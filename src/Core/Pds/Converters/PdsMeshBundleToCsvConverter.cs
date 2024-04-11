using System.Globalization;
using Core.Common.Abstractions.Converters;
using Core.Common.Results;
using Core.Pds.Extensions;
using Core.Pds.Models;
using CsvHelper;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace Core.Pds.Converters;

public class PdsMeshBundleToCsvConverter(ILogger<PdsMeshBundleToCsvConverter> logger) : IConverter<Bundle, Result<PdsMeshBundleToCsvConversionResult>>
{
    public Result<PdsMeshBundleToCsvConversionResult> Convert(Bundle source)
    {
        if (source == null)
        {
            logger.LogError("Error converting PDS bundle to CSV: {source} argument is null", nameof(source));

            return new ArgumentNullException(nameof(source));
        }

        using TextWriter writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);

        csv.WriteRecords(
            source.Entry
                .Select(t => t.Resource.ToPdsMeshRecord()));

        var csvString = writer.ToString()!;

        return new PdsMeshBundleToCsvConversionResult(csvString);
    }
}