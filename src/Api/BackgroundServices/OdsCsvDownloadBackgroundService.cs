using Core.Ods.Abstractions;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace Api.BackgroundServices;

public class OdsCsvDownloadBackgroundService(IOdsService odsService, ILogger<OdsCsvDownloadBackgroundService> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting ODS CSV download ingestion job");
        await odsService.IngestCsvDownloads(context.CancellationToken);
    }
}