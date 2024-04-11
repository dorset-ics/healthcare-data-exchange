using Api.BackgroundServices;
using Core.Ods.Abstractions;
using Quartz;

namespace Unit.Tests.Api.BackgroundServices;

public class OdsCsvDownloadBackgroundServiceTests
{
    [Fact]
    public async Task Execute_CallsRetrieveMeshMessages()
    {
        var odsService = Substitute.For<IOdsService>();
        var context = Substitute.For<IJobExecutionContext>();
        var service = new OdsCsvDownloadBackgroundService(odsService);

        await service.Execute(context);

        await odsService.Received().IngestCsvDownloads(Arg.Any<CancellationToken>());
    }
}