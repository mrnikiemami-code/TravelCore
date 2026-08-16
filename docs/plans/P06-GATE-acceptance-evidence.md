# TC-P06-GATE — P06 Acceptance Evidence

**Task:** TC-P06-GATE — P06 Acceptance Gate  
**Envelope expected baseline:** `acfed76` (`docs: record TC-P06-T012 commit SHA 8981312`; after T012 ACCEPTED)  
**Observed gate execution HEAD (preflight):** `acfed76`  
**Baseline drift:** NONE — `HEAD == origin/main == acfed76`; CLEAN tree  
**Date:** 2026-08-16  
**Scope:** Gate / acceptance only — no new P06 product features; **P07 not started**.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER `TRAVELCORE_PHASE_CONFIRM: P06` | YES (prior) |
| USER `TRAVELCORE_TASK_CONFIRM: TC-P06-GATE` | YES (ChatGPT / PIPELINE continuation) |
| USER `TRAVELCORE_MODE: PIPELINE` | YES |
| Architect Auto-Execute GATE envelope | YES (DOM truncated; authoritative scope from plan § TC-P06-GATE + §10) |
| T001–T012 accepted | YES (product commits through T012 `8981312`; hygiene through `acfed76`) |
| Working tree at gate start | CLEAN |
| HEAD == origin/main | `acfed76` |

## 2. Plan §10 acceptance checklist (GATE)

| # | Criterion | Evidence |
|---|-----------|----------|
| 1 | Media module + separate schema; Media ↛ business modules; no cross-schema writes | ArchitectureTests (25) · schema `media` · `MediaDbContext` · peer-module guards |
| 2 | MediaAsset upload → storage → metadata Ready (Access-backed) | T004 · `MediaUploadAccessAuthorizationTests` · host battery |
| 3 | Variants + dimensions persisted (constitution direction) | T005 · R3 1600/960/320 · unit/host |
| 4 | Focal point persisted and readable | T006 + T006-R1 · host focal Access tests |
| 5 | Alt/caption localization without `AltFa`-style columns; ADR 0008 | T007 · translation locale rows · host |
| 6 | Bytes in S3-compatible storage; not default domain-table blobs | T003 · `IMediaObjectStorage` · local FS + in-memory adapters |
| 7 | Optimization contract; WebP/AVIF deferred with evidence | T008 · **P06-R1 = DEFER** · [`P06-T008-optimization-contract-and-r1-defer.md`](P06-T008-optimization-contract-and-r1-defer.md) |
| 8 | Frontend presentation extends P02 with approved remote policy | T009 · app-proxy · `p02-quality-checks.mjs` media allowlist |
| 9 | Relationship semantics remain with consumers (no Place/Tour gallery engines) | T010 · **R5 CONTRACT-ONLY** · no `Modules/Place` · [`P06-T010-consumer-reference-contract-proof.md`](P06-T010-consumer-reference-contract-proof.md) |
| 10 | Admin Media baseline job-based + Access-backed | T011 · `/[locale]/admin/media` · list/upload/inspect Access host tests |
| 11 | P07+ engines absent | No Place module · ROADMAP P07 NOT_STARTED · no CMS/Tour media engines |
| 12 | Evidence pack complete; tests green; clean tree after gate hygiene | [`P06-T012-hardening-and-evidence-pack.md`](P06-T012-hardening-and-evidence-pack.md) + gate re-run below |

## 3. Validation battery (gate re-run)

| Suite | Result |
|-------|--------|
| `dotnet build TravelCore.sln` | PASS (0 errors; xUnit1051 warnings in Media.UnitTests only) |
| Media.UnitTests | 65 PASS |
| Access.UnitTests | 5 PASS |
| ArchitectureTests | 25 PASS |
| Persistence.IntegrationTests | 18 PASS |
| Host.IntegrationTests | 34 PASS |
| Frontend `npm run quality` | PASS |
| `git diff --check` | PASS |

## 4. Locked decisions preserved

- **R1 RESOLVED — DEFER:** WebP/AVIF conversion out of P06; same-format variants only
- **R2 RESOLVED:** Media-owned `IMediaObjectStorage` (not Platform-wide)
- **R3 RESOLVED:** Synchronous variants; large=1600 / medium=960 / thumbnail=320; fit-within; GIF fail-closed
- **R4 RESOLVED — APP PROXY:** TravelCore delivery endpoints; StorageKey never public
- **R5 RESOLVED — CONTRACT-ONLY:** `MediaAssetReference` + ArchitectureTests; no Destination MediaAssetId
- **R6 RESOLVED — DENY SVG**
- **Focal RESOLVED:** normalized [0,1] top-left (`TC-P06-T006-R1`)
- **R7 DEFERRED:** malware/AV scanning (recorded; non-blocking)
- **R8 UNRESOLVED:** OK for gate — no delete product UX/API in P06; do not invent lifecycle
- **R9 DEFERRED:** consumer alt override; Media owns default alt/caption only
- Access remains authorization authority for Admin Media mutations
- SEO Publish/Index authority unchanged
- No P07 Place / Tour / CMS start without USER `TRAVELCORE_PHASE_CONFIRM: P07`

## 5. Product surfaces (accepted)

| Surface | Path / note |
|---------|-------------|
| Admin Media job | `/[locale]/admin/media` — upload / inspect / alt / focal (no delete) |
| Media public delivery | App-proxy endpoints (Ready-only; anonymous) |
| Media contracts | `MediaAssetReference` · presentation helpers · no StorageKey exposure |
| Object storage | Media-owned abstraction · local FS (dev) · in-memory (tests) |

## 6. Evidence pack reference

[`docs/plans/P06-T012-hardening-and-evidence-pack.md`](P06-T012-hardening-and-evidence-pack.md)

## 7. Gate verdict (Cursor)

**PASS — P06 ready to mark COMPLETE pending architect accept of this RESULT.**  
This task does **not** start P07 / Place / Tour / CMS, and does **not** invent R8/R9 resolutions.
