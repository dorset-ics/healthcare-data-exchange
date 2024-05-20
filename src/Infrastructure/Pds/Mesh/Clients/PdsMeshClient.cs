using System.Net.Mime;
using Core.Common.Results;
using Core.Pds.Abstractions;
using Core.Pds.Extensions;
using Infrastructure.Pds.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NEL.MESH.Clients;
using NEL.MESH.Models.Foundations.Mesh;

namespace Infrastructure.Pds.Mesh.Clients;

public class PdsMeshClient(ILogger<PdsMeshClient> logger, [FromKeyedServices("Pds")] IMeshClient meshClient, PdsConfiguration pdsConfiguration)
    : IPdsMeshClient
{
    public async Task<Result<IList<string>>> RetrieveMessages()
    {
        try
        {
            var messages = await meshClient.Mailbox.RetrieveMessagesAsync();
            return messages;
        }
        catch (Exception ex)
        {
            logger.LogError("Error retrieving messages from MESH PDS mailbox: {ErrorMessage}", ex.Message);
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
            logger.LogError("Error retrieving message from MESH PDS mailbox: {ErrorMessage}", ex.Message);
            return ex;
        }
    }

    public async Task<Result<bool>> AcknowledgeMessage(string messageId)
    {
        try
        {
            return await meshClient.Mailbox.AcknowledgeMessageAsync(messageId);
        }
        catch (Exception ex)
        {
            logger.LogError("Error acknowledging message from MESH PDS mailbox: {ErrorMessage}", ex.Message);
            return ex;
        }
    }

    public async Task<Result<Message>> SendMessage(string content)
    {
        try
        {
            var fileName = DateTime.UtcNow.ToPdsMeshMessageFileName();

            var message = await meshClient.Mailbox.SendMessageAsync(
                pdsConfiguration.Mesh.MailboxId,
                pdsConfiguration.Mesh.WorkflowId,
                content,
                mexFileName: fileName,
                contentType: MediaTypeNames.Text.Csv);

            logger.LogInformation("Sent {fileName} to MESH PDS mailbox with message ID {messageId}", fileName, message.MessageId);

            return message;
        }
        catch (Exception ex)
        {
            logger.LogError("Error sending message from MESH PDS mailbox: {ErrorMessage}", ex.Message);
            return ex;
        }
    }
}