using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NEL.MESH.Clients;

namespace Infrastructure.Pds.Mesh.HealthCheck;

public class PdsMeshMailboxHealthCheck([FromKeyedServices("Pds")] IMeshClient meshClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var result = await meshClient.Mailbox.HandshakeAsync();
            return result
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Pds Mailbox is unhealthy");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Pds Mailbox is unhealthy", e);
        }
    }
}