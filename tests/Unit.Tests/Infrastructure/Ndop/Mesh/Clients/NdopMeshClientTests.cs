using System.Net.Mime;
using System.Text;
using Core;
using Infrastructure.Ndop.Configuration;
using Infrastructure.Ndop.Mesh.Clients;
using Infrastructure.Ndop.Mesh.Configuration;
using Microsoft.Extensions.Logging;
using NEL.MESH.Clients;
using NEL.MESH.Clients.Mailboxes;
using NEL.MESH.Models.Foundations.Mesh;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Infrastructure.Ndop.Mesh.Clients;

public class NdopMeshClientTests
{
    private readonly IMeshClient _meshClient;
    private readonly NdopMeshClient _ndopMeshClient;

    private readonly NdopMeshConfiguration _ndopMeshConfiguration =
        new(SendSchedule: "SendSchedule",
            RetrieveSchedule: "RetrieveSchedule",
            MailboxId: "X26ABC2",
            MailboxPassword: "password",
            Key: "Key",
            WorkflowId: "Workflow",
            RecipientMailboxId: "ABC1234"
            );

    private readonly IMailboxClient _mailboxClient;

    public NdopMeshClientTests()
    {
        var loggerMock = Substitute.For<ILogger<NdopMeshClient>>();
        _meshClient = Substitute.For<IMeshClient>();
        _mailboxClient = Substitute.For<IMailboxClient>();
        _meshClient.Mailbox.Returns(_mailboxClient);
        _ndopMeshClient = new NdopMeshClient(loggerMock, _meshClient,
            new NdopConfiguration(_ndopMeshConfiguration));
    }

    [Fact]
    public async Task SendMessage_WhenSent_ShouldCallMeshClient()
    {
        var content = "message content";

        _mailboxClient.SendMessageAsync(
            _ndopMeshConfiguration.RecipientMailboxId,
            _ndopMeshConfiguration.WorkflowId,
            content,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()).Returns(new Message() { MessageId = "test" });

        var result = await _ndopMeshClient.SendMessage(content);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MessageId.ShouldBe("test");
    }

    [Fact]
    public async Task RetrieveMessages_ShouldCallMeshClient()
    {
        var expectedMessages = new List<string> { "message1", "message2" };
        _meshClient.Mailbox.RetrieveMessagesAsync().Returns(expectedMessages);

        var messages = await _ndopMeshClient.RetrieveMessages();

        await _meshClient.Mailbox.Received().RetrieveMessagesAsync();
        messages.ShouldBe(expectedMessages);
    }


    [Fact]
    public async Task RetrieveMessages_AndMeshClientThrowsException_ExceptionIsReturned()
    {
        _meshClient.Mailbox.RetrieveMessagesAsync().ThrowsAsync(new Exception("Error"));

        var result = await _ndopMeshClient.RetrieveMessages();

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

        var actualMessage = await _ndopMeshClient.RetrieveMessage(messageId);

        Encoding.UTF8.GetString(actualMessage.Value.FileContent).ShouldBe("Hello World");
    }

    [Fact]
    public async Task RetrieveMessageById_AndMeshClientThrowsException_ExceptionIsReturned()
    {
        var messageId = "id";
        _meshClient.Mailbox.RetrieveMessageAsync(messageId).ThrowsAsync(new Exception("Error"));

        var result = await _ndopMeshClient.RetrieveMessage(messageId);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldNotBeNull();
        result.Exception.Message.ShouldBe("Error");
    }

    [Fact]
    public async Task AcknowledgeMessage_CallsMeshClientAcknowledgeMessage()
    {
        var messageId = "id";
        _meshClient.Mailbox.AcknowledgeMessageAsync(messageId).Returns(true);

        var acknowledgeMessage = await _ndopMeshClient.AcknowledgeMessage(messageId);

        acknowledgeMessage.ShouldBe(true);
    }

    [Fact]
    public async Task AcknowledgeMessage_AndExceptionIsThrownFromMeshClient_ExceptionIsReturned()
    {
        var messageId = "id";
        _meshClient.Mailbox.AcknowledgeMessageAsync(messageId).ThrowsAsync(new Exception("Error"));

        var acknowledgeMessage = await _ndopMeshClient.AcknowledgeMessage(messageId);

        acknowledgeMessage.IsFailure.ShouldBeTrue();
        acknowledgeMessage.Exception.ShouldNotBeNull();
        acknowledgeMessage.Exception.Message.ShouldBe("Error");
    }

    [Fact]
    public async Task SendMessage_AndExceptionIsThrownFromMeshClient_ExceptionIsReturned()
    {
        var content = "message content";

        _mailboxClient.SendMessageAsync(
            _ndopMeshConfiguration.RecipientMailboxId,
            _ndopMeshConfiguration.WorkflowId,
            content,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()).ThrowsAsync(new Exception("Error"));

        var acknowledgeMessage = await _ndopMeshClient.SendMessage(content);

        acknowledgeMessage.IsSuccess.ShouldBeFalse();
        acknowledgeMessage.Exception.ShouldNotBeNull();
        acknowledgeMessage.Exception.Message.ShouldBe("Error");
    }
}