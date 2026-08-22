# P39-T006 — Commercial Finance Domain Notes

| Field | Value |
|-------|--------|
| Task-ID | `TC-P39-T006` |
| Phase | P39 — Multi-Agency Commercial Finance Foundation |
| Date | 2026-08-22 |
| Depends on | `TC-P39-T005` DECISIONS_LOCKED |

## Scope delivered

- Module `CommercialFinance` with Contracts / Domain / Infrastructure
- PostgreSQL schema `commercial_finance`
- Persisted skeleton entities:
  - `CommissionAgreement`
  - `AgencyOfferCommissionOverride` (logical `agency_offer_id` only)
  - `CommercialObligation` + lifecycle guards
  - `SettlementPeriod` / `SettlementRecord`
  - `PayoutInstruction`
  - `CommercialFinanceEventConsumptionRecord` (idempotency)
- Market policy enum: `CommercialFinanceMarketPolicy` — Iran, Uae
- Optional owned Money snapshots on obligation/payout when value supplied
- Access permissions + admin read endpoints (no fake KPI data)
- Migrations: `InitialCommercialFinanceScaffolding` + `P39CommercialFinancePersistenceFoundation`

## Locked inequalities preserved

```text
Commission != Pricing
Settlement != Payment
Payout != Booking
Commercial Obligation != Invoice
AgencyOffer != Financial Transaction
```

## Explicitly NOT implemented (per envelope)

- Commission percentages / formulas
- Settlement jobs / close schedulers
- Payout execution / bank rails
- Tax / FX execution
- Automatic PaymentSucceeded event handlers

## Evidence references (logical only)

| Ref | Storage |
|-----|---------|
| `agency_profile_id` | Guid column — no FK to `agency_marketplace` |
| `agency_offer_id` | Guid column — no AgencyOffer mutation |
| `booking_id` | Guid column — no FK to `booking` |
| `payment_id` | Guid column — no FK to `payment` |

## Architectural concerns

- `ICommercialFinanceEvidencePort` is a null stub until cross-module read adapters are explicitly authorized.
- Settlement/Payout approval write endpoints deferred — read skeleton only in T006.

## Policy source

Locked decisions: [`docs/plans/P39-commercial-finance-decisions-locked.md`](../../../plans/P39-commercial-finance-decisions-locked.md)
