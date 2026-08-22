# P39 — Commission / Settlement / Payout Foundation Plan

| Field | Value |
|-------|--------|
| Task-ID | `TC-P39-T001` |
| Phase | P39 — Multi-Agency Commercial Finance Foundation |
| Date | 2026-08-22 |
| Type | Architecture / boundary planning (**docs only**) |
| Prior gate | `TC-P38-T015` · **PASS WITH KNOWN LIMITATIONS** · **ACCEPTED** · Option A locked |
| HEAD baseline | `63f0981` |

## 1. Purpose

Design the **safest ownership direction** for future multi-agency financial capabilities after P38 commerce + governance landed:

```text
AgencyOffer
    ↓
Commission Context
    ↓
Settlement Context
    ↓
Payout Context
```

This document plans **boundaries and unknowns**. It does **not** authorize:

- commission formulas / calculation engines
- settlement jobs / execution
- payout APIs / money movement
- financial reports / ledgers as product UI

---

## 2. Locked inequalities (must survive P39)

| Rule | Meaning |
|------|---------|
| Commission ≠ Pricing | Commission does not own traveler price / quote amounts |
| Settlement ≠ Payment | Settlement does not replace Payment lifecycle or PSP rails |
| Payout ≠ Booking | Payout does not mutate Booking confirmation/cancellation state |
| Audit ≠ Financial Ledger | P38 governance audit remains operational — not accounting |
| Policy ≠ Commission | AgencyOffer policy Deny/Allow is not a commission decision |
| AgencyOffer ≠ Price ≠ Quote | Unchanged from P38 |
| Quote ≠ Booking ≠ Payment | Unchanged |

---

## 3. Commission Boundary Analysis

### 3.1 Definition (direction)

**Commission** — a commercial entitlement / obligation between platform and Agency arising from a sellable commercial path (typically tied to an AgencyOffer and later a successful commercial event such as Booking/Payment evidence).

Commission is **not**:

- the traveler-facing Price
- the Quote snapshot
- a Booking status
- a Payment charge

### 3.2 Ownership (proposed)

| Concern | Owner (proposed) | Notes |
|---------|------------------|-------|
| Commission domain / schema | **New Commercial Finance capability** (name TBD: e.g. `CommercialFinance` / `AgencyCommerceFinance`) | Do **not** dump into Pricing, Booking, Payment, or AgencyMarketplace without Architect confirmation |
| AgencyOffer reference | AgencyMarketplace provides Offer identity | Commission consumes Offer id — does not redefine Offer |
| Traveler amounts | Pricing (Price/Quote) | Commission may *read* commercial context references later — never own quote math |
| Booking commercial event | Booking | Commission may *observe* booking facts later — never own booking lifecycle |
| Payment evidence | Payment | Commission accrual triggers may reference payment success later — never own PSP |

**Working recommendation:** introduce a dedicated finance module/boundary rather than extending Pricing or Payment. Exact module name + schema remain **Architect-owned** in a later task.

### 3.3 Inputs (conceptual — not schema)

Directional inputs only (unknowns marked):

| Input | Status |
|-------|--------|
| AgencyProfile / Party | Known identity from AgencyMarketplace |
| AgencyOffer id | Known from P38 |
| Booking id / commercial source | Known pattern from P38 Offer-aware Booking |
| Quote / Price currency context | Known Pricing ownership — **not** commission-owned |
| Commission model / rate / tier | **UNKNOWN** (business) |
| Who pays whom (platform↔agency) | **UNKNOWN** (business agreements) |
| Accrual event (book / pay / travel / cancel) | **UNKNOWN** (business) |

### 3.4 Relationship map

```text
AgencyOffer ──(commercial path)──► Booking ──► Payment
        │                              │
        └────────── Commission Context ┘
                    (future entitlement)
```

Commission **references** Offer + commercial events; it does **not** replace Offer governance or Payment.

---

## 4. Settlement Boundary Analysis

### 4.1 Definition (direction)

**Settlement** — periodized reconciliation of commission (and related commercial obligations) into agency/platform balances ready for payout decisions.

Settlement is **not**:

- Payment capture/refund against traveler
- Bank settlement of the PSP (that remains Payment/provider concern)
- P38 governance audit history

### 4.2 Ownership (proposed)

| Concern | Owner (proposed) |
|---------|------------------|
| Settlement periods / statements | Commercial Finance boundary |
| Link to PaymentSucceeded / Refunded | Read-only references into Payment |
| Link to Booking cancelled/changed | Read-only references into Booking |
| Traveler charge lifecycle | Payment (unchanged) |

