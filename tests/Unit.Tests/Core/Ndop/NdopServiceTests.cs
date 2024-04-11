using Core;
using Core.Common.Abstractions.Clients;
using Core.Common.Abstractions.Converters;
using Core.Common.Extensions;
using Core.Common.Results;
using Core.Ndop;
using Core.Ndop.Abstractions;
using Core.Ndop.Models;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NEL.MESH.Models.Foundations.Mesh;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Core.Ndop;

public class NdopServiceTests
{
    private readonly ILogger<NdopService> _logger;
    private readonly INdopMeshClient _meshClient;
    private readonly IDataHubFhirClient _fhirClient;
    private readonly MemoryDistributedCache _distributedCache;
    private readonly IConverter<Bundle, Result<NdopMeshBundleToCsvConversionResult>> _bundleToCsvConverter;
    private readonly IConverter<NdopMeshConversionRequest, Result<string>> _csvToJsonConverter;
    private readonly NdopService _sut;

    private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
    private const string _fileName = "NDOPREQ_20210901120000_0000000000000001";
    private const string _dataFileName = _fileName + ".dat";
    private const string _ctlFileName = _fileName + ".ctl";
    public NdopServiceTests()
    {
        _logger = Substitute.For<ILogger<NdopService>>();
        _meshClient = Substitute.For<INdopMeshClient>();
        _fhirClient = Substitute.For<IDataHubFhirClient>();
        _distributedCache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
        _bundleToCsvConverter = Substitute.For<IConverter<Bundle, Result<NdopMeshBundleToCsvConversionResult>>>();
        _csvToJsonConverter = Substitute.For<IConverter<NdopMeshConversionRequest, Result<string>>>();

        _sut = new NdopService(_logger, _meshClient, _fhirClient, _distributedCache, _csvToJsonConverter, _bundleToCsvConverter);
    }

    #region RetrieveMessage Tests

    [Fact]
    public async Task WhenRetrievingMeshMessages_ThenMessagesAreRetrievedFromTheMeshClient()
    {
        var messages = new List<string>() { "message" };

        _meshClient.RetrieveMessages().Returns(messages);
        _meshClient.RetrieveMessage("message").Returns(new Message()
        {
            Headers = { { "mex-filename", new List<string> { _dataFileName } } },
            FileContent = "content"u8.ToArray(),
            TrackingInfo = new TrackingInfo() { LocalId = "id" }
        });

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _meshClient.Received(1).RetrieveMessages();
    }

