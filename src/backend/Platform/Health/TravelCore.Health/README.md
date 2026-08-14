# TravelCore.Health

Platform health-check foundation for the TravelCore host.

- **Liveness** (`/health/live`): process/app viability; runs **zero** registered health checks (`Predicate = _ => false`).
- **Readiness** (`/health/ready`): only required-dependency checks tagged `ready`. Untagged checks affect neither endpoint.

PostgreSQL / infrastructure probes are intentionally deferred (later P01). No third-party health packages.
