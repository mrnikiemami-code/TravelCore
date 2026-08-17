# TC-P11-T010 — Foreign Package / Departure hardening tests & evidence pack

**Task:** TC-P11-T010 — P11 hardening tests and evidence pack  
**Product HEAD:** `c8ce3f1` (`TC-P11-T009`)  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability (architect Auto-Execute).

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | TourProduct ≠ TourDeparture | **PASS** — distinct aggregates; Departure holds TourProductId |
| 2 | Departure ≠ Booking / Pricing / Payment | **PASS** — no engines/types; public Published ≠ bookable |
| 3 | Tour ≠ Flight ownership / HotelBooking | **PASS** — descriptive transport + PlaceId stay options only |
| 4 | P11-R1…R8 all RESOLVED | **PASS** — plan open-decisions table |
| 5 | No new domain entities in this task | **PASS** — evidence/docs + phase boundary guardrails only |

## 2. Accepted product commits (P11)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `66cab9b` | P11 implementation plan |
| T001 | `d8d6131` | TourDeparture scaffolding (P11-R1) |
| T002 | `6641fd7` | Schedule + IANA timezone (P11-R2) |
| T003 | `1b065de` | Capacity Min/Max Pax rules (P11-R3) |
| T004 | `bad8dd2` | Lifecycle status (P11-R4) |
| T005 | `1758729` | Transport segments descriptive (P11-R5) |
| T006 | `fa89798` | Accommodation options (P11-R6) |
| T007 | `20ffbc9` | Passenger occupancy rules (P11-R7) |
| T008 | `0b42a94` | Access + Admin Departure baseline |
| T009 | `c8ce3f1` | Public published departure hooks (P11-R8) |

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P11-R1** | TourDeparture ∈ Tour; 0..N per product; TourProduct ≠ Departure |
| **P11-R2** | LocalDate Start/End + required IANA TimeZoneId |
| **P11-R3** | Min/Max Pax capacity rules; Booking owns reservation consumption later |
| **P11-R4** | Draft/Published/Closed/Cancelled/Completed; ≠ CatalogStatus/SEO/Booking |
| **P11-R5** | Descriptive TransportSegment; Tour ≠ Flight |
| **P11-R6** | AccommodationOption (PlaceId + Nights + BoardType); ≠ HotelBooking |
| **P11-R7** | PassengerRule acceptance facts; ≠ Passenger/Booking |
| **P11-R8** | Public visibility = Published only; Published ≠ Bookable |

## 4. Boundary / ownership matrix

| Concern | Owner | P11 posture |
|---------|-------|-------------|
| Product definition | **TourProduct** | Reusable catalog |
| Execution instance | **TourDeparture** | Schedule/capacity/status/transport/stay/passenger rules |
| Admin mutations | **Access** `tour.departures.read/write` | Separate from products.write |
| Public visibility | **Published status only** | Anonymous published query |
| Hotel identity | **Place** | Logical PlaceId on options |
| Flight inventory / airline | **Out** | Labels only |
| Booking / Pricing / Payment / Search | **Out of P11** | Forbidden |

## 5. AI-readiness structured facts present

- Departure dates + timezone + duration days
- Capacity rules (min/max pax)
- Transport segments (mode + origin/destination labels)
- Accommodation options (nights + board + PlaceId)
- Passenger acceptance rules
- Lifecycle status (Published gate for public)

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Scaffolding | `TourDepartureBoundaryGuardrailTests` |
| Admin Access | `TourDepartureAdminAccessGuardrailTests` |
| Public detail | `TourPublicDetailBoundaryGuardrailTests` (published hooks) |
| Phase boundary | `TourDeparturePhaseBoundaryGuardrailTests` (T010) |
| Authz matrix | `TourDepartureAccessAuthorizationTests` |
| Domain unit | Tour.UnitTests (Departure schedule/capacity/status/transport/accommodation/passenger) |
| Access catalog | Access.UnitTests (departures read/write in AdminBaseline) |

## 7. Validation battery (T010 re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) |
| Tour.UnitTests | **PASS** | **83** passed |
| Access.UnitTests | **PASS** | **5** passed |
| ArchitectureTests | **PASS** | **114** passed (incl. T010 phase boundary) |
| Persistence.IntegrationTests | **PASS** | **21** passed |
| Host.IntegrationTests | **PASS** | **41** passed |
| Frontend `tsc --noEmit` | **PASS** | clean |
| `git diff --check` | **PASS** | clean |

**Total this battery (core):** 83 + 5 + 114 + 21 + 41 = **264** passed (plus FE tsc).

## 8. Ready for Gate

After architect ACCEPT of T010 → Auto-Execute **TC-P11-GATE** (architect statement). P12 PLAN may auto-start after Gate ACCEPT under continuity override.
