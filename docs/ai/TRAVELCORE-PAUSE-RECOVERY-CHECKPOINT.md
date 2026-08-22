# TravelCore — Pause / Recovery Checkpoint

**Generated:** 2026-08-22  
**Checkpoint commit:** _(set on commit)_  
**Authority:** Architect pause directive · repository-backed recovery

---

## Purpose

Durable repository-backed recovery checkpoint before pausing TravelCore so a future architect can resume from **repository truth** without relying on chat history.

This is **NOT** a feature task. Do not implement new product behavior. Do not continue `TC-P40-T002` beyond work already present. Do not create the next task.

---

## Canonical Project Position

| Field | Value |
|-------|--------|
| **Project Status** | `PAUSED_BY_ARCHITECT` |
| **Last Architect Accepted Phase** | P39 — Multi-Agency Commercial Finance Foundation |
| **P39 Maturity** | `READY_FOUNDATION` |
| **Current Product Phase** | P40 — Marketplace Merchandising & Experience Depth |
| **Last Architect Accepted Task** | `TC-P40-T001` |
| **TC-P40-T002** | `ISSUED_BUT_NOT_ARCHITECT_ACCEPTED` |
| **TC-P40-T002_STATE** | `NOT_STARTED` |
| **Resume Rule** | Recover repository state, working tree, `origin/main`, and actual `TC-P40-T002` state before issuing any next task |

**Never** mark `TC-P40-T002` PASS/ACCEPTED unless a real Cursor RESULT exists and the Architect accepts it.

---

## Repository Snapshot (at checkpoint)

| Field | Value |
|-------|--------|
| Branch | `main` |
| HEAD | `8ff420c` (`docs(P40-T001): Marketplace experience audit and plan`) |
| `origin/main` | `8ff420c` |
| HEAD == origin/main | **YES** |
| Working tree | Clean except preserved untracked pipeline inbox stubs |

### TC-P40-T002 recovery verification

| Check | Result |
|-------|--------|
| TC-P40-T002 implementation commit? | **NO** |
| `docs/product-experience/evidence/P40-T002/` exists? | **NO** |
| Frontend/backend comparison changes? | **NO** |
| Cursor RESULT for T002? | **NO** |
| Local-only T002 work? | **NO** |
| **Recorded state** | **`NOT_STARTED`** |

Envelope `TC-P40-T002` was issued in Architect chat for Public Multi-Agency Offer Comparison; **not executed** in repository.

---

## Locked Architecture Baseline

- Modular Monolith
- Backend: .NET 10 ASP.NET Core Minimal API
- Frontend: Next.js 16 / React 19 / TypeScript / Tailwind
- PostgreSQL schema-per-module; no shared DbContext
- UUID v7 · NodaTime
- Money/Currency ADR preserved; Toman is display unit, not CurrencyCode
- Server Component First · FA / EN / AR · RTL/LTR · mobile-first · SEO-first

### Locked separations

```text
Identity != Party != Access
TourProduct != TourDeparture
Hotel Catalog != HotelBooking
Price != Quote != Booking != Payment
AgencyOffer != TourDeparture != Price != Quote
Commission != Pricing · Settlement != Payment · Payout != Booking
Commercial Obligation != Invoice · Audit != Financial Ledger
PaymentSucceeded != SettlementClosed · Approved != Paid
Search != SEO · Frontend != Source of Truth
```

---

## P38 Accepted State

`READY_COMMERCE_VERTICAL_WITH_GOVERNANCE`

TourProduct → AgencyOffer(s) → Customer Selection → Quote Context → Booking → Payment Boundary

No hidden Commission / Settlement / Payout implementation.

---

## P39 Accepted State

`READY_FOUNDATION` — CommercialFinance module skeleton (`commercial_finance` schema), entities, permissions, evidence/idempotency foundation.

**Locked business decisions:** see [`docs/plans/P39-commercial-finance-decisions-locked.md`](../plans/P39-commercial-finance-decisions-locked.md)

**Intentionally NOT implemented:** commission formulas, settlement jobs, automatic payout, bank integrations, tax engine, live FX provider, accounting ledger, invented finance reporting.

---

## P40 Accepted Direction

`TC-P40-T001` verdict: `FOUNDATION_WITH_PARTIAL_SELLABLE_PUBLIC_SLICE`

Plan: [`docs/plans/P40-marketplace-merchandising-experience-depth.md`](../plans/P40-marketplace-merchandising-experience-depth.md)  
Audit: [`docs/product-experience/evidence/P40-T001/EXPERIENCE-AUDIT.md`](../product-experience/evidence/P40-T001/EXPERIENCE-AUDIT.md)

**#1 gap:** multi-agency offer **comparison** UI does not exist (selection/radio works).

---

## Known Blockers

1. **Project paused by Architect** — no new tasks until resume reconciliation
2. **TC-P40-T002** issued but **NOT_STARTED** — must reconcile before continuing P40
3. **P35** paused (payment provider external facts)
4. **Finance engines** deferred (commission formulas, settlement jobs, payout execution, tax, live FX, bank)
5. **DEMOFEED / technical label debt** on public surfaces
6. **Campaign/Promotion/Placement** architecture not yet ADR-locked

---

## Untracked Preserved Artifacts

Pipeline inbox stubs (not committed; preserved intentionally):

- `docs/pipeline/inbox/TC-P35-T009.task.md`
- `docs/pipeline/inbox/TC-P37-GATE.gate.md`
- `docs/pipeline/inbox/TC-P37-T004.task.md`
- `docs/pipeline/inbox/TC-P38-T001.task.md` through `TC-P38-T015` (partial set)

---

## Recovery Source-of-Truth Files

Verify mutual consistency on resume:

- `docs/PROJECT-STATE.md`
- `docs/ROADMAP.md`
- `docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md`
- `docs/ai/TRAVELCORE-PAUSE-RECOVERY-CHECKPOINT.md` (this file)
- `docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`
- `docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md`
- `docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`

---

## Future Architect Recovery Procedure

Before issuing any new Cursor task:

1. Read all recovery/SoT files and latest P40 plan/evidence
2. Determine: Current Phase, Last Architect Accepted Task, issued-but-unaccepted task, HEAD, `origin/main`, working tree, incomplete task state
3. Detect SoT conflicts, unfinished commits, local-only work, untracked artifacts, accidental future-task execution
4. **Repository truth overrides chat memory** on conflict
5. Do **not** issue a next task until unresolved repository state is reconciled

### Resume order (do not auto-execute)

1. Recover/review `TC-P40-T002`
2. Multi-agency public comparison (if incomplete)
3. Marketplace card/listing/search merchandising depth
4. Public label / DEMOFEED cleanup
5. Agency Portal operational UX depth
6. Admin operational UX / professional grid depth
7. Customer Dashboard depth
8. Reusable design-system primitives
9. Campaign/Promotion/Placement/Audience architecture planning

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-22 | Initial pause checkpoint — `PAUSED_BY_ARCHITECT` · T002 `NOT_STARTED` |
