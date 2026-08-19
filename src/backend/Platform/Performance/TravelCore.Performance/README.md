# TravelCore.Performance

Platform capability for **Performance & Scale foundation boundaries** (P28).

This project declares architecture posture only in early P28 tasks:

- profile-before-optimize
- measurement foundation vs Observability separation (T003)
- runtime boundary and module interaction model (T004)
- data access and read optimization boundaries (T005)
- caching boundary and cache policy architecture (T006)
- Redis/cache non-SoR markers
- justified Dapper read posture
- module ownership preservation

It is **not** a generic optimization toolkit and does **not** introduce APM vendors, OpenTelemetry exporters, benchmark harnesses, Redis clients, cache policies, CDN wiring, or load-test harnesses until later explicit P28 task envelopes.
