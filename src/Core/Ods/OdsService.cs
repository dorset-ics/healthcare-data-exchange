using Core.Ods.Abstractions;
using Core.Ods.Enums;
using Microsoft.Extensions.Logging;

namespace Core.Ods;

public class OdsService(
    ILogger<OdsService> logger,
    IOdsCsvDownloadClient odsCsvDownloadClient,
    IOdsCsvIngestionStrategy odsCsvIngestionStrategy) : IOdsService
{
    public async Task IngestCsvDownloads(CancellationToken cancellationToken)
    {
        logger.LogInformation("Ingesting organisations from ODS CSV downloads");

        var downloadSources = Enum.GetValues<OdsCsvDownloadSource>();

        await Task.WhenAll(downloadSources.Select(source => IngestOrganisationsFromCsvDownloadSource(source, cancellationToken)));
    }

    private async Task IngestOrganisationsFromCsvDownloadSource(OdsCsvDownloadSource odsDownloadSource, CancellationToken cancellationToken)
    {
        logger.LogInformation("Ingesting {OdsDownloadSource} organisations from CSV downloads", odsDownloadSource);

        try
        {
            var downloadStream =
                await odsCsvDownloadClient.DownloadOrganisationsFromCsvSource(odsDownloadSource, cancellationToken);

            await odsCsvIngestionStrategy.Ingest(odsDownloadSource, downloadStream);

            logger.LogInformation("Successfully ingested {OdsDownloadSource} organisations from CSV downloads", odsDownloadSource);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error ingesting {odsDownloadSource} organisations from CSV downloads");
            throw;
        }
    }
}