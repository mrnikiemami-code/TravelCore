# TC-P10-T009 — Experience hardening tests & evidence pack

**Task:** TC-P10-T009 — Experience hardening tests and evidence pack  
**Product HEAD:** `0b6f191` (`TC-P10-T008`) · hygiene `792b8f5`  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability (architect Auto-Execute).  
**Note on plan remap:** Architect remapped remaining P10 work to **T009 = hardening/evidence** then **GATE**. Original plan items *Admin Experience itinerary UI* and *dedicated Public Experience Detail archetype* are **not invented here**; Experience remains a TourProduct specialization and reuses P09 Tour Admin/Public surfaces where applicable. Explicit DEFER of those UI expansions is recorded below.

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Experience ≠ Departure / Booking / Pricing / Search / Inventory | **PASS** — architecture + domain type inventory |
| 2 | No duplicate ownership (Media / Party / Destination / Place) | **PASS** — logical refs only; Contracts validation |
| 3 | AI-readiness structured facts | **PASS** — Difficulty · Eligibility · Equipment · Guide · Media roles · CatalogStatus |
| 4 | P10-R1…R8 all RESOLVED | **PASS** — plan §11 |
| 5 | No new domain entities in this task | **PASS** — evidence/docs + guardrails only |

## 2. Accepted product commits (P10)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `d6d27e6` | P10 implementation plan |
| T001 | `e5490ae` | Experience specialization 1:1 |
| T002 | `757c9b8` | Itinerary / Day / Stop (P10-R1) |
| T003 | `85553b7` | Stop Destination/Place links (P10-R2) |
| T004 | `7589ad1` | Meals + Accommodation (P10-R3/R5) |
| T005 | `f7ce58c` | Difficulty / Eligibility / Equipment / LocalTransport (P10-R6) |
| T006 | `e3dbea6` | Guide assignments via Party (P10-R7) |
| T007 | `f262084` | Media posture = TourProduct Cover/Gallery (P10-R4) |
| T008 | `0b6f191` | Publishability reuses TourCatalogStatus (P10-R8) |

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P10-R1** | Experience owns Itinerary 0..1; Day/Stop under Itinerary |
| **P10-R2** | Stop: DestinationId 0..1 + PlaceId 0..1 (Attraction-kind); logical; no exclusivity |
| **P10-R3** | Accommodation plan 0..N; optional Place Hotel logical; no TourHotelOption/HotelBooking |
| **P10-R4** | Cover/Gallery via TourProductMediaLink; Day/Stop media DEFERRED |
| **P10-R5** | Meals on Day; Breakfast/Lunch/Dinner/Other; unique day+type |
| **P10-R6** | Difficulty enum; Eligibility code/value/detail; Equipment Required/Recommended |
| **P10-R7** | ExperienceGuideAssignment; GuidePartyId Person; Primary/Assistant |
| **P10-R8** | Reuse TourCatalogStatus; publish gate; Published ≠ bookable |

## 4. Boundary / ownership matrix

| Concern | Owner | Experience posture |
|---------|-------|--------------------|
| Media assets / StorageKey | **Media** | Tour holds MediaAssetId links only |
| Guide person identity | **Party** | Logical GuidePartyId |
| Destination geography | **Destination** | Logical DestinationId |
| Place / Attraction / Hotel facts | **Place** | Logical PlaceId |
| Catalog visibility | **TourProduct.CatalogStatus** | Experience publishability evaluator |
| Departure / Flight / HotelOption / Booking / Pricing / Search | **Out of P10** | Forbidden types not introduced |

## 5. AI-readiness facts present

- Difficulty (closed enum)
- Eligibility requirements (structured code/value/detail)
- Equipment (Required/Recommended)
- Local transport codes
- Guide assignments (Party + Role)
- Cover / Gallery semantic media roles
- CatalogStatus (Draft / Published / Inactive)

## 6. Explicit DEFERs (not blockers under architect remap)

| Item | Status | Note |
|------|--------|------|
| Admin Experience itinerary job UX expansion | **DEFERRED** | Reuse P09 Admin Tour catalog for TourKind.Experience; dedicated itinerary editor not invented in T009 |
| Dedicated Public Experience Detail archetype polish | **DEFERRED** | Reuse P09 public Tour detail for Experience products; itinerary composition UI polish may follow after Gate |
| Day/Stop media roles | **DEFERRED** | Per P10-R4 |
| Departure / Pricing / Booking | **OUT** | P11+ |

## 7. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Experience specialization | `TourExperienceSpecializationGuardrailTests` |
| Stop links | T003 section in same suite |
| Media posture | `ExperienceMediaPostureGuardrailTests` · `TourMediaRelationGuardrailTests` |
| Publishability | `ExperiencePublishabilityGuardrailTests` |
| Forbidden P11 types | Package/Departure/FlightSegment/TourHotelOption inventory asserts |
| Unit domain | Tour.UnitTests (Experience itinerary / meals / ops / guide / media / publishability) |
| Persistence | `TourMigrationLifecycleTests` (14 migrations · Experience tables) |

## 8. Validation battery (T009 re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| Api build | **PASS** | 0 Warning(s), 0 Error(s) |
| Tour.UnitTests | **PASS** | **52** passed |
| ArchitectureTests | **PASS** | **104** passed (incl. ExperiencePhaseBoundaryGuardrailTests) |
| Persistence.IntegrationTests | **PASS** | **21** passed |
| `git diff --check` | **PASS** | clean |

**Total this battery:** 177 passed (52+104+21).

## 9. Ready for Gate

After architect ACCEPT of T009 → Auto-Execute **TC-P10-GATE** (architect statement). P11 PLAN may auto-start after Gate ACCEPT under continuity override.
