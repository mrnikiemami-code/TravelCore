# TC-P12-GATE — P12 Acceptance Evidence

**Task:** TC-P12-GATE — P12 Pricing Acceptance Gate  
**Baseline HEAD:** `a522dd5` (`TC-P12-T009` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Gate / acceptance only — no new pricing capability; **P13 not started** until Gate ACCEPT (continuity may auto-start P13 PLAN after ACCEPT).

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER PIPELINE + continuity override | YES |
| Ceremonial GATE token | **Not required** |
| Architect Auto-Execute GATE | YES |
| T001–T009 ACCEPTED | YES |
| Evidence pack | YES — [`P12-T009-hardening-and-evidence-pack.md`](P12-T009-hardening-and-evidence-pack.md) |
| Working tree at gate start | CLEAN (`a522dd5`) |

## 2. Checklist (architect GATE)

| # | Criterion | Result |
|---|-----------|--------|
| 1 | Independent Pricing module · schema `pricing` (P12-R1) | **PASS** — T001 |
| 2 | Money / Currency platform reuse (P12-R2) | **PASS** — T002 |
| 3 | Price + PriceComponent; polymorphic `TargetType`+`TargetId` (P12-R3) | **PASS** — T003 |
| 4 | Quote snapshot + expiration; Price ≠ Quote (P12-R4) | **PASS** — T004 |
| 5 | Occupancy / passenger **category** pricing; ≠ Booking passenger (P12-R5) | **PASS** — T005 |
| 6 | Admin Pricing owned by Pricing, not Tour Admin (P12-R6) | **PASS** — T006 |
| 7 | FX boundary; no ExchangeRate table / no conversion (P12-R7) | **PASS** — T007 |
| 8 | Public read-only price summary; no Book Now (P12-R8) | **PASS** — T008 |
| 9 | Hardening / evidence / phase boundary | **PASS** — T009 |
| 10 | Price ≠ Quote ≠ Payment / Booking Amount | **PASS** — phase boundary tests |
| 11 | Pricing ≠ Tour table ownership · no Tour FK | **PASS** — guardrails |
| 12 | No Booking / Payment / Checkout / FX engine | **PASS** — guardrails |

## 3. Locked decisions

**P12-R1…R8 all RESOLVED** — see [`P12-implementation-plan.md`](P12-implementation-plan.md) open-decisions table.

Agency override of rates remains **UNRESOLVED** (prefer DEFER to P13) — not invented here.

## 4. Validation battery (gate re-run / T009 battery)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) · 15 Warning(s) (unrelated xUnit analyzers) |
| Pricing.UnitTests | **PASS** | **63** |
| ArchitectureTests | **PASS** | **145** |
| Persistence.IntegrationTests | **PASS** | **23** |
| Host.IntegrationTests | **PASS** | **43** |
| Frontend `tsc --noEmit` | **PASS** | clean |
| `git diff --check` | **PASS** | clean |

**Total core:** 63 + 145 + 23 + 43 = **274** passed (+ FE tsc).

## 5. Explicit OUT / DEFER

- Booking engine / reservation / hold / inventory — **later (P19)**
- Payment capture / settlement — **later (P20)**
- FX Service ExchangeRate + conversion — **deferred** (boundary recorded in T007)
- Checkout / public Book Now CTA — **later (P14+)**
- Product-level (TourProduct) pricing — **DEFER** (P12-R3)
- Agency override of rates / marketplace — **UNRESOLVED · prefer P13**
- Search indexing of prices — **P15**
- Dedicated Admin Pricing Next.js page — **not invented** (API baseline exists)

## 6. Architect STOP rules honored

| Rule | Honored |
|------|---------|
| No P13 product before Gate ACCEPT | YES |
| No inventing unlocked R# | YES (R1–R8 resolved; agency-rate override left UNRESOLVED) |
| No new pricing capability in GATE | YES |
| No force-push / history rewrite | YES |

## 7. Gate outcome

**ACCEPTED** (`b372367`) · P12 COMPLETE · P13 PLAN auto-started (no ceremonial Gate wait).
