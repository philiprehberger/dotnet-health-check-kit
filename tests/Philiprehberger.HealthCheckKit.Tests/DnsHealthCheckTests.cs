using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class DnsHealthCheckTests
{
    [Fact]
    public void Constructor_WithNullHostname_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DnsHealthCheck(null!));
    }

    [Fact]
    public async Task CheckHealthAsync_WithInvalidHostname_ReturnsUnhealthy()
    {
        var check = new DnsHealthCheck("this.host.does.not.exist.invalid");
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WithLocalhost_ReturnsHealthy()
    {
        var check = new DnsHealthCheck("localhost");
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("localhost", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_UnhealthyResult_ContainsHostnameInDescription()
    {
        var check = new DnsHealthCheck("definitely.not.a.real.host.invalid");
        var result = await check.CheckHealthAsync(null!);
        Assert.Contains("definitely.not.a.real.host.invalid", result.Description);
    }
}
