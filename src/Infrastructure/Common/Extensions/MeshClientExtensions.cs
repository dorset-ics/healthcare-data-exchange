using System.Net.Mime;
using NEL.MESH.Clients.Mailboxes;
using NEL.MESH.Models.Foundations.Mesh;

namespace Infrastructure.Common.Extensions
{
    public static class MeshClientExtensions
    {
        public static async ValueTask<Message> SendMessageAndControlFileAsync(this IMailboxClient mailbox, string mexFrom, string mexTo, string mexWorkflowId, string content, string mexFileName, string contentType, string controlId)
        {
            var message = await mailbox.SendMessageAsync(
                mexTo,
                mexWorkflowId,
                content,
                mexFileName: mexFileName,
                contentType: contentType);

            var controlFileContent = "<DTSControl>"
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

            var controlFileName = $"{Path.GetFileNameWithoutExtension(mexFileName)}.ctl";

            await mailbox.SendMessageAsync(
                mexTo,
                mexWorkflowId,
                controlFileContent,
                mexFileName: controlFileName,
                contentType: MediaTypeNames.Text.Xml);

            return message;
        }
    }
}
