using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that verifies managed memory usage does not exceed a configured threshold.
/// </summary>
public sealed class MemoryHealthCheck : IHealthCheck
{
    private readonly long _maximumBytes;

    /// <summary>
    /// Initializes a new instance of <see cref="MemoryHealthCheck"/>.
    /// </summary>
    /// <param name="maximumBytes">The maximum allowed managed memory in bytes.</param>
    public MemoryHealthCheck(long maximumBytes)
    {
        _maximumBytes = maximumBytes;
    }

    /// <summary>
    /// Runs the health check by comparing current managed memory usage against the configured maximum.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if memory usage is within the limit,
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);

        var result = allocated <= _maximumBytes
            ? HealthCheckResult.Healthy($"Memory usage is {allocated} bytes, within {_maximumBytes} byte limit.")
            : HealthCheckResult.Unhealthy($"Memory usage is {allocated} bytes, exceeding {_maximumBytes} byte limit.");

        return Task.FromResult(result);
    }
}
