using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class TcpHealthCheckTests
{
    [Fact]
    public void Constructor_WithNullHost_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TcpHealthCheck(null!, 80));
    }

    [Fact]
    public async Task CheckHealthAsync_WithUnreachableHost_ReturnsUnhealthy()
    {
        var check = new TcpHealthCheck("192.0.2.1", 65534, TimeSpan.FromMilliseconds(500));
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WithInvalidHost_ReturnsUnhealthy()
    {
        var check = new TcpHealthCheck("this.host.does.not.exist.invalid", 80, TimeSpan.FromMilliseconds(500));
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WithShortTimeout_ReturnsUnhealthy()
    {
        var check = new TcpHealthCheck("192.0.2.1", 80, TimeSpan.FromMilliseconds(1));
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
