using Infrastructure.Ndop.Mesh.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NEL.MESH.Clients;
using NSubstitute.ExceptionExtensions;

namespace Unit.Tests.Infrastructure.Ndop.HealthCheck;

public class NdopMeshMailboxHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ShouldReturnHealthy_WhenHandshakeIsSuccessful()
    {
        var meshClient = Substitute.For<IMeshClient>();
        meshClient.Mailbox.HandshakeAsync().Returns(true);
        var healthCheck = new NdopMeshMailboxHealthCheck(meshClient);
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenHandshakeFails()
    {
        var meshClient = Substitute.For<IMeshClient>();
        meshClient.Mailbox.HandshakeAsync().Returns(false);
        var healthCheck = new NdopMeshMailboxHealthCheck(meshClient);
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenHandshakeThrowsException()
    {
        var meshClient = Substitute.For<IMeshClient>();
        meshClient.Mailbox.HandshakeAsync().ThrowsAsync(new Exception());
        var healthCheck = new NdopMeshMailboxHealthCheck(meshClient);
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }
}