# Changelog

## 0.3.0 (2026-04-27)

- Add `AddPingCheck(host, timeout?)` for ICMP reachability via `System.Net.NetworkInformation.Ping`
- Add `AddProcessMemoryCheck(maximumWorkingSetBytes)` distinct from the managed-heap `AddMemoryCheck`
- Add `tags` parameter to every `AddXxxCheck` method; tags surface on the report's `HealthCheckEntry.Tags`
- Add `HealthCheckRunner.PerCheckTimeout` (default 30s) so a hung individual check no longer stalls the report
- Pin CI workflow to `actions/checkout@v5` per the standardized template

## 0.2.1 (2026-03-31)

- Standardize README to 3-badge format with emoji Support section
- Update CI actions to v5 for Node.js 24 compatibility

## 0.2.0 (2026-03-28)

- Add TCP port connectivity check via `AddTcpCheck` with configurable timeout
- Add DNS resolution check via `AddDnsCheck`
- Add SSL certificate expiration check via `AddCertificateCheck` with configurable warning window
- Add composite `HealthReport` with overall status, individual results, and timing
- Add `HealthCheckRunner` with `RunAllAsync` for standalone health check execution
- Add `BuildRunner` method to `HealthCheckKitBuilder`
- Add unit tests for all health check types
- Add GitHub issue templates, dependabot, and pull request template
- Update CI workflow to include test step

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
