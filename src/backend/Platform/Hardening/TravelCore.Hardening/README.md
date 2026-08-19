# TravelCore.Hardening

Platform capability for **Production Hardening foundation boundaries** (P29).

This project declares architecture posture only in early P29 tasks:

- security-from-day-one
- secrets != business data
- health != rich diagnostics
- audit metadata != audit-event product
- security/authorization review boundary (T003) · **P29-R1 RESOLVED**
- rate limiting / abuse protection boundary (T004)
- audit / compliance event boundary (T005)
- content sanitization / file security boundary (T006)
- backup/restore / DR / DB recovery boundary (T007)
- operational platform hardening + production verification + runbooks (T008)
- module ownership preservation

It is **not** a generic security toolkit and does **not** introduce rate limiters, audit-event stores, secret managers, backup automation, WAF vendors, SAST/DAST products, CI/CD YAML, or penetration-test harnesses until later explicit P29 task envelopes.
