using Core.Ods.Abstractions;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace Api.BackgroundServices;

public class OdsCsvDownloadBackgroundService(IOdsService odsService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await odsService.IngestCsvDownloads(context.CancellationToken);
    }
}