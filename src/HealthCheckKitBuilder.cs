using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Fluent builder for composing health checks. Use with
/// <see cref="ServiceCollectionExtensions.AddHealthCheckKit"/> to register checks in the DI container.
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
