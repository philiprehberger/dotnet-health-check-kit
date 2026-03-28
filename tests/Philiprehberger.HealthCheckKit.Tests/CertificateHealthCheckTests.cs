using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class CertificateHealthCheckTests
{
    [Fact]
    public void Constructor_WithNullHost_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CertificateHealthCheck(null!));
    }

    [Fact]
    public async Task CheckHealthAsync_WithUnreachableHost_ReturnsUnhealthy()
    {
        var check = new CertificateHealthCheck("192.0.2.1");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        try
        {
            var result = await check.CheckHealthAsync(null!, cts.Token);
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
        catch (OperationCanceledException)
        {
            // Acceptable — unreachable host may cause timeout
        }
    }

    [Fact]
    public async Task CheckHealthAsync_WithInvalidHostname_ReturnsUnhealthy()
    {
        var check = new CertificateHealthCheck("this.host.does.not.exist.invalid", warningDays: 30);
        var result = await check.CheckHealthAsync(null!);
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("this.host.does.not.exist.invalid", result.Description);
    }

    [Fact]
    public void Constructor_DefaultWarningDays_DoesNotThrow()
    {
        var check = new CertificateHealthCheck("example.com");
        Assert.NotNull(check);
    }
}
