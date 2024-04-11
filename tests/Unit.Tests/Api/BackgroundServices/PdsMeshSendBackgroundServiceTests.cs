using Api.BackgroundServices;
using Core.Pds.Abstractions;
using Quartz;

namespace Unit.Tests.Api.BackgroundServices;

public class PdsMeshSendBackgroundServiceTests
{
    [Fact]
    public async Task Execute_CallsSendMeshMessages()
    {
        var pdsService = Substitute.For<IPdsService>();
        var context = Substitute.For<IJobExecutionContext>();
        var service = new PdsMeshSendBackgroundService(pdsService);

        await service.Execute(context);

        await pdsService.Received().SendMeshMessages(Arg.Any<CancellationToken>());
    }
}