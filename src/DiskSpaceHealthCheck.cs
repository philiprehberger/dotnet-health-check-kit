using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that verifies sufficient free disk space is available on a given drive.
/// </summary>
public sealed class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly long _minimumFreeBytes;
    private readonly string _drivePath;

    /// <summary>
    /// Initializes a new instance of <see cref="DiskSpaceHealthCheck"/>.
    /// </summary>
    /// <param name="minimumFreeBytes">The minimum number of free bytes required for a healthy result.</param>
    /// <param name="drivePath">The drive path to check. Defaults to <c>"/"</c>.</param>
    public DiskSpaceHealthCheck(long minimumFreeBytes, string drivePath = "/")
    {
        _minimumFreeBytes = minimumFreeBytes;
        _drivePath = drivePath ?? throw new ArgumentNullException(nameof(drivePath));
    }

    /// <summary>
    /// Runs the health check by comparing available free space against the configured minimum.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if free space meets or exceeds the minimum,
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var drive = new DriveInfo(_drivePath);
            var available = drive.AvailableFreeSpace;

            var result = available >= _minimumFreeBytes
                ? HealthCheckResult.Healthy($"Drive {_drivePath} has {available} bytes free.")
                : HealthCheckResult.Unhealthy($"Drive {_drivePath} has {available} bytes free, below minimum {_minimumFreeBytes}.");

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Failed to check drive {_drivePath}.", ex));
        }
    }
}
