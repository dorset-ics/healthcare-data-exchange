using Core;
using Core.Common.Abstractions.Clients;
using Core.Common.Abstractions.Converters;
using Core.Common.Models;
using Core.Common.Results;
using Core.Pds;
using Core.Pds.Abstractions;
using Core.Pds.Converters;
using Core.Pds.Models;
using FluentValidation;
using FluentValidation.Results;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using NEL.MESH.Models.Foundations.Mesh;
using static Hl7.Fhir.Model.Bundle;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Core.Pds;

public class PdsServiceTests
{
    private readonly ILogger<PdsService> _logger;
    private readonly IPdsMeshClient _pdsMeshClient;
    private readonly IDataHubFhirClient _fhirClient;
    private readonly IPdsFhirClient _pdsFhirClient;
    private readonly IConverter<string, Result<string>> _csvToJsonConverter;
    private readonly IConverter<Bundle, Result<PdsMeshBundleToCsvConversionResult>> _bundleToCsvConverter;
    private readonly PdsMeshCsvToJsonConverter _pdsMeshCsvToJsonConverter;
    private readonly IValidator<string> _validatorMock;

    private readonly Message _message;
    private readonly PdsService _sut;

    private const string BaseSamplePath = "Core/Pds/Converters/Samples";

    public PdsServiceTests()
    {
        _logger = Substitute.For<ILogger<PdsService>>();
        _pdsMeshClient = Substitute.For<IPdsMeshClient>();
        _pdsFhirClient = Substitute.For<IPdsFhirClient>();
        _fhirClient = Substitute.For<IDataHubFhirClient>();
        _csvToJsonConverter = Substitute.For<IConverter<string, Result<string>>>();
        _bundleToCsvConverter = Substitute.For<IConverter<Bundle, Result<PdsMeshBundleToCsvConversionResult>>>();
        _message = Substitute.For<Message>();

        var loggerMock = Substitute.For<ILogger<PdsMeshCsvToJsonConverter>>();
        _validatorMock = Substitute.For<IValidator<string>>();
        _validatorMock.Validate(Arg.Any<string>()).Returns(new ValidationResult());
        _pdsMeshCsvToJsonConverter = new PdsMeshCsvToJsonConverter(loggerMock, _validatorMock);

        _sut = new PdsService(
            _logger, _pdsMeshClient, _pdsFhirClient, _fhirClient, _csvToJsonConverter, _bundleToCsvConverter);
    }

    #region RetrieveMessage Tests