### 4.3 Timing concepts (directional)

| Concept | Direction | Status |
|---------|-----------|--------|
| Accrual | When commission becomes owed | UNKNOWN rule |
| Settlement window | Daily / weekly / monthly / trip-complete | UNKNOWN |
| Holdbacks | Cancel window / dispute hold | UNKNOWN |
| Adjustments | Partial cancel / no-show / goodwill | UNKNOWN |

### 4.4 Relationship with Payment lifecycle

```text
PaymentSucceeded  ≠  SettlementClosed
PaymentRefunded   ≠  CommissionReversed (may correlate later)
PSP bank settlement ≠ Agency Settlement statement
```

Settlement may **consume payment evidence**; it must never **become** the Payment module.

---

## 5. Payout Boundary Analysis

### 5.1 Definition (direction)

**Payout** — operational money movement (or instruction) to an Agency (or platform) based on settled balances.

Payout is **not**:

- Booking confirmation
- Traveler refund (Payment)
- Commission calculation itself

### 5.2 Responsibility (proposed)

| Concern | Owner (proposed) |
|---------|------------------|
| Payout instruction records | Commercial Finance boundary |
| Bank/PSP payout rails | External dependency + possibly Payment adapters later | Must not blur traveler PaymentTarget |
| Agency KYC / bank details | Party/Identity/B2B commercial profile direction — **UNKNOWN depth** |
| Operator approval of payout batches | Admin ops (future) — not Agency self-serve by default |

### 5.3 External dependencies

- Agency bank / wallet destination facts
- Regulatory constraints (Iran / UAE entity split already a P35 theme)
- Currency / FX ownership (already **not** Pricing — future FX service)
- Tax withholding / invoicing (**UNKNOWN**)

### 5.4 Operational requirements (directional)

- Explicit actor + approval trail (separate from P38 Offer governance audit)
- Idempotent payout instructions
- No silent mutation of Booking/Payment from payout success/failure

---

## 6. Domain Separation Checklist

| Assertion | Required outcome |
|-----------|------------------|
| Commission does not own pricing | PASS (locked) |
| Settlement does not replace payment | PASS (locked) |
| Payout does not modify booking state | PASS (locked) |
| AgencyMarketplace remains Offer/governance owner | PASS |
| Pricing remains amount authority | PASS |
| Payment remains money-execution authority for traveler rails | PASS |
| Governance Audit remains non-financial | PASS |

---

## 7. Business Unknowns (explicit)

These block formula/engine implementation until Architect + business resolve:

1. **Commission models** — flat %, tiered, per-product, per-channel, net vs gross
2. **Agency agreements** — contract artifacts, effective dates, exclusivity
3. **Payout schedules** — cadence, minimums, holds
4. **Tax requirements** — VAT/withholding per jurisdiction
5. **Currency handling** — offer currency vs settlement currency vs payout currency
6. **Entity model** — Iran vs UAE legal entities for acquiring vs agency payout
7. **Dispute / clawback** — who initiates, who approves, how Booking/Payment evidence binds
8. **Reporting consumers** — ops vs accounting; must not collapse into fake KPI UI

---

## 8. Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Putting commission math into Pricing | High | Keep Commission ≠ Pricing; separate module boundary |
| Treating PaymentSucceeded as Settlement | High | Explicit Settlement ≠ Payment inequality + tests later |
| Using P38 governance audit as ledger | High | Audit ≠ Financial Ledger remains hard rule |
| Implementing formulas before agreements | High | T001 docs-only; no engines until unknowns resolved |
| Agency self-payout without controls | Medium | Admin approval path required in future tasks |
| Currency/FX leakage into Pricing | Medium | FX remains non-Pricing ownership |

---

## 9. Suggested future task themes (not authorized here)

1. Commercial Finance module skeleton (contracts/names only)
2. Commission model vocabulary (still no formulas)
3. Settlement period concepts + statement shape
4. Payout instruction shape + approval
5. Access permissions for finance ops
6. Only then: calculation / jobs / reports under explicit Architect envelopes

---

## 10. Explicitly out of scope (T001)

- Commission formulas
- Settlement jobs
- Payout APIs
- Financial reports
- Money movement
- Schema migrations / production finance code

---

## 11. Deliverable status

| Deliverable | Status |
|-------------|--------|
| This plan | Created |
| SoT updates (PROJECT-STATE / ROADMAP / RECOVERY) | Required with T001 RESULT |
| Production financial code | **Forbidden** for T001 |
