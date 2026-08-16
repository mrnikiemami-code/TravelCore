# P06 Evidence Pack — TC-P06-T012

**Task:** TC-P06-T012 — Phase hardening tests & evidence pack  
**Baseline HEAD:** `4afc867` (`docs: record TC-P06-T011 commit SHA 8b0de5a`)  
**Date:** 2026-08-16  
**Scope:** Validation / evidence only — tiny unambiguous P06 fixes allowed; **gate not executed**; **P06 not marked COMPLETE**; **P07 not started**.

## 1. Capability matrix (product commits)

| Task | Commit | Capability |
|------|--------|------------|
| PLAN | `87069e4` | Authoritative P06 plan |
| T001 | `e5bfd39` | Media module scaffolding · schema `media` |
| T002 | `020ce99` | MediaAsset metadata domain + persistence |
| T003 | `cf95e5c` | Media-owned object storage abstraction |
| T004 | `7f83885` | Access-backed upload + validation · R6 SVG DENY |
| T005 | `91444ad` | Sync variants + dimensions · R3 1600/960/320 |
| T006 | `166e9db` | Focal point persistence |
| T006-R1 | `b6f0cfb` | Focal coordinate policy · normalized [0,1] top-left |
| T007 | `85c8e7a` | Alt/caption translations (ADR 0008; no AltFa/AltEn) |
| T008 | `f50cce3` | Optimization contract · **R1 DEFER** WebP/AVIF |
| T009 | `3a25e7d` | App-proxy public delivery · **R4 APP PROXY** |
| T010 | `05ef0ac` | Consumer reference proof · **R5 CONTRACT-ONLY** |
| T011 | `8b0de5a` | Admin Media operational baseline (upload/inspect/alt/focal) |
| T012 | *(this task)* | Hardening battery + evidence pack for gate prep |

## 2. Ownership invariants

| Invariant | Posture |
|-----------|---------|
| Media owns bytes + asset metadata | Proven (T001–T009) · schema `media` · ArchitectureTests |
| Consumers own relationship meaning (role/order/gallery) | Proven contract-only (T010) · no Destination MediaAssetId |
| StorageKey never public | Proven (T009 app-proxy) |
| Media ↛ peer Infrastructure / Domain | ArchitectureTests peer-module guards |
| No cross-schema FK / generic Media link table | ArchitectureTests |
| SEO Publish/Index authority unchanged | Out of P06 mutation scope |
| Place / Tour / CMS engines absent | Scope review · P07+ NOT_STARTED |

## 3. Locked decisions R1–R9 (+ focal)

| ID | Decision | Classification | Gate posture |
|----|----------|----------------|--------------|
| R1 | WebP/AVIF conversion pipeline | **RESOLVED — DEFER** | Same-format variants only; evidence T008 |
| R2 | Object-storage ownership | **RESOLVED** | Media-owned `IMediaObjectStorage` (not Platform-wide) |
| R3 | Variant generation | **RESOLVED** | Sync · large=1600 / medium=960 / thumbnail=320 · fit-within · no crop/upscale · GIF fail-closed |
| R4 | Public URL strategy | **RESOLVED — APP PROXY** | TravelCore delivery endpoints · anonymous Ready-only |
| R5 | Destination MediaAssetId | **RESOLVED — CONTRACT-ONLY** | `MediaAssetReference` + ArchitectureTests |
| R6 | SVG acceptance | **RESOLVED — DENY** | `image/svg+xml` / `.svg` / detected SVG-XML |
| Focal | Coordinate space | **RESOLVED** | Normalized **[0,1] top-left** (`TC-P06-T006-R1`) |
| R7 | Malware/AV scanning | **DEFERRED** | Security requirement recorded; not blocking P06 gate prep |
| R8 | Physical vs soft-delete + orphan cleanup | **UNRESOLVED** | **OK for gate prep** — deletion is **not** in P06 product scope; **no delete UX / no domain-delete API** |
| R9 | Consumer alt override | **DEFERRED** | Media owns default alt/caption only; consumer override later |

### R8 explicit statement (gate prep)

**P06-R8 remains UNRESOLVED and that is acceptable for `TC-P06-GATE` preparation** because:

1. Product scope of P06 does not include MediaAsset domain deletion.
2. Repo scan found **no** Admin delete UI, **no** `MapDelete` on Media asset endpoints, and **no** domain soft-delete / delete lifecycle product behavior.
3. Existing `IMediaObjectStorage.DeleteAsync` is **technical blob compensation** only (upload/variant failure cleanup), explicitly documented as not R8.
4. Closing R8 requires a future architect decision; inventing delete semantics in P06 is forbidden.

## 4. Forbidden delete / lifecycle scan (T012)

