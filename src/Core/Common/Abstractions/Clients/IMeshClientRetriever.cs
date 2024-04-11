using Core.Common.Results;
using NEL.MESH.Models.Foundations.Mesh;

namespace Core.Common.Abstractions.Clients;

public interface IMeshClientRetriever
{
    Task<Result<IList<string>>> RetrieveMessages();
    Task<Result<Message>> RetrieveMessage(string messageId);
    Task<Result<bool>> AcknowledgeMessage(string messageId);
}