    [Fact]
    public async Task GivenMeshClientFailsToRetrieveMessages_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _meshClient.RetrieveMessages().Returns(new Exception("Failure!"));
        _meshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "message"u8.ToArray() });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GivenMeshClientFailsToRetrieveMessage_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _meshClient.RetrieveMessages().Returns(new List<string>() { "message" });
        _meshClient.RetrieveMessage(Arg.Any<string>()).Returns(new Exception("Failure!"));
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GiveCsvToJsonConversionFails_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _meshClient.RetrieveMessages().Returns(new List<string> { "message" });
        _meshClient.RetrieveMessage("message").Returns(new Message
        {
            Headers = { { "mex-filename", new List<string> { _dataFileName } } },
            FileContent = "content"u8.ToArray(),
            TrackingInfo = new TrackingInfo { LocalId = "id" }
        });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received().AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GiveFhirConvertDataFails_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _meshClient.RetrieveMessages().Returns(new List<string> { "message" });
        _meshClient.RetrieveMessage("message").Returns(new Message
        {
            Headers = { { "mex-filename", new List<string> { _dataFileName } } },
            FileContent = "content"u8.ToArray(),
            TrackingInfo = new TrackingInfo() { LocalId = "id" }
        });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received().AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GivenFhirTransactionSucceeds_WhenRetrievingMeshMessages_ThenMessageIsAcknowledged()
    {
        var localId = "id";
        await _distributedCache.SetAsync(_fileName, localId, _distributedCacheEntryOptions);
        await _distributedCache.SetAsync(localId, new List<string> { "number1", "number2" }, _distributedCacheEntryOptions);
        _meshClient.RetrieveMessages().Returns(new List<string> { "message" });
        _meshClient.RetrieveMessage("message").Returns(new Message
        {
            Headers = { { "mex-filename", new List<string> { _dataFileName } } },
            FileContent = "content"u8.ToArray(),
            TrackingInfo = new TrackingInfo() { LocalId = localId }
        });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _fhirClient.Received(1).TransactionAsync<Consent>(Arg.Any<Bundle>());
        _logger.ReceivedWithAnyArgs().LogInformation("Message {message} acknowledged in MESH", "message");
    }

    [Fact]
    public async Task GivenEmptyContentReceived_WhenRetrievingMeshMessages_ThenMessageIsAcknowledged()
    {
        var localId = "id";
        await _distributedCache.SetAsync(_fileName, localId, _distributedCacheEntryOptions);
        await _distributedCache.SetAsync(localId, new List<string> { "number1", "number2" }, _distributedCacheEntryOptions);
        _meshClient.RetrieveMessages().Returns(new List<string> { "message" });
        _meshClient.RetrieveMessage("message").Returns(new Message
        {
            Headers = { { "mex-filename", new List<string> { _dataFileName } } },
            FileContent = ""u8.ToArray(),
            TrackingInfo = new TrackingInfo() { LocalId = localId }
        });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _fhirClient.Received(0).TransactionAsync<Consent>(Arg.Any<Bundle>());
        _logger.ReceivedWithAnyArgs().LogInformation("Message {message} acknowledged in MESH", "message");
    }

    [Fact]
    public async Task GivenControlFileReceived_WhenRetrievingMeshMessages_ThenCacheIsUpdatedAndMessageIsAcknowledged()
    {
        var localId = "id";
        _meshClient.RetrieveMessages().Returns(new List<string> { "message" });
        _meshClient.RetrieveMessage("message").Returns(new Message
        {
            Headers = { { "mex-filename", new List<string> { _ctlFileName } } },
            FileContent = "<?xml version=\"1.0\"?>\n<DTSControl>\n<LocalId>id</LocalId>\n \n</DTSControl>\n"u8.ToArray(),
            TrackingInfo = new TrackingInfo { LocalId = localId }
        });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _fhirClient.Received(0).TransactionAsync<Consent>(Arg.Any<Bundle>());
        var actualLocalId = await _distributedCache.GetAsync<string>(_fileName);
        actualLocalId.Should().Be(localId);
        _logger.ReceivedWithAnyArgs().LogInformation("Message {message} acknowledged in MESH", "message");
    }

    [Fact]
    public async Task GivenEmptyTraceFileReceived_WhenRetrievingMeshMessages_TheMessageIsAcknowledged()
    {
        _meshClient.RetrieveMessages().Returns(new List<string> { "message" });
        _meshClient.RetrieveMessage("message").Returns(new Message
        {
            Headers = { { "mex-filename", new List<string> { "filename" } } },
            FileContent = "content"u8.ToArray(),
            TrackingInfo = new TrackingInfo { LocalId = "id" }
        });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _fhirClient.Received(0).TransactionAsync<Consent>(Arg.Any<Bundle>());
        _logger.ReceivedWithAnyArgs().LogInformation("Message {message} acknowledged in MESH", "message");
    }

    [Fact]
    public async Task GiveFhirTransactionFails_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _meshClient.RetrieveMessages().Returns(new List<string> { "message" });
        _meshClient.RetrieveMessage("message").Returns(new Message
        {
            Headers = { { "mex-filename", new List<string> { _dataFileName } } },
            FileContent = "content"u8.ToArray(),
            TrackingInfo = new TrackingInfo() { LocalId = "id" }
        });
        _meshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received().AnyLogOfType(LogLevel.Error);
    }

    #endregion

    #region Cancellation and Exception Handling Tests

    [Fact]
    public async Task GivenCancellationRequested_WhenSendingMeshMessages_ThenOperationIsCancelled()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var bundle = new Bundle();
        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(bundle);

        await _sut.SendMeshMessages(cancellationTokenSource.Token);

        await _fhirClient.DidNotReceive().ContinueAsync(Arg.Any<Bundle>());
    }

    [Fact]
    public async Task GivenExceptionThrown_WhenSendingMeshMessages_ThenExceptionIsPropagated()
    {
        _fhirClient.When(x => x.SearchResourceByParams<Patient>(Arg.Any<SearchParams>())).Do(x => { throw new Exception("Test exception"); });

        await _sut.SendMeshMessages(new CancellationToken());

        await _meshClient.DidNotReceive().SendMessage(Arg.Any<string>());
    }

    #endregion

    #region SendMessage Tests

    [Fact]
    public async Task WhenSendingMeshMessages_ThenTheFhirClientIsSearched()
    {
        await _sut.SendMeshMessages(new CancellationToken());

        await _fhirClient.Received(1).SearchResourceByParams<Patient>(Arg.Any<SearchParams>());
    }

    [Fact]
    public async Task GivenNoResources_WhenSendingMeshMessages_ThenNoMessagesAreSent()
    {
        // Don't use _sut in this test, so we can use a mock of the cache.

        var distributedCacheMock = Substitute.For<IDistributedCache>();
        var ndopService = new NdopService(
            _logger, _meshClient, _fhirClient, distributedCacheMock, _csvToJsonConverter, _bundleToCsvConverter);

        await ndopService.SendMeshMessages(new CancellationToken());

        await _meshClient.DidNotReceive().SendMessage(Arg.Any<string>());

        await _meshClient.DidNotReceive().SendMessage(Arg.Any<string>());
        distributedCacheMock.DidNotReceive();
    }

    [Fact]
    public async Task GivenSinglePageOfResources_WhenSendingMeshMessages_ThenSingleMessageIsProcessed()
    {
        var bundle = new Bundle()
        {
            Entry = Enumerable.Range(0, Globals.FhirServerMaxPageSize).Select(index =>
                new Bundle.EntryComponent() { Resource = new Patient() { Id = $"Test Patient {index}" } }
            ).ToList()
        };
        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(bundle);
        _bundleToCsvConverter.Convert(Arg.Any<Bundle>()).Returns(
            new NdopMeshBundleToCsvConversionResult("content", new List<string> { "number1", "number2" }));
        _meshClient.SendMessage("content")
            .Returns(new Message { TrackingInfo = new TrackingInfo { LocalId = "localId" } });

        await _sut.SendMeshMessages(new CancellationToken());

        _bundleToCsvConverter.Received(1).Convert(bundle);
        await _meshClient.Received(1).SendMessage("content");
        await _fhirClient.Received(1).ContinueAsync(bundle);
        var async = await _distributedCache.GetAsync<List<string>>("localId");
        async.Should().BeEquivalentTo(new List<string> { "number1", "number2" });
    }

    [Fact]
    public async Task GivenPartialPageOfResources_WhenSendingMeshMessages_ThenTheFhirClientSearchIsNotContinued()
    {
        var bundle = new Bundle()
        {
            Entry = Enumerable.Range(0, Globals.FhirServerMaxPageSize - 1).Select(index =>
                new Bundle.EntryComponent() { Resource = new Patient() { Id = $"Test Patient {index}" } }
            ).ToList()
        };

        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(bundle);

        await _sut.SendMeshMessages(new CancellationToken());

        await _fhirClient.DidNotReceive().ContinueAsync(bundle);
    }

    [Fact]
    public async Task GivenMultiplePagesOfResources_WhenSendingMeshMessages_MessagesAreProcessedAndSentByThePageCount()
    {
        var nhsNumbers = new List<string> { "number1", "number2" };
        var bundle = new Bundle
        {
            Entry = Enumerable.Range(0, Globals.FhirServerMaxPageSize).Select(index =>
                new Bundle.EntryComponent { Resource = new Patient { Id = $"Test Patient {index}" } }
            ).ToList()
        };
        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(bundle);
        var messagesSent = 0;
        _fhirClient.ContinueAsync(Arg.Any<Bundle>()).Returns(_ =>
        {
            messagesSent++;
            return messagesSent < 3 ? bundle : null;
        });
        _bundleToCsvConverter.Convert(Arg.Any<Bundle>()).Returns(
            new NdopMeshBundleToCsvConversionResult("content", nhsNumbers));
        _meshClient.SendMessage("content")
            .Returns(new Message { TrackingInfo = new TrackingInfo { LocalId = "localId" } });
        await _sut.SendMeshMessages(new CancellationToken());

        _bundleToCsvConverter.Received(3).Convert(bundle);
        await _meshClient.Received(3).SendMessage("content");
        await _fhirClient.Received(3).ContinueAsync(bundle);
        var value = await _distributedCache.GetAsync<List<string>>("localId");
        value.Should().BeEquivalentTo(nhsNumbers);
    }

    #endregion
}