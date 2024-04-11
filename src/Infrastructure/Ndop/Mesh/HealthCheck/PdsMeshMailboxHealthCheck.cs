using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NEL.MESH.Clients;

namespace Infrastructure.Ndop.Mesh.HealthCheck;

public class NdopMeshMailboxHealthCheck([FromKeyedServices("Ndop")] IMeshClient meshClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var result = await meshClient.Mailbox.HandshakeAsync();
            return result
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Ndop Mailbox is unhealthy");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Ndop Mailbox is unhealthy", e);
        }
    }
}