using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Fluent builder for composing health checks. Use with
/// <see cref="ServiceCollectionExtensions.AddHealthCheckKit"/> to register checks in the DI container,
/// or call <see cref="BuildRunner"/> to create a <see cref="HealthCheckRunner"/> for standalone execution.
/// </summary>
public sealed class HealthCheckKitBuilder
{
    internal List<(string Name, Func<IServiceProvider, IHealthCheck> Factory)> Checks { get; } = [];

    /// <summary>
    /// Adds a URL health check that sends an HTTP GET request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <param name="timeout">Optional timeout for the HTTP request.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public HealthCheckKitBuilder AddUrlCheck(string url, TimeSpan? timeout = null)
    {
        Checks.Add(($"url:{url}", _ => new UrlHealthCheck(url, timeout)));
        return this;
    }

    /// <summary>
    /// Adds a disk space health check that verifies sufficient free space on a drive.
    /// </summary>
    /// <param name="minimumFreeBytes">The minimum number of free bytes required.</param>
    /// <param name="drivePath">The drive path to check. Defaults to the root drive.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public HealthCheckKitBuilder AddDiskSpaceCheck(long minimumFreeBytes, string? drivePath = null)
    {
        var path = drivePath ?? "/";
        Checks.Add(($"disk:{path}", _ => new DiskSpaceHealthCheck(minimumFreeBytes, path)));
        return this;
    }

    /// <summary>
    /// Adds a memory health check that verifies managed memory usage is within limits.
    /// </summary>
    /// <param name="maximumBytes">The maximum allowed managed memory in bytes.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public HealthCheckKitBuilder AddMemoryCheck(long maximumBytes)
    {
        Checks.Add(("memory", _ => new MemoryHealthCheck(maximumBytes)));
        return this;
    }

    /// <summary>
    /// Adds a TCP port connectivity check that verifies a remote host is reachable on the specified port.
    /// </summary>
    /// <param name="host">The hostname or IP address to connect to.</param>
    /// <param name="port">The TCP port to connect to.</param>
    /// <param name="timeout">Optional timeout for the connection attempt. Defaults to 5 seconds.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public HealthCheckKitBuilder AddTcpCheck(string host, int port, TimeSpan? timeout = null)
    {
        Checks.Add(($"tcp:{host}:{port}", _ => new TcpHealthCheck(host, port, timeout)));
        return this;
    }

    /// <summary>
    /// Adds a DNS resolution check that verifies a hostname can be resolved to at least one IP address.
    /// </summary>
    /// <param name="hostname">The hostname to resolve.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public HealthCheckKitBuilder AddDnsCheck(string hostname)
    {
        Checks.Add(($"dns:{hostname}", _ => new DnsHealthCheck(hostname)));
        return this;
    }

    /// <summary>
    /// Adds an SSL certificate expiration check that verifies the server certificate is valid
    /// and not expiring within the specified warning window.
    /// </summary>
    /// <param name="host">The hostname to connect to over TLS.</param>
    /// <param name="warningDays">Number of days before expiration to report as degraded. Defaults to 30.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public HealthCheckKitBuilder AddCertificateCheck(string host, int warningDays = 30)
    {
        Checks.Add(($"cert:{host}", _ => new CertificateHealthCheck(host, warningDays)));
        return this;
    }

    /// <summary>
    /// Adds a custom health check with a user-defined check function.
    /// </summary>
    /// <param name="name">A unique name for the health check.</param>
    /// <param name="check">A function that performs the health check and returns a result.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public HealthCheckKitBuilder AddCustomCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> check)
    {
        Checks.Add((name, _ => new DelegateHealthCheck(check)));
        return this;
    }

    /// <summary>
    /// Builds a <see cref="HealthCheckRunner"/> that can execute all configured checks
    /// and produce a composite <see cref="HealthReport"/>.
    /// </summary>
    /// <returns>A runner that executes all configured health checks.</returns>
    public HealthCheckRunner BuildRunner()
    {
        var runner = new HealthCheckRunner();

        foreach (var (name, factory) in Checks)
        {
            var check = factory(null!);
            runner.AddCheck(name, ct => check.CheckHealthAsync(null!, ct));
        }

        return runner;
    }
}

/// <summary>
/// Internal health check that wraps a delegate function.
/// </summary>
internal sealed class DelegateHealthCheck : IHealthCheck
{
    private readonly Func<CancellationToken, Task<HealthCheckResult>> _check;

    /// <summary>
    /// Initializes a new instance of <see cref="DelegateHealthCheck"/>.
    /// </summary>
    /// <param name="check">The delegate to execute.</param>
    public DelegateHealthCheck(Func<CancellationToken, Task<HealthCheckResult>> check)
    {
        _check = check ?? throw new ArgumentNullException(nameof(check));
    }

    /// <summary>
    /// Runs the delegated health check.
    /// </summary>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return _check(cancellationToken);
    }
}
