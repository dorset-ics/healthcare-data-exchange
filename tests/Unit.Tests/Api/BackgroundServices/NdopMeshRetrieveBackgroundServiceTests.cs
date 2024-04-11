using Api.BackgroundServices;
using Core.Ndop.Abstractions;
using Quartz;

namespace Unit.Tests.Api.BackgroundServices;

public class NdopMeshRetrieveBackgroundServiceTests
{
    [Fact]
    public async Task Execute_CallsRetrieveMeshMessages()
    {
        var ndopService = Substitute.For<INdopService>();
        var context = Substitute.For<IJobExecutionContext>();
        var service = new NdopMeshRetrieveBackgroundService(ndopService);

        await service.Execute(context);

        await ndopService.Received().RetrieveMeshMessages(Arg.Any<CancellationToken>());
    }
}