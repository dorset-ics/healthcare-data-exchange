using Core.Common.Abstractions.Clients;
using Core.Common.Abstractions.Converters;
using Core.Common.Results;
using Core.Ods.Enums;
using Core.Ods.Models;
using Core.Ods.Strategies;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Core.Ods.Strategies;

public class OdsCsvImportStrategyTests
{
    private readonly ILogger<OdsCsvIngestionStrategy> _loggerMock;
    private readonly IDataHubFhirClient _fhirClientMock;
    private readonly IConverter<OdsCsvIngestionData, Result<string>> _oConverterMock;

    public OdsCsvImportStrategyTests()
    {
        _loggerMock = Substitute.For<ILogger<OdsCsvIngestionStrategy>>();
        _fhirClientMock = Substitute.For<IDataHubFhirClient>();
        _oConverterMock = Substitute.For<IConverter<OdsCsvIngestionData, Result<string>>>();
    }

    [Theory]
    [InlineData(OdsCsvDownloadSource.EnglandAndWales)]
    [InlineData(OdsCsvDownloadSource.Scotland)]
    [InlineData(OdsCsvDownloadSource.NorthernIreland)]
    public async Task ImportOrganisationCsv_WhenCalled_ShouldConvertData(OdsCsvDownloadSource odsCsvSource)
    {
        var sut = new OdsCsvIngestionStrategy(_loggerMock, _fhirClientMock, _oConverterMock);
        var downloadStream = new MemoryStream(("a,b,c" + Environment.NewLine).ToString().Select(c => (byte)c).ToArray());
        var orgData = OdsCsvIngestionData.GetDataBySource(("a,b,c" + Environment.NewLine), odsCsvSource);
        var badJsonData = new Result<string>(new Exception($"Error converting CSV to json for {odsCsvSource}"));

        _oConverterMock.Convert(orgData).Returns(badJsonData);

        await sut.Ingest(odsCsvSource, downloadStream);

        _oConverterMock.Received(1).Convert(orgData);
    }
}