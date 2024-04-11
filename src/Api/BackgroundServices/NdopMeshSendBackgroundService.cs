using Core.Ndop.Abstractions;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace Api.BackgroundServices;

public class NdopMeshSendBackgroundService(INdopService ndopService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await ndopService.SendMeshMessages(context.CancellationToken);
    }
}