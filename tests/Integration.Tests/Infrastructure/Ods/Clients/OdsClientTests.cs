using System.Globalization;
using Core.Ods.Abstractions;
using Core.Ods.Enums;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Tests.Infrastructure.Ods.Clients;

public class OdsClientTests : IDisposable
{
    private readonly ApiWebApplicationFactory _webApplicationFactory;

    private readonly IOdsCsvDownloadClient _odsCsvDownloadClient;

    public OdsClientTests()
    {
        _webApplicationFactory = new ApiWebApplicationFactory();
        _odsCsvDownloadClient = _webApplicationFactory.Services.GetService<IOdsCsvDownloadClient>()
                     ?? throw new Exception("Failed to resolve IOdsCsvDownloadClient from the service provider.");
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(OdsCsvDownloadSource.EnglandAndWales)]
    [InlineData(OdsCsvDownloadSource.NorthernIreland)]
    [InlineData(OdsCsvDownloadSource.Scotland)]
    public async Task DownloadOrganisationsFromSourceAsync_GivenCsvOdsDownloadSource_ShouldReturnStream(OdsCsvDownloadSource odsCsvSource)
    {
        await DownloadOrganisationsFromSourceAsync_GivenOdsDownloadSource_ShouldReturnStream(odsCsvSource);
    }

    private async Task DownloadOrganisationsFromSourceAsync_GivenOdsDownloadSource_ShouldReturnStream(OdsCsvDownloadSource odsCsvSource)
    {
        using var resultStream = await _odsCsvDownloadClient.DownloadOrganisationsFromCsvSource(odsCsvSource, CancellationToken.None);
        using var streamReader = new StreamReader(resultStream);
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        csv.Read().ShouldBeTrue();
        csv.GetField<string>(0).ShouldNotBeNullOrWhiteSpace();
    }
}
