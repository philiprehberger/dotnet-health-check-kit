using Xunit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit.Tests;

public class HealthCheckKitBuilderTests
{
    [Fact]
    public void AddUrlCheck_AddsCheckToList()
    {
        var builder = new HealthCheckKitBuilder();
        var result = builder.AddUrlCheck("https://example.com");

        Assert.Same(builder, result);
        Assert.Single(builder.Checks);
        Assert.Equal("url:https://example.com", builder.Checks[0].Name);
    }

    [Fact]
    public void AddTcpCheck_AddsCheckToList()
    {
        var builder = new HealthCheckKitBuilder();
        var result = builder.AddTcpCheck("localhost", 8080);

        Assert.Same(builder, result);
        Assert.Single(builder.Checks);
        Assert.Equal("tcp:localhost:8080", builder.Checks[0].Name);
    }

    [Fact]
    public void AddDnsCheck_AddsCheckToList()
    {
        var builder = new HealthCheckKitBuilder();
        var result = builder.AddDnsCheck("example.com");

        Assert.Same(builder, result);
        Assert.Single(builder.Checks);
        Assert.Equal("dns:example.com", builder.Checks[0].Name);
    }

    [Fact]
    public void AddCertificateCheck_AddsCheckToList()
    {
        var builder = new HealthCheckKitBuilder();
        var result = builder.AddCertificateCheck("example.com", warningDays: 14);

        Assert.Same(builder, result);
        Assert.Single(builder.Checks);
        Assert.Equal("cert:example.com", builder.Checks[0].Name);
    }

    [Fact]
    public void FluentChaining_AddsMultipleChecks()
    {
        var builder = new HealthCheckKitBuilder();
        builder
            .AddUrlCheck("https://example.com")
            .AddTcpCheck("localhost", 5432)
            .AddDnsCheck("example.com")
            .AddCertificateCheck("example.com")
            .AddDiskSpaceCheck(1_000_000)
            .AddMemoryCheck(500_000_000)
            .AddCustomCheck("custom", _ => Task.FromResult(HealthCheckResult.Healthy()));

        Assert.Equal(7, builder.Checks.Count);
    }

    [Fact]
    public void BuildRunner_ReturnsRunnerWithAllChecks()
    {
        var builder = new HealthCheckKitBuilder();
        builder.AddCustomCheck("test", _ => Task.FromResult(HealthCheckResult.Healthy()));

        var runner = builder.BuildRunner();
        Assert.NotNull(runner);
    }
}
