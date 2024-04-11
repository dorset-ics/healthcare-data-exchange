using System.Net.Mime;
using System.Text;
using Core;
using Infrastructure.Pds.Configuration;
using Infrastructure.Pds.Fhir.Configuration;
using Infrastructure.Pds.Mesh.Clients;
using Infrastructure.Pds.Mesh.Configuration;
using Microsoft.Extensions.Logging;
using NEL.MESH.Clients;
using NEL.MESH.Clients.Mailboxes;
using NEL.MESH.Models.Foundations.Mesh;
using NSubstitute.ExceptionExtensions;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Infrastructure.Pds.Mesh.Clients;

public class PdsMeshClientTests
{
    private readonly IMeshClient _meshClient;
    private readonly PdsMeshClient _pdsMeshClient;

    private const string SendSchedule = "SendSchedule";
    private const string RetrieveSchedule = "RetrieveSchedule";
    private const string MailboxId = "X26ABC2";
    private const string MailboxPassword = "test";
    private const string MailboxKey = "Key";
    private const string WorkflowId = "Workflow";

    private readonly PdsMeshConfiguration _pdsMeshConfiguration =
        new(SendSchedule: SendSchedule, RetrieveSchedule: RetrieveSchedule, MailboxId: MailboxId, MailboxPassword: MailboxPassword, Key: MailboxKey, WorkflowId: WorkflowId);

    private readonly IMailboxClient _mailboxClient;

    public PdsMeshClientTests()
    {
        var loggerMock = Substitute.For<ILogger<PdsMeshClient>>();
        _meshClient = Substitute.For<IMeshClient>();
        _mailboxClient = Substitute.For<IMailboxClient>();
        _meshClient.Mailbox.Returns(_mailboxClient);
        _pdsMeshClient = new PdsMeshClient(loggerMock, _meshClient,
            new PdsConfiguration(new PdsFhirConfiguration(string.Empty, new PdsAuthConfiguration()), _pdsMeshConfiguration));
    }

    [Fact]
    public async Task SendMessage_WhenSent_ShouldCallMeshClient()
    {
        var content = "message content";
        _mailboxClient.SendMessageAsync(
            MailboxId,
            WorkflowId,
            content,
            "",
            "",
            Arg.Is<string>(x => x.StartsWith(Globals.PdsMeshMessageFileNamePrefix)),
            "",
            MediaTypeNames.Text.Csv).Returns(new Message { MessageId = "test" });

        var result = await _pdsMeshClient.SendMessage(content);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MessageId.ShouldBe("test");
    }

    [Fact]
    public async Task RetrieveMessages_ShouldCallMeshClient()
    {
        var expectedMessages = new List<string> { "message1", "message2" };
        _meshClient.Mailbox.RetrieveMessagesAsync().Returns(expectedMessages);

        var messages = await _pdsMeshClient.RetrieveMessages();

        await _meshClient.Mailbox.Received().RetrieveMessagesAsync();
        messages.ShouldBe(expectedMessages);
    }


    [Fact]
    public async Task RetrieveMessages_AndMeshClientThrowsException_ExceptionIsReturned()
    {
        _meshClient.Mailbox.RetrieveMessagesAsync().ThrowsAsync(new Exception("Error"));

        var result = await _pdsMeshClient.RetrieveMessages();

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldNotBeNull();
        result.Exception.Message.ShouldBe("Error");
    }

    [Fact]
    public async Task RetrieveMessage_CallsMeshClientRetrieveMessageAsync()
    {
        var messageId = "id";
        var expectedMessage = Substitute.For<Message>();
        expectedMessage.FileContent = "Hello World"u8.ToArray();

        _meshClient.Mailbox.RetrieveMessageAsync(messageId).Returns(expectedMessage);

        var actualMessage = await _pdsMeshClient.RetrieveMessage(messageId);

        Encoding.UTF8.GetString(actualMessage.Value.FileContent).ShouldBe("Hello World");
    }

    [Fact]
    public async Task RetrieveMessageById_AndMeshClientThrowsException_ExceptionIsReturned()
    {
        var messageId = "id";
        _meshClient.Mailbox.RetrieveMessageAsync(messageId).ThrowsAsync(new Exception("Error"));

        var result = await _pdsMeshClient.RetrieveMessage(messageId);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldNotBeNull();
        result.Exception.Message.ShouldBe("Error");
    }

    [Fact]
    public async Task AcknowledgeMessage_CallsMeshClientAcknowledgeMessage()
    {
        var messageId = "id";
        _meshClient.Mailbox.AcknowledgeMessageAsync(messageId).Returns(true);

        var acknowledgeMessage = await _pdsMeshClient.AcknowledgeMessage(messageId);

        acknowledgeMessage.ShouldBe(true);
    }

    [Fact]
    public async Task AcknowledgeMessage_AndExceptionIsThrownFromMeshClient_ExceptionIsReturned()
    {
        var messageId = "id";
        _meshClient.Mailbox.AcknowledgeMessageAsync(messageId).ThrowsAsync(new Exception("Error"));

        var acknowledgeMessage = await _pdsMeshClient.AcknowledgeMessage(messageId);

        acknowledgeMessage.IsFailure.ShouldBeTrue();
        acknowledgeMessage.Exception.ShouldNotBeNull();
        acknowledgeMessage.Exception.Message.ShouldBe("Error");
    }

    [Fact]
    public async Task SendMessage_AndExceptionIsThrownFromMeshClient_ExceptionIsReturned()
    {
        var content = "content";
        _mailboxClient.SendMessageAsync(
            MailboxId,
            WorkflowId,
            content,
            "",
            "",
            Arg.Is<string>(x => x.StartsWith(Globals.PdsMeshMessageFileNamePrefix)),
            "",
            MediaTypeNames.Text.Csv).ThrowsAsync(new Exception("Error"));

        var acknowledgeMessage = await _pdsMeshClient.SendMessage(content);

        acknowledgeMessage.IsSuccess.ShouldBeFalse();
        acknowledgeMessage.Exception.ShouldNotBeNull();
        acknowledgeMessage.Exception.Message.ShouldBe("Error");
    }
}