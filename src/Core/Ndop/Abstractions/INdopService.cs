namespace Core.Ndop.Abstractions;

public interface INdopService
{
    Task RetrieveMeshMessages(CancellationToken cancellationToken);
    Task SendMeshMessages(CancellationToken cancellationToken);
}