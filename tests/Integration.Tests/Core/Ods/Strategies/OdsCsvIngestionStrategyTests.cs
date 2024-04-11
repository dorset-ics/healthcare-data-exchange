using System.Security.Cryptography;
using Core.Ods.Abstractions;
using Core.Ods.Enums;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.DataHub.Clients.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests.Core.Ods.Strategies;

public class OdsCsvIngestionStrategyTests : IDisposable
{
    private const string BaseSamplePath = "Core/Ods/Strategies/Samples";
    private readonly ApiWebApplicationFactory _webApplicationFactory;
    private readonly IOdsCsvIngestionStrategy _sut;
    private readonly IDataHubFhirClientWrapper _dataHubFhirClientWrapper;

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    public OdsCsvIngestionStrategyTests()
    {
        _webApplicationFactory = new ApiWebApplicationFactory();
        _dataHubFhirClientWrapper = _webApplicationFactory.Services.GetService<IDataHubFhirClientWrapper>()!;
        _sut = _webApplicationFactory.Services.GetService<IOdsCsvIngestionStrategy>()
               ?? throw new Exception("Failed to resolve IOdsCsvImportStrategy from the service provider");
    }

    [Theory]
    [InlineData("etrust.csv", OdsCsvDownloadSource.EnglandAndWales)]
    [InlineData("hospitals.csv", OdsCsvDownloadSource.Scotland)]
    [InlineData("niorg.csv", OdsCsvDownloadSource.NorthernIreland)]
    public async Task Ingest_WhenCalled_ShouldPersistOrganisationsInDataHub(string fileName, OdsCsvDownloadSource odsCsvSource)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, fileName);

        using var reader = new StreamReader(filePath);
        await _sut.Ingest(odsCsvSource, reader.BaseStream);

        var fileContent = await File.ReadAllTextAsync(filePath);
        await CheckOrganizationExists(fileContent);
    }

    private async Task CheckOrganizationExists(string fileContent)
    {
        var lines = fileContent.Split(Environment.NewLine);

        foreach (var line in lines)
        {
            if (line.Length > 0)
            {
                var orgId = line.Split(",")[0].Replace("\"", string.Empty);
                var orgBundle = await _dataHubFhirClientWrapper.SearchResourceByParams<Organization>(
                        new SearchParams().Where($"identifier={orgId}"));

                orgBundle.Should().NotBeNull();
                orgBundle?.Entry.Count(e => ((Organization)e.Resource).Identifier.Any(id => id.Value == orgId)).ShouldBe(1);
            }
        }
    }
}
