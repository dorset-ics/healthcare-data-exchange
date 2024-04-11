using Core.Ods;
using Core.Ods.Abstractions;
using Core.Ods.Enums;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Core.Ods;

public class OdsServiceTests
{
    private readonly ILogger<OdsService> _logger;
    private readonly IOdsCsvDownloadClient _odsCsvDownloadClient;
    private readonly IOdsCsvIngestionStrategy _odsCsvIngestionStrategy;
    private readonly OdsService _sut;

    public OdsServiceTests()
    {
        _logger = Substitute.For<ILogger<OdsService>>();
        _odsCsvDownloadClient = Substitute.For<IOdsCsvDownloadClient>();
        _odsCsvIngestionStrategy = Substitute.For<IOdsCsvIngestionStrategy>();
        _sut = new OdsService(_logger, _odsCsvDownloadClient, _odsCsvIngestionStrategy);
    }

    [Fact]
    public async Task IngestCsvDownloads_WhenExecuted_DownloadsDataFromOdsClientForEachSource()
    {
        var ct = new CancellationToken();

        await _sut.IngestCsvDownloads(ct);

        await _odsCsvDownloadClient.Received(Enum.GetValues<OdsCsvDownloadSource>().Length)
            .DownloadOrganisationsFromCsvSource(Arg.Any<OdsCsvDownloadSource>(), ct);
    }

    [Fact]
    public async Task IngestCsvDownloads_WhenExecuted_IngestsFileForEachSource()
    {
        var ct = new CancellationToken();

        await _sut.IngestCsvDownloads(ct);

        await _odsCsvIngestionStrategy.Received(Enum.GetValues<OdsCsvDownloadSource>().Length)
            .Ingest(Arg.Any<OdsCsvDownloadSource>(), Arg.Any<Stream>());
    }
}