    [Fact]
    public async Task WhenRetrievingMeshMessages_ThenMessagesAreRetrievedFromTheMeshClient()
    {
        var messages = new List<string>() { "message" };

        _pdsMeshClient.RetrieveMessages().Returns(messages);
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "content"u8.ToArray() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseSinglePatient.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);

        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _pdsMeshClient.Received(1).RetrieveMessages();
    }

    [Fact]
    public async Task GivenNoMessages_WhenRetrievingMeshMessages_ThenNoMessagesAreProcessed()
    {
        var messages = new List<string>();

        _pdsMeshClient.RetrieveMessages().Returns(messages);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _csvToJsonConverter.Received(0).Convert(Arg.Any<string>());
        await _fhirClient.Received(0).ConvertData(Arg.Any<ConvertDataRequest>());
        await _fhirClient.Received(0).UpdateResource(Arg.Any<Bundle>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenSingleMessage_WhenRetrievingMeshMessages_ThenMessageIsProcessed(bool acknowledgeSucceeded)
    {
        var messages = new List<string>() { "message" };

        _pdsMeshClient.RetrieveMessages().Returns(messages);
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "content"u8.ToArray() });
        _pdsMeshClient.AcknowledgeMessage("message").Returns(acknowledgeSucceeded);

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseSinglePatient.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);
        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _fhirClient.Received(1).ConvertData(Arg.Any<ConvertDataRequest>());
        await _fhirClient.Received(1).TransactionAsync<Patient>(Arg.Any<Bundle>());
        await _pdsMeshClient.Received(1).AcknowledgeMessage("message");
    }


    [Fact]
    public async Task GivenMultipleMessages_WhenRetrievingMeshMessages_ThenMessagesAreProcessed()
    {
        var messages = new List<string>() { "message 1", "message 2", "message 3" };

        _pdsMeshClient.RetrieveMessages().Returns(messages);
        _pdsMeshClient.RetrieveMessage(Arg.Any<string>()).Returns(new Message() { FileContent = "content"u8.ToArray() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseMultiplePatients.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _csvToJsonConverter.Received(3).Convert(Arg.Any<string>());
        await _fhirClient.Received(3).ConvertData(Arg.Any<ConvertDataRequest>());
        await _fhirClient.Received(3).TransactionAsync<Patient>(Arg.Any<Bundle>());
        await _pdsMeshClient.Received(1).AcknowledgeMessage("message 1");
        await _pdsMeshClient.Received(1).AcknowledgeMessage("message 2");
        await _pdsMeshClient.Received(1).AcknowledgeMessage("message 3");
    }

    [Fact]
    public async Task GivenMeshClientFailsToRetrieveMessages_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.RetrieveMessages().Returns(new Exception("Failure!"));
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "message"u8.ToArray() });
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns("conversion result");
        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle());
        _fhirClient.TransactionAsync<Patient>(Arg.Any<Bundle>()).Returns(new Bundle());
        _pdsMeshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GivenMeshClientFailsToRetrieveMessage_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.RetrieveMessages().Returns(new List<string>() { "message" });
        _pdsMeshClient.RetrieveMessage(Arg.Any<string>()).Returns(new Exception("Failure!"));
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns("conversion result");
        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle());
        _fhirClient.TransactionAsync<Patient>(Arg.Any<Bundle>()).Returns(new Bundle());
        _pdsMeshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GiveCsvToJsonConversionFails_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.RetrieveMessages().Returns(new List<string>() { "message" });
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "message"u8.ToArray() });
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(new Exception("Failure!"));
        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle());
        _fhirClient.TransactionAsync<Patient>(Arg.Any<Bundle>()).Returns(new Bundle());
        _pdsMeshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GiveFhirConvertDataFails_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.RetrieveMessages().Returns(new List<string>() { "message" });
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "message"u8.ToArray() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseSinglePatient.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);
        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Exception("Failure!"));
        _fhirClient.TransactionAsync<Patient>(Arg.Any<Bundle>()).Returns(new Bundle());
        _pdsMeshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GiveFhirTransactionFails_WhenRetrievingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.RetrieveMessages().Returns(new List<string>() { "message" });
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "message"u8.ToArray() });
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseSinglePatient.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);
        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle());
        _fhirClient.TransactionAsync<Patient>(Arg.Any<Bundle>()).Returns(new Exception("Failure!"));
        _pdsMeshClient.AcknowledgeMessage(Arg.Any<string>()).Returns(true);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    #endregion

    #region Cancellation and Exception Handling Tests

    [Fact]
    public async Task GivenCancellationRequested_WhenRetrievingMeshMessages_ThenOperationIsCancelled()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var messages = new List<string>() { "message 1", "message 2", "message 3" };
        _pdsMeshClient.RetrieveMessages().Returns(messages);

        await _sut.RetrieveMeshMessages(cancellationTokenSource.Token);
        _csvToJsonConverter.DidNotReceive().Convert(Arg.Any<string>());
    }

    [Fact]
    public async Task GivenRetrieveMessagesFails_WhenRetrievingMeshMessages_ThenExceptionIsPropagated()
    {
        _pdsMeshClient.RetrieveMessages().Returns(new Exception("Test exception"));

        await _sut.RetrieveMeshMessages(new CancellationToken());
        await _fhirClient.DidNotReceive().UpdateResource(Arg.Any<Bundle>());
    }

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
        _fhirClient.When(x => x.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()))
            .Do(x => { throw new Exception("Test exception"); });

        await _sut.SendMeshMessages(new CancellationToken());
        await _pdsMeshClient.DidNotReceive().SendMessage(Arg.Any<string>());
    }

    #endregion

    #region SendMeshMessages Tests

    [Fact]
    public async Task WhenSendingMeshMessages_ThenTheFhirClientIsSearched()
    {
        await _sut.SendMeshMessages(new CancellationToken());

        await _fhirClient.Received(1).SearchResourceByParams<Patient>(Arg.Any<SearchParams>());
    }

    [Fact]
    public async Task GivenNoResources_WhenSendingMeshMessages_ThenNoMessagesAreSent()
    {
        await _sut.SendMeshMessages(new CancellationToken());

        await _pdsMeshClient.Received(0).SendMessage(Arg.Any<string>());
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
        _bundleToCsvConverter.Convert(Arg.Any<Bundle>()).Returns(new PdsMeshBundleToCsvConversionResult("content"));
        _pdsMeshClient.SendMessage(Arg.Any<string>()).Returns(new Message());

        await _sut.SendMeshMessages(new CancellationToken());

        _bundleToCsvConverter.Received(1).Convert(bundle);
        await _pdsMeshClient.Received(1).SendMessage("content");
        await _fhirClient.Received(1).ContinueAsync(bundle);
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

        await _fhirClient.Received(0).ContinueAsync(bundle);
    }

    [Fact]
    public async Task GivenMultiplePagesOfResources_WhenSendingMeshMessages_ThenMultipleMessagesAreProcessed()
    {
        var bundle = new Bundle()
        {
            Entry = Enumerable.Range(0, Globals.FhirServerMaxPageSize).Select(index =>
                new Bundle.EntryComponent() { Resource = new Patient() { Id = $"Test Patient {index}" } }
            ).ToList()
        };

        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(bundle);
        _pdsMeshClient.SendMessage(Arg.Any<string>()).Returns(new Message());

        var messagesSent = 0;

        _fhirClient.ContinueAsync(Arg.Any<Bundle>()).Returns(callInfo =>
        {
            messagesSent++;

            return messagesSent < 3 ? bundle : null;
        });

        _bundleToCsvConverter.Convert(Arg.Any<Bundle>()).Returns(new PdsMeshBundleToCsvConversionResult("content"));

        await _sut.SendMeshMessages(new CancellationToken());

        _bundleToCsvConverter.Received(3).Convert(bundle);
        await _pdsMeshClient.Received(3).SendMessage("content");
        await _fhirClient.Received(3).ContinueAsync(bundle);
    }

    [Fact]
    public async Task GiveMeshClientFailsToSendMessage_WhenSendingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.SendMessage(Arg.Any<string>()).Returns(new Exception("Failure!"));
        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(new Bundle());
        _bundleToCsvConverter.Convert(Arg.Any<Bundle>()).Returns(new PdsMeshBundleToCsvConversionResult("conversion result"));

        await _sut.SendMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GiveFhirClientCantSearch_WhenSendingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.SendMessage(Arg.Any<string>()).Returns(_message);
        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(new Exception("Failure!"));
        _bundleToCsvConverter.Convert(Arg.Any<Bundle>()).Returns(new PdsMeshBundleToCsvConversionResult("conversion result"));

        await _sut.SendMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    [Fact]
    public async Task GiveBundleToCsvConversionFails_WhenSendingMeshMessages_ThenErrorIsLogged()
    {
        _pdsMeshClient.SendMessage(Arg.Any<string>()).Returns(_message);
        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(new Bundle());
        _bundleToCsvConverter.Convert(Arg.Any<Bundle>()).Returns(new Exception("Failure!"));

        await _sut.SendMeshMessages(new CancellationToken());

        _logger.Received(1).AnyLogOfType(LogLevel.Error);
    }

    #endregion


    #region Merged and Deleted patients tests

    [Fact]
    public async Task GivenRetrieveMeshMessageHadDeletedPatient_WhenDeletePatientIsCalled_ThenDeleteSucceeeds()
    {
        var messages = new List<string>() { "message" };

        _pdsMeshClient.RetrieveMessages().Returns(messages);
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "content"u8.ToArray() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseDeletedPatient.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);
        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle { Entry = [] });

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _pdsMeshClient.Received(1).RetrieveMessages();
        await _fhirClient.Received(1).ConvertData(Arg.Is<ConvertDataRequest>(request => request.Input == "{\"patients\":[]}"));
        await _fhirClient.Received(1).TransactionAsync<Patient>(Arg.Is<Bundle>(bundle => bundle.Entry.Count == 1
            && bundle.Entry[0].Request.Url == "Patient/9990554412"
            && bundle.Entry[0].Request.Method == Bundle.HTTPVerb.DELETE));
    }

    [Fact]
    public async Task GivenRetrieveMeshMessageHadMergedPatient_WhenDeletePatientIsCalled_ThenDeleteSucceeeds()
    {
        var messages = new List<string>() { "message" };

        _pdsMeshClient.RetrieveMessages().Returns(messages);
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "content"u8.ToArray() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseMergedPatient.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);

        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle
        {
            Entry =
            [
                new()
                {
                    Resource = new Patient { Id = "1234567890" },
                    Request = new RequestComponent { Method = Bundle.HTTPVerb.PUT, Url = "Patient/1234567890" }
                }
            ]
        });

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _pdsMeshClient.Received(1).RetrieveMessages();
        await _fhirClient.Received(1).ConvertData(Arg.Any<ConvertDataRequest>());
        await _fhirClient.Received(1).TransactionAsync<Patient>(Arg.Is<Bundle>(bundle => bundle.Entry.Count == 2
            && bundle.Entry[0].Request.Url == "Patient/1234567890"
            && bundle.Entry[0].Request.Method == Bundle.HTTPVerb.PUT
            && bundle.Entry[1].Request.Url == "Patient/9990554412"
            && bundle.Entry[1].Request.Method == Bundle.HTTPVerb.DELETE));
    }

    [Fact]
    public async Task GivenRetrieveMeshMessageHadDeletedAndMergedPatients_WhenDeletePatientIsCalled_ThenDeleteSucceeeds()
    {
        var messages = new List<string>() { "message" };

        _pdsMeshClient.RetrieveMessages().Returns(messages);
        _pdsMeshClient.RetrieveMessage("message").Returns(new Message() { FileContent = "content"u8.ToArray() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BaseSamplePath, "MeshResponseDeletedAndMergedPatients.csv");
        var fileContent = await File.ReadAllTextAsync(filePath);
        var result = _pdsMeshCsvToJsonConverter.Convert(fileContent);
        _csvToJsonConverter.Convert(Arg.Any<string>()).Returns(result);

        _fhirClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle
        {
            Entry =
            [
                new()
                {
                    Resource = new Patient { Id = "1234567890" },
                    Request = new RequestComponent { Method = Bundle.HTTPVerb.PUT, Url = "Patient/1234567890" }
                }
            ]
        });

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await _pdsMeshClient.Received(1).RetrieveMessages();
        await _fhirClient.Received(1).ConvertData(Arg.Any<ConvertDataRequest>());
        await _fhirClient.Received(1).TransactionAsync<Patient>(Arg.Is<Bundle>(bundle => bundle.Entry.Count == 3
            && bundle.Entry[0].Request.Url == "Patient/1234567890"
            && bundle.Entry[0].Request.Method == Bundle.HTTPVerb.PUT
            && bundle.Entry[1].Request.Url == "Patient/9990554414"
            && bundle.Entry[1].Request.Method == Bundle.HTTPVerb.DELETE
            && bundle.Entry[2].Request.Url == "Patient/9990554412"
            && bundle.Entry[2].Request.Method == Bundle.HTTPVerb.DELETE));
    }

    #endregion

}