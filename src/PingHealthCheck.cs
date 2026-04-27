using System.Net.NetworkInformation;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that sends an ICMP echo (ping) request to a host and reports reachability.
/// </summary>
public sealed class PingHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Initializes a new instance of <see cref="PingHealthCheck"/>.
    /// </summary>
    /// <param name="host">The host to ping (hostname or IP address).</param>
    /// <param name="timeout">Optional timeout for the ping request. Defaults to 5 seconds.</param>
    public PingHealthCheck(string host, TimeSpan? timeout = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _timeout = timeout ?? TimeSpan.FromSeconds(5);
    }

    /// <summary>
    /// Sends an ICMP echo request to the host and returns the result.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> when the host replies within the timeout,
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(_host, (int)_timeout.TotalMilliseconds).ConfigureAwait(false);

            if (reply.Status == IPStatus.Success)
            {
                return HealthCheckResult.Healthy($"Ping to {_host} succeeded in {reply.RoundtripTime}ms.");
            }

            return HealthCheckResult.Unhealthy($"Ping to {_host} failed with status {reply.Status}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Ping to {_host} threw an exception.", ex);
        }
    }
}
