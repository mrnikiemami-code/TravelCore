# P24 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P24-PLAN` |
| Phase | P24 — B2B / Agency Commerce |
| Status | PLAN ACCEPTED · **P24-R1 = RESOLVED** · **P24-R2–R8 OPEN** · T001 implemented · **not COMPLETE** |
| Baseline | `eea58e2` (`docs(dynamic-package): complete P23 acceptance gate`) |
| Authoritative sources | `docs/ROADMAP.md` § P24 · `docs/PROJECT-STATE.md` · `docs/architecture/04-module-boundaries.md` · `docs/domain/module-ownership-matrix.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/06-cross-module-communication.md` · `docs/architecture/07-data-architecture.md` · `docs/architecture/15-future-architecture-transition-map.md` · P13 Agency Marketplace · P19 Booking · P20 Payment · P21 HotelBooking · P22 Flight · P23 DynamicPackage |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document defines the P24 execution architecture and task decomposition.

> **Envelope note:** Authored after `TC-P23-GATE = ACCEPTED` from repository SoT. `TC-P24-T001` implements schema `b2b` foundation only; **do not execute `TC-P24-T002`** until architect accepts T001.

---

## 0. Next-phase resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior phase status | **P23 COMPLETE / ACCEPTED** |
| Authoritative next phase | **P24 — B2B / Agency Commerce** |
| Declared status before this plan | **PLANNED / NOT_STARTED** |
| PLAN already existed? | **NO** |
| P24 product started? | **YES** — `TC-P24-T001` foundation (schema `b2b`; no product tables) |

---

## 1. Phase purpose

P24 introduces B2B/Agency commerce boundaries on top of existing module ownership without collapsing Tour, Booking, Flight, HotelBooking, DynamicPackage, Pricing, or Payment boundaries.

Planned scope themes from SoT:

- Agency access posture and authorization boundaries
- B2B contracts and commercial rule boundaries
- Partner-specific pricing/booking orchestration boundaries
- Credit/commercial policies only where architecture proves ownership

---

## 2. Preserved locked architecture

P24 must preserve:

1. Schema-per-module and no peer-schema FK.
2. No shared DbContext across modules.
3. No distributed transactions.
4. `Price != Quote != Booking != Payment`.
5. Payment remains money-movement owner.
6. Search is not transaction SoT.
7. Existing closed confirmation/cancellation boundaries from P19–P23.
8. No fake production providers/suppliers.

---

## 3. Current SoT baseline snapshot

- P13 Agency Marketplace is complete and remains commercial-layer SoR for agency offer context.
- P19 Booking remains TourDeparture booking owner.
- P20 Payment remains payment/refund owner.
- P21 HotelBooking and P22 Flight stay independent transactional owners.
- P23 DynamicPackage is complete and does not transfer execution ownership from Flight/HotelBooking/Payment.
- P24 foundation started via `TC-P24-T001` (schema `b2b`; no product tables). Further decisions remain OPEN until architect lock.

---

## 4. Decision inventory for P24 (open for architect locks)

| ID | Topic | Status |
|----|-------|--------|
| `P24-R1` | Agency identity/auth boundary vs Party/Access | **RESOLVED** — independent B2B module · schema `b2b` · **B2B != Identity** · **B2B != Access** · **B2B != Party** · **B2B != Booking** · **B2B != Payment** · **B2B != AgencyMarketplace** · Agency is business concept (not Identity) · agency users are Access subjects · agency organization relationship belongs to Party · Identity/Access/Party ownership unchanged · Payment target kinds unchanged (3 only) |
| `P24-R2` | Contract ownership model and lifecycle | OPEN |
| `P24-R3` | Partner pricing authority vs Pricing module | OPEN |
| `P24-R4` | Partner booking orchestration boundary vs Booking/Flight/HotelBooking/DynamicPackage | OPEN |
| `P24-R5` | Credit/commercial ledger ownership vs Payment/Accounting | OPEN |
| `P24-R6` | Public/Admin/Agency surface boundaries and authorization | OPEN |
| `P24-R7` | Reporting/read-model boundaries and operational visibility | OPEN |
| `P24-R8` | Deferred/out-of-scope posture (providers, settlement, advanced finance) | OPEN |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P24-T001` — ownership/module/schema boundaries (**IMPLEMENTED / AWAITING_ARCHITECT_REVIEW**)
2. `TC-P24-T002` — contract lifecycle boundary
3. `TC-P24-T003` — partner pricing boundary
4. `TC-P24-T004` — partner booking boundary
5. `TC-P24-T005` — credit/commercial policy boundary
6. `TC-P24-T006` — API/surface and authorization boundary
7. `TC-P24-T007` — reporting/operational boundary
8. `TC-P24-T008` — hardening and guardrails
9. `TC-P24-T009` — evidence pack
10. `TC-P24-GATE` — acceptance gate

### TC-P24-T001 — B2B module / schema foundation

- Depends on **P24-R1**. **IMPLEMENTED / AWAITING_ARCHITECT_REVIEW.** Independent `B2B.Contracts` / `B2B.Domain` / `B2B.Infrastructure` · schema `b2b` · host registration after DynamicPackage without endpoints · no Agency/Contract/Commission/CreditLimit/Wallet/Settlement entities · no Booking abstraction · no Payment target · **TC-P24-T002 NOT EXECUTED**.

---

## 6. IN / OUT for T001

### IN
- Independent B2B module + schema `b2b` foundation
- Host registration + guardrail tests
- P24-R1 recorded RESOLVED

### OUT
- Agency/Contract/Commission/CreditLimit/Wallet/Settlement entities
- Booking abstraction / Payment target / API / Frontend
- Identity / Access / Party ownership changes
- Executing `TC-P24-T002`

---

## 7. Plan outcome

- P24 plan document created.
- P24 task envelope captured.
- SoT updated for T001 completion.
- `TC-P24-T001` **EXECUTED** (foundation only).
- `TC-P24-T002` remains **NOT EXECUTED**.
