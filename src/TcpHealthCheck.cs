using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that verifies TCP connectivity to a remote host and port.
/// </summary>
public sealed class TcpHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly int _port;
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Initializes a new instance of <see cref="TcpHealthCheck"/>.
    /// </summary>
    /// <param name="host">The hostname or IP address to connect to.</param>
    /// <param name="port">The TCP port to connect to.</param>
    /// <param name="timeout">Optional timeout for the connection attempt. Defaults to 5 seconds.</param>
    public TcpHealthCheck(string host, int port, TimeSpan? timeout = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port;
        _timeout = timeout ?? TimeSpan.FromSeconds(5);
    }

    /// <summary>
    /// Runs the health check by attempting a TCP connection to the configured host and port.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if the connection succeeds,
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_timeout);

            await client.ConnectAsync(_host, _port, cts.Token);

            return HealthCheckResult.Healthy($"TCP connection to {_host}:{_port} succeeded.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy($"TCP connection to {_host}:{_port} timed out after {_timeout.TotalSeconds}s.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"TCP connection to {_host}:{_port} failed.", ex);
        }
    }
}
