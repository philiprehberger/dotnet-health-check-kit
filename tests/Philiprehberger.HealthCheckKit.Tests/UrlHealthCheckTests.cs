using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class UrlHealthCheckTests
{
    [Fact]
    public void Constructor_WithNullUrl_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UrlHealthCheck(null!));
    }

    [Fact]
    public async Task CheckHealthAsync_WithInvalidUrl_ReturnsUnhealthy()
    {
        var check = new UrlHealthCheck("http://192.0.2.1:1", TimeSpan.FromMilliseconds(500));
        var result = await check.CheckHealthAsync(null!);
        Assert.NotEqual(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public void Constructor_DefaultTimeout_DoesNotThrow()
    {
        var check = new UrlHealthCheck("https://example.com");
        Assert.NotNull(check);
    }
}
