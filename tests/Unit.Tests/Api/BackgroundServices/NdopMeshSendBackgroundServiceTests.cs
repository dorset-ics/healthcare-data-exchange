using Api.BackgroundServices;
using Core.Ndop.Abstractions;
using Quartz;

namespace Unit.Tests.Api.BackgroundServices;

public class NdopMeshSendBackgroundServiceTests
{
    [Fact]
    public async Task Execute_CallsSendMeshMessages()
    {
        var ndopService = Substitute.For<INdopService>();
        var context = Substitute.For<IJobExecutionContext>();
        var service = new NdopMeshSendBackgroundService(ndopService);

        await service.Execute(context);

        await ndopService.Received().SendMeshMessages(Arg.Any<CancellationToken>());
    }
}