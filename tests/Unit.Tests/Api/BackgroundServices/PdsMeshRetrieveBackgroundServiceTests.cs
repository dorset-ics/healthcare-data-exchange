using Api.BackgroundServices;
using Core.Pds.Abstractions;
using Quartz;

namespace Unit.Tests.Api.BackgroundServices;

public class PdsMeshRetrieveBackgroundServiceTests
{
    [Fact]
    public async Task Execute_CallsRetrieveMeshMessages()
    {
        var pdsService = Substitute.For<IPdsService>();
        var context = Substitute.For<IJobExecutionContext>();
        var service = new PdsMeshRetrieveBackgroundService(pdsService);

        await service.Execute(context);

        await pdsService.Received().RetrieveMeshMessages(Arg.Any<CancellationToken>());
    }
}