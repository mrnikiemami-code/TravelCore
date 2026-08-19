# P24 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P24-PLAN` |
| Phase | P24 — B2B / Agency Commerce |
| Status | PLAN ACCEPTED · **P24-R1–R7 = RESOLVED** · **P24-R8 OPEN** · T001–T007 implemented · **not COMPLETE** |
| Baseline | `eea58e2` (`docs(dynamic-package): complete P23 acceptance gate`) |
| Authoritative sources | `docs/ROADMAP.md` § P24 · `docs/PROJECT-STATE.md` · `docs/architecture/04-module-boundaries.md` · `docs/domain/module-ownership-matrix.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/06-cross-module-communication.md` · `docs/architecture/07-data-architecture.md` · `docs/architecture/15-future-architecture-transition-map.md` · P13 Agency Marketplace · P19 Booking · P20 Payment · P21 HotelBooking · P22 Flight · P23 DynamicPackage |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document defines the P24 execution architecture and task decomposition.

> **Envelope note:** `TC-P24-T001`–`T006` ACCEPTED · `TC-P24-T007` operational boundary delivered · **do not execute `TC-P24-T008`** until architect accepts T007.

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
| `P24-R2` | Agency business identity boundary vs Party/Access/Identity | **RESOLVED** — `AgencyReference` / `AgencyRelationshipBoundary` / `AgencyMembershipBoundary` in B2B.Domain · Agency is business concept (not Identity) · agency users are Access subjects · agency organization relationship belongs to Party · no Agency aggregate · no persistence · no Booking/Payment relations |
| `P24-R3` | Agency membership & Access relationship boundary | **RESOLVED** — `AgencyMemberReference` / `AgencyAccessRelationshipBoundary` in B2B.Domain · membership intent only · agency users are Access subjects · B2B does not own users/authentication/authorization · no AgencyMember/User/Role/Permission tables · no invitation flow · Identity/Access/Party ownership unchanged |
| `P24-R4` | Agency commercial profile boundary vs Booking/Payment/Pricing | **RESOLVED** — `AgencyCommercialProfileBoundary` / `AgencyBusinessReference` / `CommercialCapabilityReference` in B2B.Domain · commercial profile intent only · B2B does not own financial execution · Payment owns money execution · Booking owns reservation execution · Pricing remains price authority · no Contract/Commission/Credit/Wallet/Settlement/Invoice |
| `P24-R5` | Agency distribution boundary vs Booking/Pricing/Payment | **RESOLVED** — `AgencyDistributionBoundary` / `SalesChannelReference` / `DistributionCapabilityReference` / `AgencyDistributionReference` · distribution intent only · distribution is not sales implementation · Booking/Pricing/Payment ownership unchanged · no commission/agency pricing/discount/contract/settlement |
| `P24-R6` | Agency commerce payment boundary vs Payment ownership | **RESOLVED** — `AgencyPaymentRelationshipBoundary` / `PaymentResponsibilityReference` / `CommercialPaymentCapabilityReference` / `AgencyPaymentReference` · PaymentTargetKind unchanged (TourBooking, HotelBooking, FlightBooking) · no Wallet/Credit/Settlement/Invoice/Commission payout · Payment ownership unchanged |
| `P24-R7` | Agency commerce operational boundary | **RESOLVED** — `AgencyOperationalBoundary` / `AgencyReportingReference` / `AgencyOperationalCapabilityReference` / `AgencyOperationalReference` · no Admin/Public API · no dashboard/reporting engine/audit system · no authorization changes · no booking/payment operation changes |
| `P24-R8` | Deferred/out-of-scope posture (providers, settlement, advanced finance) | OPEN |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P24-T001` — ownership/module/schema boundaries (**IMPLEMENTED / ACCEPTED**)
2. `TC-P24-T002` — agency business identity boundary (**IMPLEMENTED / ACCEPTED**)
3. `TC-P24-T003` — agency membership & Access relationship boundary (**IMPLEMENTED / ACCEPTED**)
4. `TC-P24-T004` — agency commercial profile boundary (**IMPLEMENTED / ACCEPTED**)
5. `TC-P24-T005` — agency distribution boundary (**IMPLEMENTED / ACCEPTED**)
6. `TC-P24-T006` — agency commerce payment boundary (**IMPLEMENTED / ACCEPTED**)
7. `TC-P24-T007` — agency commerce operational boundary (**IMPLEMENTED / AWAITING_ARCHITECT_REVIEW**)
8. `TC-P24-T008` — hardening and guardrails
9. `TC-P24-T009` — evidence pack
10. `TC-P24-GATE` — acceptance gate

### TC-P24-T007 — Agency commerce operational boundary

- Depends on **P24-R7**. **IMPLEMENTED / AWAITING_ARCHITECT_REVIEW.** Domain boundary models only · no Admin/Public API · no dashboard/reporting engine/audit system · no authorization/booking/payment operation changes · **TC-P24-T008 NOT EXECUTED**.

---

## 7. Plan outcome

- `TC-P24-T001`–`T006` **EXECUTED / ACCEPTED**.
- `TC-P24-T007` **EXECUTED** (boundary only).
- `TC-P24-T008` remains **NOT EXECUTED**.
