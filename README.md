# Philiprehberger.HealthCheckKit

[![CI](https://github.com/philiprehberger/dotnet-health-check-kit/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-health-check-kit/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.HealthCheckKit.svg)](https://www.nuget.org/packages/Philiprehberger.HealthCheckKit)
[![GitHub release](https://img.shields.io/github/v/release/philiprehberger/dotnet-health-check-kit)](https://github.com/philiprehberger/dotnet-health-check-kit/releases)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-health-check-kit)](https://github.com/philiprehberger/dotnet-health-check-kit/commits/main)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-health-check-kit)](LICENSE)
[![Bug Reports](https://img.shields.io/github/issues/philiprehberger/dotnet-health-check-kit/bug)](https://github.com/philiprehberger/dotnet-health-check-kit/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![Feature Requests](https://img.shields.io/github/issues/philiprehberger/dotnet-health-check-kit/enhancement)](https://github.com/philiprehberger/dotnet-health-check-kit/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)
[![Sponsor](https://img.shields.io/badge/sponsor-GitHub%20Sponsors-ec6cb9)](https://github.com/sponsors/philiprehberger)

Composable health checks for ASP.NET Core with built-in URL, TCP, DNS, certificate, disk, and memory checks.

## Installation

```bash
dotnet add package Philiprehberger.HealthCheckKit
```

## Usage

```csharp
using Philiprehberger.HealthCheckKit;

builder.Services.AddHealthCheckKit(checks => checks
    .AddUrlCheck("https://api.example.com/health")
    .AddTcpCheck("db-server", 5432)
    .AddDnsCheck("api.example.com")
    .AddCertificateCheck("api.example.com")
    .AddDiskSpaceCheck(minimumFreeBytes: 500_000_000)
    .AddMemoryCheck(maximumBytes: 1_073_741_824));

app.MapHealthChecks("/health");
```

### TCP Port Connectivity

```csharp
using Philiprehberger.HealthCheckKit;

builder.Services.AddHealthCheckKit(checks => checks
    .AddTcpCheck("db-server", 5432)
    .AddTcpCheck("redis", 6379, timeout: TimeSpan.FromSeconds(2)));
```

### DNS Resolution

```csharp
using Philiprehberger.HealthCheckKit;

builder.Services.AddHealthCheckKit(checks => checks
    .AddDnsCheck("api.example.com")
    .AddDnsCheck("storage.example.com"));
```

### SSL Certificate Expiration

```csharp
using Philiprehberger.HealthCheckKit;

builder.Services.AddHealthCheckKit(checks => checks
    .AddCertificateCheck("api.example.com")
    .AddCertificateCheck("payments.example.com", warningDays: 14));
```

### Composite Health Report

```csharp
using Philiprehberger.HealthCheckKit;

var runner = new HealthCheckKitBuilder()
    .AddUrlCheck("https://api.example.com/health")
    .AddTcpCheck("db-server", 5432)
    .AddDnsCheck("api.example.com")
    .BuildRunner();

var report = await runner.RunAllAsync();

Console.WriteLine($"Status: {report.Status}, Total: {report.TotalElapsed.TotalMilliseconds}ms");

foreach (var entry in report.Entries)
{
    Console.WriteLine($"  {entry.Name}: {entry.Result.Status} ({entry.Duration.TotalMilliseconds}ms)");
}
```

### Built-in Checks

```csharp
using Philiprehberger.HealthCheckKit;

// URL check — Healthy on 2xx, Degraded on timeout, Unhealthy otherwise
builder.Services.AddHealthCheckKit(checks => checks
    .AddUrlCheck("https://downstream-service.example.com/ping"));

// Disk space check — Unhealthy when free space drops below threshold
builder.Services.AddHealthCheckKit(checks => checks
    .AddDiskSpaceCheck(minimumFreeBytes: 500_000_000));

// Memory check — Unhealthy when managed memory exceeds limit
builder.Services.AddHealthCheckKit(checks => checks
    .AddMemoryCheck(maximumBytes: 1_073_741_824));
```

### Custom Checks

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Philiprehberger.HealthCheckKit;

builder.Services.AddHealthCheckKit(checks => checks
    .AddCustomCheck("database", async cancellationToken =>
    {
        var isHealthy = await CheckDatabaseConnectionAsync(cancellationToken);
        return isHealthy
            ? HealthCheckResult.Healthy("Database is reachable.")
            : HealthCheckResult.Unhealthy("Database is unreachable.");
    }));
```

## API

### `HealthCheckKitBuilder`

| Method | Description |
|--------|-------------|
| `AddUrlCheck(string url, TimeSpan? timeout = null)` | Adds an HTTP GET check for the given URL |
| `AddTcpCheck(string host, int port, TimeSpan? timeout = null)` | Adds a TCP port connectivity check |
| `AddDnsCheck(string hostname)` | Adds a DNS resolution check |
| `AddCertificateCheck(string host, int warningDays = 30)` | Adds an SSL certificate expiration check |
| `AddDiskSpaceCheck(long minimumFreeBytes, string? drivePath = null)` | Adds a disk free space check |
| `AddMemoryCheck(long maximumBytes)` | Adds a managed memory usage check |
| `AddCustomCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> check)` | Adds a custom delegate-based check |
| `BuildRunner()` | Creates a `HealthCheckRunner` for standalone execution |

### `HealthCheckRunner`

| Method | Description |
|--------|-------------|
| `RunAllAsync(CancellationToken cancellationToken = default)` | Runs all checks and returns a composite `HealthReport` |

### `HealthReport`

| Property | Type | Description |
|----------|------|-------------|
| `Status` | `HealthReportStatus` | Overall status: Healthy, Degraded, or Unhealthy |
| `Entries` | `IReadOnlyList<HealthCheckEntry>` | Individual check results |
| `TotalElapsed` | `TimeSpan` | Total time to run all checks |

### `HealthCheckEntry`

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Name of the health check |
| `Result` | `HealthCheckResult` | The check result with status and description |
| `Duration` | `TimeSpan` | How long the check took to execute |

### `ServiceCollectionExtensions`

| Method | Description |
|--------|-------------|
| `AddHealthCheckKit(this IServiceCollection services, Action<HealthCheckKitBuilder> configure)` | Registers all configured health checks with the DI container |

### `TcpHealthCheck`

| Member | Description |
|--------|-------------|
| `TcpHealthCheck(string host, int port, TimeSpan? timeout = null)` | Creates a TCP connectivity check with optional timeout (default 5s) |
| `CheckHealthAsync(context, cancellationToken)` | Returns Healthy if connection succeeds, Unhealthy otherwise |

### `DnsHealthCheck`

| Member | Description |
|--------|-------------|
| `DnsHealthCheck(string hostname)` | Creates a DNS resolution check for the given hostname |
| `CheckHealthAsync(context, cancellationToken)` | Returns Healthy if resolution returns addresses, Unhealthy otherwise |

### `CertificateHealthCheck`

| Member | Description |
|--------|-------------|
| `CertificateHealthCheck(string host, int warningDays = 30)` | Creates an SSL certificate expiration check |
| `CheckHealthAsync(context, cancellationToken)` | Returns Healthy, Degraded (expiring soon), or Unhealthy (expired/failed) |

### `UrlHealthCheck`

| Member | Description |
|--------|-------------|
| `UrlHealthCheck(string url, TimeSpan? timeout = null)` | Creates a URL health check with optional timeout (default 5s) |
| `CheckHealthAsync(context, cancellationToken)` | Returns Healthy (2xx), Degraded (timeout), or Unhealthy |

### `DiskSpaceHealthCheck`

| Member | Description |
|--------|-------------|
| `DiskSpaceHealthCheck(long minimumFreeBytes, string drivePath = "/")` | Creates a disk space check for the given drive |
| `CheckHealthAsync(context, cancellationToken)` | Returns Healthy if free space meets minimum, Unhealthy otherwise |

### `MemoryHealthCheck`

| Member | Description |
|--------|-------------|
| `MemoryHealthCheck(long maximumBytes)` | Creates a memory check with the given byte limit |
| `CheckHealthAsync(context, cancellationToken)` | Returns Healthy if within limit, Unhealthy otherwise |

## Development

```bash
dotnet build src/Philiprehberger.HealthCheckKit.csproj --configuration Release
```

## Support

If you find this package useful, consider giving it a star on GitHub — it helps motivate continued maintenance and development.

[![LinkedIn](https://img.shields.io/badge/Philip%20Rehberger-LinkedIn-0A66C2?logo=linkedin)](https://www.linkedin.com/in/philiprehberger)
[![More packages](https://img.shields.io/badge/more-open%20source%20packages-blue)](https://philiprehberger.com/open-source-packages)

## License

[MIT](LICENSE)
