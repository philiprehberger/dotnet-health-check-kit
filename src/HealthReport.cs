using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Represents the overall status of a composite health report.
/// </summary>
public enum HealthReportStatus
{
    /// <summary>All checks passed.</summary>
    Healthy,

    /// <summary>At least one check reported degraded, but none are unhealthy.</summary>
    Degraded,

    /// <summary>At least one check is unhealthy.</summary>
    Unhealthy
}

/// <summary>
/// Represents the result of a single health check within a composite report.
/// </summary>
public sealed class HealthCheckEntry
{
    /// <summary>
    /// Gets the name of the health check.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the status of the health check.
    /// </summary>
    public required HealthCheckResult Result { get; init; }

    /// <summary>
    /// Gets how long the health check took to execute.
    /// </summary>
    public required TimeSpan Duration { get; init; }
}

/// <summary>
/// Composite health report containing results from multiple health checks with timing information.
/// </summary>
public sealed class HealthReport
{
    /// <summary>
    /// Gets the overall status derived from individual check results.
    /// </summary>
    public required HealthReportStatus Status { get; init; }

    /// <summary>
    /// Gets the individual health check entries.
    /// </summary>
    public required IReadOnlyList<HealthCheckEntry> Entries { get; init; }

    /// <summary>
    /// Gets the total elapsed time for running all health checks.
    /// </summary>
    public required TimeSpan TotalElapsed { get; init; }
}

/// <summary>
/// Provides the ability to run all configured health checks and produce a composite <see cref="HealthReport"/>.
/// </summary>
public sealed class HealthCheckRunner
{
    private readonly List<(string Name, Func<CancellationToken, Task<HealthCheckResult>> Check)> _checks = [];

    /// <summary>
    /// Initializes a new instance of <see cref="HealthCheckRunner"/>.
    /// </summary>
    internal HealthCheckRunner()
    {
    }

    /// <summary>
    /// Adds a named health check to the runner.
    /// </summary>
    /// <param name="name">The name of the health check.</param>
    /// <param name="check">The function that performs the health check.</param>
    internal void AddCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> check)
    {
        _checks.Add((name, check));
    }

    /// <summary>
    /// Runs all configured health checks and returns a composite <see cref="HealthReport"/>
    /// with overall status, individual results, and timing information.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="HealthReport"/> summarizing all check results.</returns>
    public async Task<HealthReport> RunAllAsync(CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var entries = new List<HealthCheckEntry>(_checks.Count);

        foreach (var (name, check) in _checks)
        {
            var sw = Stopwatch.StartNew();
            HealthCheckResult result;

            try
            {
                result = await check(cancellationToken);
            }
            catch (Exception ex)
            {
                result = HealthCheckResult.Unhealthy($"Check '{name}' threw an exception.", ex);
            }

            sw.Stop();

            entries.Add(new HealthCheckEntry
            {
                Name = name,
                Result = result,
                Duration = sw.Elapsed
            });
        }

        totalStopwatch.Stop();

        var status = DetermineOverallStatus(entries);

        return new HealthReport
        {
            Status = status,
            Entries = entries,
            TotalElapsed = totalStopwatch.Elapsed
        };
    }

    private static HealthReportStatus DetermineOverallStatus(List<HealthCheckEntry> entries)
    {
        var hasUnhealthy = false;
        var hasDegraded = false;

        foreach (var entry in entries)
        {
            switch (entry.Result.Status)
            {
                case HealthStatus.Unhealthy:
                    hasUnhealthy = true;
                    break;
                case HealthStatus.Degraded:
                    hasDegraded = true;
                    break;
            }
        }

        if (hasUnhealthy) return HealthReportStatus.Unhealthy;
        if (hasDegraded) return HealthReportStatus.Degraded;
        return HealthReportStatus.Healthy;
    }
}
