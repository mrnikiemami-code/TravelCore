# TC-P32-T006 — Destination Media Ownership Finding

| Field | Value |
|-------|--------|
| Task-ID | `TC-P32-T006` |
| Status (Cursor) | **BLOCKED** |
| Date | 2026-08-21 |

## Architecture finding

**Does Destination already have a supported Media ownership path?**  
**No.**

Evidence:

1. Destination Contracts / Application Service: no Cover / Gallery / MediaLink / SetCover APIs.
2. Destination Domain / Infrastructure: no Media relations (grep empty for MediaAsset / SetCover / Gallery).
3. Project SoT: **P06-R5 RESOLVED — CONTRACT-ONLY** — `MediaAssetReference` + ArchitectureTests only; **no Destination schema `MediaAssetId`**.
4. TC-P32-T002 correctly skipped Destination pack attach for this reason.

## Correct ownership model (unchanged)

```text
Media owns technical asset truth
Domain owns semantic relationship
```

Place and Tour already implement domain semantic links (Cover/Gallery). Destination does **not**.

## What was NOT done (correct fail-closed)

- No invented Destination Media schema
- No hardcoded frontend Destination cover URLs
- No Media bypass
- No DEMOFEED-only fake destination media path

## Required follow-up (Architect-authorized only)

A Level 1–3 decision / ADR-capable task to introduce Destination↔Media semantic ownership (Cover 0..1, optional Gallery), then DEMOFEED enrich for destination pack files.

Until then: Home destination cards remain gradient-led despite pack files on disk.

## Acceptance risks

P32 GATE may remain **WITH KNOWN LIMITATIONS** until Destination Media ownership is authorized and implemented.
