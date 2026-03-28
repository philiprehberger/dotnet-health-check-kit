using System.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that verifies DNS resolution succeeds for a given hostname.
/// </summary>
public sealed class DnsHealthCheck : IHealthCheck
{
    private readonly string _hostname;

    /// <summary>
    /// Initializes a new instance of <see cref="DnsHealthCheck"/>.
    /// </summary>
    /// <param name="hostname">The hostname to resolve.</param>
    public DnsHealthCheck(string hostname)
    {
        _hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
    }

    /// <summary>
    /// Runs the health check by resolving the configured hostname via DNS.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if DNS resolution returns at least one address,
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(_hostname, cancellationToken);

            return addresses.Length > 0
                ? HealthCheckResult.Healthy($"DNS resolution for {_hostname} returned {addresses.Length} address(es).")
                : HealthCheckResult.Unhealthy($"DNS resolution for {_hostname} returned no addresses.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"DNS resolution for {_hostname} failed.", ex);
        }
    }
}
