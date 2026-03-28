using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class MemoryHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WithHighLimit_ReturnsHealthy()
    {
        var check = new MemoryHealthCheck(long.MaxValue);
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WithZeroLimit_ReturnsUnhealthy()
    {
        var check = new MemoryHealthCheck(0);
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_DescriptionContainsByteInfo()
    {
        var check = new MemoryHealthCheck(long.MaxValue);
        var result = await check.CheckHealthAsync(null!);
        Assert.Contains("bytes", result.Description);
    }
}
