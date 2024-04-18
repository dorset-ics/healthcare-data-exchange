using Core.Ods.Abstractions;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.DataHub.Clients.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests.Core.Ods;

public class OdsServiceTests : IDisposable
{
    private readonly ApiWebApplicationFactory _webApplicationFactory;
    private readonly IOdsService _sut;
    private readonly IFhirClientWrapper _fhirClientWrapper;

    public OdsServiceTests()
    {
        _webApplicationFactory = new ApiWebApplicationFactory();
        _fhirClientWrapper = _webApplicationFactory.Services.GetService<IFhirClientWrapper>()!;
        _sut = _webApplicationFactory.Services.GetService<IOdsService>()
               ?? throw new Exception("Failed to resolve IOdsService from the service provider");
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GivenValidOdsDownloadSource_WhenIngestingCsvDownload_ThenResultIsSuccessful()
    {
        var dateIngestStarted = DateTime.Now;

        // The complete CSV ingest currently takes a long time and this test can take 25 mins to complete.
        // So run it in a task, iterate for a maximum amount of time, query the fhir store at intervals, and
        // as soon as we find something bomb out and assert.

        var task = new Task(async () => await _sut.IngestCsvDownloads(new CancellationToken()));

        task.Start();

        Bundle? bundle = null;

        while (DateTime.Now <= dateIngestStarted.AddSeconds(60))
        {
            bundle =
                await _fhirClientWrapper.SearchResourceByParams<Organization>(
                    new SearchParams().Where($"_lastUpdated=ge{dateIngestStarted.ToString("yyyy-MM-ddTHH:mm:ss")}"));

            if (bundle?.Entry.Count > 0)
                break;

            Thread.Sleep(500);
        }

        bundle?.Should().NotBeNull();
        bundle?.Entry.Count.ShouldBeGreaterThan(0);
    }
}