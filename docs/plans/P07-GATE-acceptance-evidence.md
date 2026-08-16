# TC-P07-GATE — P07 Acceptance Evidence

**Task:** TC-P07-GATE — P07 Acceptance Gate  
**Envelope expected baseline:** `fcefadd` (`docs: record TC-P07-T008 commit SHA f7843cc in ledger`; after T008 ACCEPTED)  
**Observed gate execution HEAD (preflight):** `fcefadd`  
**Baseline drift:** NONE — `HEAD == origin/main == fcefadd`; CLEAN tree  
**Date:** 2026-08-16  
**Scope:** Gate / acceptance only — no new P07 product features; **P08 not started**; **P07-R3 not closed by invention**.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER `TRAVELCORE_PHASE_CONFIRM: P07` | YES (prior) |
| USER `TRAVELCORE_TASK_CONFIRM: TC-P07-GATE` | YES (ChatGPT / PIPELINE continuation) |
| USER `TRAVELCORE_MODE: PIPELINE` | YES |
| Architect Auto-Execute GATE envelope | YES (DOM truncated after PRE-FLIGHT; authoritative scope from plan § TC-P07-GATE + §10) |
| Critical-Gate / User-Confirmed | YES (architect: `Critical-Gate: YES` · `User-Confirmed: YES`) |
| T001–T008 accepted | YES (product commits through T008 `f7843cc`; hygiene through `fcefadd`) |
| Working tree at gate start | CLEAN |
| HEAD == origin/main | `fcefadd` |

## 2. Plan §10 acceptance checklist (GATE)

| # | Criterion | Evidence |
|---|-----------|----------|
| 1 | Place module separate schema; no cross-schema writes | ArchitectureTests (45) · schema `place` · `PlaceDbContext` · Place boundary guards |
| 2 | Hotel/Restaurant/Attraction catalog identity under Place ownership | T002 + **P07-R1** · `places` + typed specialization tables · `PlaceId` only |
| 3 | Localization without `NameFa`/`NameEn` | T003 · `PlaceTranslation` locale rows · ADR 0008 |
| 4 | Destination relationship by reference; Destination ≠ Place | T003 + **P07-R2** · optional nullable `DestinationId` · `IDestinationExistenceQuery` · no cross-schema FK |
| 5 | Geo/address on Place catalog entities | T003 · Place-owned address/coordinates |
| 6 | Facilities/classification/catalog status baseline | T004 · `place_facilities` · ClassificationCode · `PlaceCatalogStatus` Draft/Active/Inactive |
| 7 | Place↔Media relations owned by Place; MediaAssetId refs only | T005 · Cover/Gallery · `place_media_links` · Media.Contracts readiness |
| 8 | Admin Place baseline Access-backed; job-based UX | T006 + T006-R1 · `place.places.write` · `/[locale]/admin/catalog/places` · Ready picker |
| 9 | Public Place detail baseline | T007 · `/[locale]/places/[slug]` · Active-only public |
| 10 | SEO hooks without SEO owning Place catalog text | T007 + **P07-R4/R5** · Place owns current Slug; SEO owns binding/history/IndexPolicy; default **noindex, follow** |
| 11 | No HotelBooking / live inventory / reservation / voucher in Place | Architecture `PlaceCatalogBoundaryGuardrailTests` + domain comments |
| 12 | Evidence pack + tests green + clean tree | [`P07-T008-hardening-and-evidence-pack.md`](P07-T008-hardening-and-evidence-pack.md) + gate re-run below |

## 3. Validation battery (gate re-run)

| Suite | Result |
|-------|--------|
| `dotnet build TravelCore.sln` | PASS (0 errors; prior xUnit analyzer warnings only) |
| ArchitectureTests | **45 PASS** |
| Place.UnitTests | **26 PASS** |
| Destination.UnitTests | **7 PASS** (included for gate completeness) |
| Media.UnitTests | **67 PASS** |
| Seo.UnitTests | **41 PASS** |
| Access.UnitTests | **5 PASS** |
| Persistence.IntegrationTests | **19 PASS** |
| Host.IntegrationTests | **36 PASS** |
| Frontend `npm run quality` | PASS (lint · typecheck · build · 12 node tests · p02 PASS) |
| `git diff --check` | PASS |

## 4. Locked decisions preserved

- **P07-R1 RESOLVED:** Core Place + typed Hotel/Restaurant/Attraction specialization; `PlaceId` only; no TPH; no HotelBooking fields
- **P07-R2 RESOLVED:** Optional single DestinationId (0..1); no cross-schema FK; Contracts existence validation
- **P07-R3 UNRESOLVED:** OK for gate — no Delete/Archive product capability in P07; `PlaceCatalogStatus` is not Deleted/Archived; do not invent lifecycle
- **P07-R4 RESOLVED:** Place owns current locale-specific `PlaceTranslation.Slug`; SEO owns route history/redirects/IndexPolicy
- **P07-R5 RESOLVED:** Default **noindex, follow**; Active/public/publish ≠ Index; no Destination IndexPolicy inheritance
- Place ≠ Destination · Place ≠ Media · Place ≠ SEO · Hotel Catalog ≠ Hotel Booking
- Access remains authorization authority for Admin Place mutations
- P08 / CMS / Tour / HotelBooking **NOT_STARTED** without USER `TRAVELCORE_PHASE_CONFIRM: P08`

## 5. Product surfaces (accepted)

| Surface | Path / note |
|---------|-------------|
| Admin Place job | `/[locale]/admin/catalog/places` — catalog ops + Ready media picker + slug/SEO publish hooks (no delete) |
| Public Place detail | `/[locale]/places/[slug]` — Server Component · Cover + Gallery · Destination compose · default noindex,follow |
| Place schema | `place` — places · hotels/restaurants/attractions · translations · facilities · media_links |
| Contracts | Destination existence · Media readiness/presentation · SEO Place publication |

## 6. Evidence pack reference

[`docs/plans/P07-T008-hardening-and-evidence-pack.md`](P07-T008-hardening-and-evidence-pack.md)

## 7. R3 honesty check (gate-required)

| Check | Result |
|-------|--------|
| P07-R3 still recorded UNRESOLVED | YES |
| Hard-delete / archive / soft-delete product shipped | NO |
| Admin Delete UX / HTTP | NO (guardrailed) |
| CatalogStatus used as Deleted/Archived synonym | NO |
| Gate invents R3 resolution | **NO** |

## 8. Gate verdict (Cursor)

**PASS — P07 ready to mark COMPLETE pending architect accept of this RESULT.**  
This task does **not** start P08 / CMS / Tour / HotelBooking, and does **not** invent P07-R3 resolution.
