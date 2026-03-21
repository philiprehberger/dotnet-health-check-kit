using Microsoft.Extensions.DependencyInjection;

namespace Philiprehberger.HealthCheckKit;

/// <summary>
/// Extension methods for registering HealthCheckKit checks with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HealthCheckKit health checks to the service collection using the fluent builder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action that configures the health checks via <see cref="HealthCheckKitBuilder"/>.</param>
    /// <returns>The service collection for further chaining.</returns>
    public static IServiceCollection AddHealthCheckKit(
        this IServiceCollection services,
        Action<HealthCheckKitBuilder> configure)
    {
        var builder = new HealthCheckKitBuilder();
        configure(builder);

        var healthChecks = services.AddHealthChecks();

        foreach (var (name, factory) in builder.Checks)
        {
            healthChecks.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
                name,
                factory,
                failureStatus: null,
                tags: null));
        }

        return services;
    }
}
