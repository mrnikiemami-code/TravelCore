# TC-P09-GATE — P09 Acceptance Evidence

**Task:** TC-P09-GATE — P09 Acceptance Gate  
**Observed gate execution HEAD (preflight):** `0334bae`  
**Baseline drift:** NONE — `HEAD == origin/main == 0334bae`; CLEAN tree  
**Date:** 2026-08-17  
**Scope:** Gate / acceptance only — no new P09 product features; **P10/P11 not started** until Gate ACCEPT; under continuity override P10 PLAN may auto-start after Gate ACCEPT.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER `TRAVELCORE_MODE: PIPELINE` | YES (continuity override ON) |
| Ceremonial `TRAVELCORE_TASK_CONFIRM: TC-P09-GATE` | **Not required** (continuity override 2026-08-17) |
| Architect Auto-Execute GATE envelope | YES |
| T001–T010 accepted | YES (product through T010 `0334bae`) |
| Working tree at gate start | CLEAN |
| HEAD == origin/main | `0334bae` |

## 2. Plan §10 acceptance checklist (GATE)

| # | Criterion | Evidence |
|---|-----------|----------|
| 1 | Tour module separate schema `tour`; no cross-schema writes | ArchitectureTests · `TourDbContext` schema `tour` · migration lifecycle |
| 2 | TourProduct shared core under Tour ownership | T002 + **P09-R1** · `TourProductId` · Experience/Package typed specialization deferred |
| 3 | Localization without `TitleFa`/`TitleEn` | T003 · `TourProductTranslation` · ADR 0008 |
| 4 | Classification / Origin / Destination by ID/contracts | T004 + **P09-R2** · logical refs · no cross-schema FK |
| 5 | Agency by Party/Agency ID; Party ≠ Tour merge | T005 + **P09-R3** · `AgencyId` 0..1 · Party.Contracts |
| 6 | Services / Policies / Requirements baseline | T006 |
| 7 | Tour↔Media; MediaAssetId only | T007 + **P09-R8** · Cover/Gallery · no StorageKey |
| 8 | Publishing + Access-backed Admin Tour | T008/T009 · Draft\|Published\|Inactive · `tour.products.write` · `/admin/catalog/tours` |
| 9 | Public Tour Core hooks | T008/T010 · `/[locale]/tours/[slug]` |
| 10 | SEO hooks; IndexPolicy default R6 | T008/T010 + **P09-R5/R6** · path `tours/{slug}` · default **noindex, follow** · Published ≠ Index |
| 11 | No TourDeparture / FlightSegment / TourHotelOption / Experience itinerary product | Boundary guards · P10/P11 deferred |
| 12 | No Pricing · Booking · Search · Content CMS ownership in Tour | Boundary guards |
| 13 | Experience/Package not unlocked nullable blob | **P09-R7 RESOLVED DEFER** specialty to P10/P11 |
| 14 | Evidence pack + tests green + clean tree | [`P09-T010-hardening-and-evidence-pack.md`](P09-T010-hardening-and-evidence-pack.md) + gate re-run below |

## 3. Validation battery (gate re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build` TravelCore.Api | **PASS** | 0 Warning(s), 0 Error(s) |
| Tour.UnitTests | **PASS** | **32** passed |
| Access.UnitTests | **PASS** | **5** passed |
| ArchitectureTests | **PASS** | **93** passed |
| Persistence.IntegrationTests | **PASS** | **21** passed |
| Host.IntegrationTests | **PASS** | **40** passed |
| Frontend `npx tsc --noEmit` | **PASS** | exit 0 |
| `git diff --check` | **PASS** | exit 0 |

**Total tests this battery:** 191 passed (32+5+93+21+40).

## 4. Locked decisions preserved (R1–R8 all RESOLVED)

- **P09-R1 RESOLVED:** Core TourProduct + Typed Specialization; `TourProductId`; TourDeparture separate future aggregate
- **P09-R2 RESOLVED:** Destinations 0..N · Origin 0..1 · logical only · Destination.Contracts
- **P09-R3 RESOLVED:** AgencyId 0..1 · PartyKind.Agency · Party.Contracts
- **P09-R4 RESOLVED:** Draft \| Published \| Inactive; Published = catalog-visible ≠ bookable; no hard-delete in P09
- **P09-R5 RESOLVED:** TourProductTranslation owns current Slug; SEO owns history/redirects/IndexPolicy; `tours/{slug}`
- **P09-R6 RESOLVED:** default missing IndexPolicy = **noindex, follow**; Published ≠ Index
- **P09-R7 RESOLVED:** Specialty DEFERRED to P10/P11
- **P09-R8 RESOLVED:** Cover 0..1 · Gallery 0..N · MediaAssetId logical · no StorageKey/FK

## 5. Product surfaces (accepted)

| Surface | Path / note |
|---------|-------------|
| Admin Tour job | `/[locale]/admin/catalog/tours` — create/list/open/translate/slug/status/classification/media Ready picker · SEO publish |
| Public Tour detail | `/[locale]/tours/[slug]` — Server Component · media presentation · compose SEO · fallback noindex,follow |
| Tour schema | `tour` — products · translations · destinations · catalog facts · media links |
| SEO publication | `POST /api/seo/publication/tour-product` · path `tours/{slug}` · no IndexPolicy mutation |

## 6. Evidence pack reference

[`docs/plans/P09-T010-hardening-and-evidence-pack.md`](P09-T010-hardening-and-evidence-pack.md)

## 7. Architect STOP rules honored

| Rule | Honored |
|------|---------|
| Do NOT start P10 product implementation before Gate ACCEPT | YES |
| Do NOT invent TourDeparture/Booking/Pricing/Search | YES |
| Do NOT mark P09 COMPLETE before architect ACCEPT of this RESULT | YES (this document is evidence; COMPLETE awaits ACCEPT) |

## 8. Gate verdict (Cursor)

**PASS** — ready for architect ACCEPT. After ACCEPT: P09 COMPLETE; under continuity override auto-start **TC-P10-PLAN**.
