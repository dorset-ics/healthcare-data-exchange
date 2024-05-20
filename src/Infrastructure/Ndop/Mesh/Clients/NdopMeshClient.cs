using System.Net.Mime;
using Core.Common.Results;
using Core.Ndop.Abstractions;
using Core.Ndop.Extensions;
using Infrastructure.Common.Extensions;
using Infrastructure.Ndop.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NEL.MESH.Clients;
using NEL.MESH.Models.Foundations.Mesh;

namespace Infrastructure.Ndop.Mesh.Clients;

public class NdopMeshClient(ILogger<NdopMeshClient> logger, [FromKeyedServices("Ndop")] IMeshClient meshClient, NdopConfiguration ndopConfiguration)
    : INdopMeshClient
{
    public async Task<Result<bool>> AcknowledgeMessage(string messageId)
    {
        try
        {
            return await meshClient.Mailbox.AcknowledgeMessageAsync(messageId);
        }
        catch (Exception ex)
        {
            logger.LogError("Error acknowledging message from MESH NDOP mailbox: {ErrorMessage}", ex.Message);
            return ex;
        }
    }

    public async Task<Result<Message>> RetrieveMessage(string messageId)
    {
        try
        {
            var message = await meshClient.Mailbox.RetrieveMessageAsync(messageId);
            return message;
        }
        catch (Exception ex)
        {
            logger.LogError("Error retrieving message from MESH NDOP mailbox: {ErrorMessage}", ex.Message);
            return ex;
        }
    }

    public async Task<Result<IList<string>>> RetrieveMessages()
    {
        try
        {
            var messages = await meshClient.Mailbox.RetrieveMessagesAsync();
            return messages;
        }
        catch (Exception ex)
        {
            logger.LogError("Error retrieving messages from MESH NDOP mailbox: {ErrorMessage}", ex.Message);
            return ex;
        }
    }

    public async Task<Result<Message>> SendMessage(string content)
    {
        try
        {
            var fileName = DateTime.UtcNow.ToNdopMeshMessageFileName();

            var controlId = $"{ndopConfiguration.Mesh.MailboxId}_{Guid.NewGuid()}";

            var message = await meshClient.Mailbox.SendMessageAndControlFileAsync(
                ndopConfiguration.Mesh.MailboxId,
                ndopConfiguration.Mesh.RecipientMailboxId,
                ndopConfiguration.Mesh.WorkflowId,
                content,
                fileName,
                MediaTypeNames.Text.Csv,
                controlId);

            logger.LogInformation("Sent {fileName} to MESH NDOP mailbox with message ID {messageId}", fileName, message.MessageId);
            message.TrackingInfo = new TrackingInfo { LocalId = controlId, MessageId = message.MessageId };
            return message;
        }
        catch (Exception ex)
        {
            logger.LogError("Error sending message to MESH NDOP mailbox: {ErrorMessage}", ex.Message);
            return ex;
        }
    }
}