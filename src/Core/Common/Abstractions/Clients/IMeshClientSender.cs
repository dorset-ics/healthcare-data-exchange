using Core.Common.Results;
using NEL.MESH.Models.Foundations.Mesh;

namespace Core.Common.Abstractions.Clients;

public interface IMeshClientSender
{
    Task<Result<Message>> SendMessage(string content);
}