using Core.Pds.Abstractions;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace Api.BackgroundServices;

public class PdsMeshRetrieveBackgroundService(IPdsService pdsService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await pdsService.RetrieveMeshMessages(context.CancellationToken);
    }
}