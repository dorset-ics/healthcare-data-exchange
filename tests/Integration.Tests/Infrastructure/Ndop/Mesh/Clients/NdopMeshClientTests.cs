using Core.Ndop.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NEL.MESH.Clients;
using NEL.MESH.Models.Configurations;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests.Infrastructure.Ndop.Mesh.Clients
{
    public class NdopMeshClientTests : IDisposable
    {
        private readonly ApiWebApplicationFactory _webApplicationFactory;

        private readonly INdopMeshClient _ndopMeshClient;

        public NdopMeshClientTests()
        {
            _webApplicationFactory = new ApiWebApplicationFactory();
            _ndopMeshClient = _webApplicationFactory.Services.GetService<INdopMeshClient>()
                             ?? throw new Exception("Failed to resolve IMeshClientSender from the service provider.");
        }

        public void Dispose()
        {
            _webApplicationFactory.Dispose();
            GC.SuppressFinalize(this);
        }


        [Fact]
        public async Task SendMessage_WithMessage_ShouldSendMessageToMailbox()
        {
            var receiver = GetMeshClient();
            var content = "content";
            var message = await _ndopMeshClient.SendMessage(content);

            var retrieveMessageAsync = await receiver.Mailbox.RetrieveMessageAsync(message.Value.MessageId);
            var messageContent = System.Text.Encoding.Default.GetString(retrieveMessageAsync.FileContent);
            messageContent.ShouldNotBeNull();
            messageContent.ShouldBe(content);

            await receiver.Mailbox.AcknowledgeMessageAsync(message.Value.MessageId);
        }


        private static MeshClient GetMeshClient()
        {
            var meshConfigurations = new MeshConfiguration
            {
                MailboxId = "X26ABC2",
                Password = "password",
                Key = "TestKey",
                Url = "http://localhost:8700",
                MaxChunkSizeInMegabytes = 100
            };

            return new MeshClient(meshConfigurations);
        }
    }
}