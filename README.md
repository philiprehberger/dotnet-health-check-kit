# Philiprehberger.HealthCheckKit

[![CI](https://github.com/philiprehberger/dotnet-health-check-kit/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-health-check-kit/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.HealthCheckKit.svg)](https://www.nuget.org/packages/Philiprehberger.HealthCheckKit)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-health-check-kit)](LICENSE)

Composable health checks for ASP.NET Core with built-in URL, disk space, memory, and custom check support.

## Installation

```bash
dotnet add package Philiprehberger.HealthCheckKit
```

## Usage

```csharp
using Philiprehberger.HealthCheckKit;

builder.Services.AddHealthCheckKit(checks => checks
    .AddUrlCheck("https://api.example.com/health")
    .AddDiskSpaceCheck(minimumFreeBytes: 500_000_000)
    .AddMemoryCheck(maximumBytes: 1_073_741_824));

app.MapHealthChecks("/health");
```

### Register Health Checks

```csharp
using Philiprehberger.HealthCheckKit;

builder.Services.AddHealthCheckKit(checks => checks
    .AddUrlCheck("https://api.example.com/health", timeout: TimeSpan.FromSeconds(3))
    .AddDiskSpaceCheck(minimumFreeBytes: 1_000_000_000, drivePath: "C:\\")
    .AddMemoryCheck(maximumBytes: 512_000_000));
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
        // Run your custom health logic
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
| `AddDiskSpaceCheck(long minimumFreeBytes, string? drivePath = null)` | Adds a disk free space check |
| `AddMemoryCheck(long maximumBytes)` | Adds a managed memory usage check |
| `AddCustomCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> check)` | Adds a custom delegate-based check |

### `ServiceCollectionExtensions`

| Method | Description |
|--------|-------------|
| `AddHealthCheckKit(this IServiceCollection services, Action<HealthCheckKitBuilder> configure)` | Registers all configured health checks with the DI container |

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

## License

MIT
