using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that verifies the current process's working set (resident memory) does not exceed
/// a configured threshold. Distinct from <see cref="MemoryHealthCheck"/> which inspects only the
/// managed heap via <see cref="GC.GetTotalMemory(bool)"/>.
/// </summary>
public sealed class ProcessMemoryHealthCheck : IHealthCheck
{
    private readonly long _maximumWorkingSetBytes;

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessMemoryHealthCheck"/>.
    /// </summary>
    /// <param name="maximumWorkingSetBytes">Maximum allowed working set size in bytes.</param>
    public ProcessMemoryHealthCheck(long maximumWorkingSetBytes)
    {
        _maximumWorkingSetBytes = maximumWorkingSetBytes;
    }

    /// <summary>
    /// Compares the current process working set against the configured maximum.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if usage is within the limit,
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var process = Process.GetCurrentProcess();
        var workingSet = process.WorkingSet64;

        var result = workingSet <= _maximumWorkingSetBytes
            ? HealthCheckResult.Healthy($"Process working set is {workingSet} bytes, within {_maximumWorkingSetBytes} byte limit.")
            : HealthCheckResult.Unhealthy($"Process working set is {workingSet} bytes, exceeding {_maximumWorkingSetBytes} byte limit.");

        return Task.FromResult(result);
    }
}
