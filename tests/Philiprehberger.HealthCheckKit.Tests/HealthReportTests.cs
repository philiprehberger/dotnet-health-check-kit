using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class HealthReportTests
{
    [Fact]
    public async Task RunAllAsync_WithAllHealthyChecks_ReturnsHealthyStatus()
    {
        var builder = new HealthCheckKitBuilder();
        builder.AddCustomCheck("test1", _ => Task.FromResult(HealthCheckResult.Healthy("OK")));
        builder.AddCustomCheck("test2", _ => Task.FromResult(HealthCheckResult.Healthy("OK")));

        var runner = builder.BuildRunner();
        var report = await runner.RunAllAsync();

        Assert.Equal(HealthReportStatus.Healthy, report.Status);
        Assert.Equal(2, report.Entries.Count);
        Assert.True(report.TotalElapsed > TimeSpan.Zero);
    }

    [Fact]
    public async Task RunAllAsync_WithDegradedCheck_ReturnsDegradedStatus()
    {
        var builder = new HealthCheckKitBuilder();
        builder.AddCustomCheck("healthy", _ => Task.FromResult(HealthCheckResult.Healthy("OK")));
        builder.AddCustomCheck("degraded", _ => Task.FromResult(HealthCheckResult.Degraded("Slow")));

        var runner = builder.BuildRunner();
        var report = await runner.RunAllAsync();

        Assert.Equal(HealthReportStatus.Degraded, report.Status);
    }

    [Fact]
    public async Task RunAllAsync_WithUnhealthyCheck_ReturnsUnhealthyStatus()
    {
        var builder = new HealthCheckKitBuilder();
        builder.AddCustomCheck("healthy", _ => Task.FromResult(HealthCheckResult.Healthy("OK")));
        builder.AddCustomCheck("degraded", _ => Task.FromResult(HealthCheckResult.Degraded("Slow")));
        builder.AddCustomCheck("unhealthy", _ => Task.FromResult(HealthCheckResult.Unhealthy("Down")));

        var runner = builder.BuildRunner();
        var report = await runner.RunAllAsync();

        Assert.Equal(HealthReportStatus.Unhealthy, report.Status);
    }

    [Fact]
    public async Task RunAllAsync_WithNoChecks_ReturnsHealthyStatus()
    {
        var builder = new HealthCheckKitBuilder();
        var runner = builder.BuildRunner();
        var report = await runner.RunAllAsync();

        Assert.Equal(HealthReportStatus.Healthy, report.Status);
        Assert.Empty(report.Entries);
    }

    [Fact]
    public async Task RunAllAsync_EntryContainsNameAndDuration()
    {
        var builder = new HealthCheckKitBuilder();
        builder.AddCustomCheck("my-check", _ => Task.FromResult(HealthCheckResult.Healthy("OK")));

        var runner = builder.BuildRunner();
        var report = await runner.RunAllAsync();

        var entry = Assert.Single(report.Entries);
        Assert.Equal("my-check", entry.Name);
        Assert.True(entry.Duration >= TimeSpan.Zero);
        Assert.Equal(HealthStatus.Healthy, entry.Result.Status);
    }

    [Fact]
    public async Task RunAllAsync_WithThrowingCheck_ReturnsUnhealthy()
    {
        var builder = new HealthCheckKitBuilder();
        builder.AddCustomCheck("throws", _ => throw new InvalidOperationException("boom"));

        var runner = builder.BuildRunner();
        var report = await runner.RunAllAsync();

        Assert.Equal(HealthReportStatus.Unhealthy, report.Status);
        var entry = Assert.Single(report.Entries);
        Assert.Equal(HealthStatus.Unhealthy, entry.Result.Status);
    }
}