| Surface | Result |
|---------|--------|
| `MediaEndpoints` HTTP verbs | GET/POST/PUT only — **no** asset `MapDelete` |
| Admin Media UI (`admin/media`, `features/admin-media`) | **no** delete/remove copy or actions |
| Media Domain | Comments affirm R8 open; no product delete API |
| Storage `DeleteAsync` | Technical compensation only (upload/variant) |

**Verdict:** No `BLOCKED_ARCHITECTURE_VIOLATION` — no forbidden delete product behavior introduced.

## 5. ArchitectureTests posture

| Guard | Status |
|-------|--------|
| Media schema `media` | PASS (`ArchitectureGuardrailTests`) |
| Peer modules must not reference Media.Infrastructure/Domain | PASS (`MediaConsumerReferenceGuardrailTests`) |
| Destination has no MediaAssetId persistence fields | PASS |
| No generic Media consumer link entities | PASS |
| `MediaAssetReference` stable contract (no StorageKey) | PASS |

**T012 decision:** No ArchitectureTests code change — no new unambiguous P06 boundary gap found during hardening.

## 6. Validation battery (this task)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 errors (xUnit1051 warnings in Media.UnitTests only) |
| Media.UnitTests | **PASS** | 65 passed |
| Access.UnitTests | **PASS** | 5 passed |
| ArchitectureTests | **PASS** | 25 passed |
| Persistence.IntegrationTests | **PASS** | 18 passed |
| Host.IntegrationTests | **PASS** | 34 passed |
| Frontend `npm run quality` | **PASS** | lint · typecheck · build · test:quality |
| `git diff --check` | **PASS** | clean |
| HEAD == origin/main (preflight) | **PASS** | `4afc867` · clean tree |

## 7. Host / authz evidence (representative)

| Area | Proof |
|------|-------|
| Upload + SVG deny + Access | `MediaUploadAccessAuthorizationTests` |
| Variant generate Access | `MediaVariantGenerateAccessAuthorizationTests` |
| Focal Access | `MediaFocalPointAccessAuthorizationTests` |
| Alt/caption Access + presentation | `MediaAssetTranslationAccessAuthorizationTests` |
| Admin list Access | `MediaAssetListAccessAuthorizationTests` |
| App-proxy delivery / StorageKey never public | `MediaAppProxyDeliveryTests` |
| Persistence migrations | `MediaMigrationLifecycleTests` |

## 8. Frontend evidence

| Surface | Path / note |
|---------|-------------|
| Admin Media job | `/[locale]/admin/media` — upload/inspect/alt/focal (T011) |
| No delete UX | Confirmed in admin-media feature |
| Presentation consumption | App-proxy URL helpers from Media contracts (T009/T010) |
| Quality gates | `p02-quality-checks.mjs` includes media allowlist checks |

## 9. Gate checklist preview (plan §10)

| # | Criterion | Evidence posture |
|---|-----------|------------------|
| 1 | Media module + separate schema / Media ↛ business modules | Architecture + Persistence |
| 2 | Upload → storage → Ready (Access-backed) | T004 + host |
| 3 | Variants + dimensions | T005 + unit/host |
| 4 | Focal point persisted/readable | T006 + host |
| 5 | Alt/caption localization (ADR 0008) | T007 + host |
| 6 | Bytes in object storage (not domain-table blobs) | T003 + unit |
| 7 | Optimization contract; WebP/AVIF deferred | T008 · **R1 DEFER** |
| 8 | Frontend presentation / remote policy | T009 + quality |
| 9 | Consumer relationship semantics (no gallery engines) | T010 · **R5** |
| 10 | Admin Media job-based + Access-backed | T011 + host |
| 11 | P07+ engines absent | Scope / ROADMAP |
| 12 | Evidence pack + green battery | **this document** |

## 10. Deferred / known non-blocking limitations

| Item | Note |
|------|------|
| R1 WebP/AVIF | Deferred; same-format variants only |
| R7 Malware/AV | Deferred security requirement |
| R8 Domain delete | Unresolved; **no delete product behavior** — OK for gate prep |
| R9 Consumer alt override | Deferred to consumer phases |
| Signed URL delivery | Deferred (R4 chose APP PROXY) |
| Platform-wide `IObjectStorage` | Not introduced (R2 Media-owned) |
| Destination assign / picker | Explicitly out of T011/T010 CONTRACT-ONLY |
| S3 vendor adapter | Local FS + in-memory adapters in P06; vendor later |
| GIF animated variant policy | Fail-closed under R3 |

## 11. Ready for gate?

**YES — evidence pack ready for `TC-P06-GATE`.**

This task does **not**:

- execute `TC-P06-GATE`
- mark P06 COMPLETE
- start P07 / Place / Tour / CMS
- silently resolve R8 or R9

Gate still requires architect acceptance of remaining tasks (as issued) and USER token `TRAVELCORE_TASK_CONFIRM: TC-P06-GATE` when the architect issues the gate.
