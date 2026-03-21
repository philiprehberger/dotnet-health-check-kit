using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Health check that verifies a URL is reachable by sending an HTTP GET request.
/// Returns Healthy for 2xx responses, Degraded on timeout, and Unhealthy otherwise.
/// </summary>
public sealed class UrlHealthCheck : IHealthCheck
{
    private readonly string _url;
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Initializes a new instance of <see cref="UrlHealthCheck"/>.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <param name="timeout">Optional timeout for the HTTP request. Defaults to 5 seconds.</param>
    public UrlHealthCheck(string url, TimeSpan? timeout = null)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _timeout = timeout ?? TimeSpan.FromSeconds(5);
    }

    /// <summary>
    /// Runs the health check by sending an HTTP GET request to the configured URL.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if the response status code is 2xx,
    /// <see cref="HealthCheckResult.Degraded"/> if the request times out,
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient { Timeout = _timeout };

        try
        {
            var response = await client.GetAsync(_url, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"URL {_url} returned {(int)response.StatusCode}.")
                : HealthCheckResult.Unhealthy($"URL {_url} returned {(int)response.StatusCode}.");
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded($"URL {_url} timed out after {_timeout.TotalSeconds}s.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"URL {_url} is unreachable.", ex);
        }
    }
}
