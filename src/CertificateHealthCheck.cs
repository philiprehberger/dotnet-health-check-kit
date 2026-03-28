using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that verifies an SSL/TLS certificate is valid and not expiring soon.
/// </summary>
public sealed class CertificateHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly int _warningDays;

    /// <summary>
    /// Initializes a new instance of <see cref="CertificateHealthCheck"/>.
    /// </summary>
    /// <param name="host">The hostname to connect to over TLS.</param>
    /// <param name="warningDays">Number of days before expiration to report as degraded. Defaults to 30.</param>
    public CertificateHealthCheck(string host, int warningDays = 30)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _warningDays = warningDays;
    }

    /// <summary>
    /// Runs the health check by connecting via TLS and inspecting the server certificate expiration.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if the certificate is valid and not expiring within the warning window,
    /// <see cref="HealthCheckResult.Degraded"/> if the certificate expires within the warning window,
    /// <see cref="HealthCheckResult.Unhealthy"/> if the certificate has already expired or the connection fails.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(_host, 443, cancellationToken);

            X509Certificate2? serverCert = null;

            using var ssl = new SslStream(tcp.GetStream(), false, (_, cert, _, _) =>
            {
                if (cert is not null)
                {
                    serverCert = new X509Certificate2(cert);
                }
                return true;
            });

            await ssl.AuthenticateAsClientAsync(_host);

            if (serverCert is null)
            {
                return HealthCheckResult.Unhealthy($"No certificate received from {_host}.");
            }

            var expiration = serverCert.NotAfter;
            var daysUntilExpiry = (expiration - DateTime.UtcNow).TotalDays;

            if (daysUntilExpiry <= 0)
            {
                return HealthCheckResult.Unhealthy($"Certificate for {_host} expired on {expiration:yyyy-MM-dd}.");
            }

            if (daysUntilExpiry <= _warningDays)
            {
                return HealthCheckResult.Degraded($"Certificate for {_host} expires in {(int)daysUntilExpiry} day(s) on {expiration:yyyy-MM-dd}.");
            }

            return HealthCheckResult.Healthy($"Certificate for {_host} is valid until {expiration:yyyy-MM-dd} ({(int)daysUntilExpiry} days remaining).");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy($"Certificate check for {_host} failed.", ex);
        }
    }
}
