using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class DiskSpaceHealthCheckTests
{
    [Fact]
    public void Constructor_WithNullDrivePath_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DiskSpaceHealthCheck(1000, null!));
    }

    [Fact]
    public async Task CheckHealthAsync_WithLowThreshold_ReturnsHealthy()
    {
        var check = new DiskSpaceHealthCheck(1);
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WithExtremeThreshold_ReturnsUnhealthy()
    {
        var check = new DiskSpaceHealthCheck(long.MaxValue);
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
