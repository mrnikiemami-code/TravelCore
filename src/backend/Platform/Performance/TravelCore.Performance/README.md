# TravelCore.Performance

Platform capability for **Performance & Scale foundation boundaries** (P28).

This project declares architecture posture only in early P28 tasks:

- profile-before-optimize
- Redis/cache non-SoR markers
- justified Dapper read posture
- module ownership preservation

It is **not** a generic optimization toolkit and does **not** introduce Redis clients, cache policies, CDN wiring, or load-test harnesses until later explicit P28 task envelopes.
