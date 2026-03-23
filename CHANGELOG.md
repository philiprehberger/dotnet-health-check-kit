# Changelog

## 0.1.1 (2026-03-23)

- Shorten package description to meet 120-character limit

## 0.1.0 (2026-03-21)

- Initial release
- URL health check with configurable timeout (Healthy/Degraded/Unhealthy)
- Disk space health check with minimum free bytes threshold
- Memory health check with maximum managed memory limit
- Custom delegate-based health checks
- Fluent builder API with `HealthCheckKitBuilder`
- ASP.NET Core DI integration via `AddHealthCheckKit` extension method
