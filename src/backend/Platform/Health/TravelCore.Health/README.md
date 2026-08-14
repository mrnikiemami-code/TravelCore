# TravelCore.Health

Platform health-check foundation for the TravelCore host.

- **Liveness** (`/health/live`): process/app viability; does not run readiness-tagged checks.
- **Readiness** (`/health/ready`): required-dependency checks tagged `ready`.

PostgreSQL / infrastructure probes are intentionally deferred (later P01). No third-party health packages.
