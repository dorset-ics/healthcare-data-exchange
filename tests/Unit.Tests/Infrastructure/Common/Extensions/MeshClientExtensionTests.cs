using System.Net.Mime;
using Infrastructure.Common.Extensions;
using NEL.MESH.Clients.Mailboxes;
using NSubstitute.ReceivedExtensions;

namespace Unit.Tests.Infrastructure.Common.Extensions;

public class MeshClientExtensionTests
{
    private readonly IMailboxClient _mailboxClient;

    public MeshClientExtensionTests()
    {
        _mailboxClient = Substitute.For<IMailboxClient>();
    }

    [Fact]
    public async Task SendMessageAndControlFile_SendsTwoMessagesToMesh()
    {
        var mexFrom = "mexFrom";
        var mexTo = "mexTo";
        var mexWorkflowId = "mexWorkflowId";
        var content = "content";
        var mexFileName = "mexFileName.dat";
        var contentType = "contentType";
        var controlId = "controlId";

        await _mailboxClient.SendMessageAndControlFileAsync(
            mexFrom,
            mexTo,
            mexWorkflowId,
            content,
            mexFileName,
            contentType,
            controlId);

        await _mailboxClient.Received(2).SendMessageAsync(
            mexTo,
            mexWorkflowId,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task SendMessageAndControlFile_SendsDataFileMessageToMesh()
    {
        var mexFrom = "mexFrom";
        var mexTo = "mexTo";
        var mexWorkflowId = "mexWorkflowId";
        var content = "content";
        var mexFileName = "mexFileName.dat";
        var contentType = "contentType";
        var controlId = "controlId";

        await _mailboxClient.SendMessageAndControlFileAsync(
            mexFrom,
            mexTo,
            mexWorkflowId,
            content,
            mexFileName,
            contentType,
            controlId);

        await _mailboxClient.Received(1).SendMessageAsync(
            mexTo,
            mexWorkflowId,
            content,
            Arg.Any<string>(),
            Arg.Any<string>(),
            mexFileName,
            Arg.Any<string>(),
            contentType,
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task SendMessageAndControlFile_SendsControlFileMessageToMesh()
    {
        var mexFrom = "mexFrom";
        var mexTo = "mexTo";
        var mexWorkflowId = "mexWorkflowId";
        var content = "content";
        var mexFileName = "mexFileName.dat";
        var contentType = "contentType";
        var controlId = "controlId";

        await _mailboxClient.SendMessageAndControlFileAsync(
            mexFrom,
            mexTo,
            mexWorkflowId,
            content,
            mexFileName,
            contentType,
            controlId);

        var expectedControlFileContent = "<DTSControl>"
                               + "<Version>1.0</Version>"
                               + "<AddressType>DTS</AddressType>"
                               + "<MessageType>Data</MessageType>"
                               + $"<WorkflowId>{mexWorkflowId}</WorkflowId>"
                               + $"<To_DTS>{mexTo}</To_DTS>"
                               + $"<From_DTS>{mexFrom}</From_DTS>"
                               + $"<Subject>{controlId}</Subject>"
                               + $"<LocalId>{controlId}</LocalId>"
                               + "<Compress>Y</Compress>"
                               + "<AllowChunking>Y</AllowChunking>"
                               + "<Encrypted>N</Encrypted>"
                               + "</DTSControl>";

        await _mailboxClient.Received(1).SendMessageAsync(
            mexTo,
            mexWorkflowId,
            expectedControlFileContent,
            Arg.Any<string>(),
            Arg.Any<string>(),
            mexFileName.Replace(".dat", ".ctl"),
            Arg.Any<string>(),
            MediaTypeNames.Text.Xml,
            Arg.Any<string>(),
            Arg.Any<string>());
    }
}