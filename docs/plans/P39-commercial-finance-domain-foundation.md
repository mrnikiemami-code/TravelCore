# P39 — Commercial Finance Domain Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P39-T002` |
| Phase | P39 — Multi-Agency Commercial Finance Foundation |
| Date | 2026-08-22 |
| Type | Domain / architecture foundation (**docs only**) |
| Depends on | `TC-P39-T001` ACCEPTED · `docs/plans/P39-commission-settlement-foundation-plan.md` |
| HEAD baseline | `05e92a9` |

## 1. Purpose

Define the **minimum domain vocabulary** for future Commercial Finance without implementing engines, jobs, bank rails, or reports.

```text
Commission Agreement
    ↓
Commission Rule Context
    ↓
Commercial Obligation
    ↓
Settlement Period → Settlement Record
    ↓
Payout Instruction
```

---

## 2. Locked inequalities (carry forward)

| Rule | Status |
|------|--------|
| Commission ≠ Pricing | Locked |
| Settlement ≠ Payment | Locked |
| Payout ≠ Booking | Locked |
| Audit ≠ Financial Ledger | Locked |
| AgencyOffer ≠ Financial Transaction | Locked |
| Policy ≠ Commission | Locked |

---

## 3. Candidate domain concepts

### 3.1 Commission Agreement

**Intent:** lasting commercial relationship terms between platform and an Agency (Party / AgencyProfile), scoped in time.

**Holds (directional):** party references · effective window · status · references to rule contexts  
**Does not hold:** traveler Price · Quote amounts · Booking status · Payment PSP payloads

**Status:** vocabulary only — agreement document storage / legal workflow **UNKNOWN**

### 3.2 Commission Rule Context

**Intent:** structured context describing *which* rules apply for an agreement / channel / product family — **not** the formula engine.

**Holds (directional):** agreement reference · channel/scope tags · rule-kind placeholders  
**Does not hold:** executable % formulas · money fields as SoT for traveler

**Status:** placeholders only until business models known

### 3.3 Commercial Obligation

**Intent:** a single owed/receivable commercial duty arising from a commercial event (e.g. Offer+Booking+Payment evidence path).

**Holds (directional):** obligation kind · parties · currency intent · source references (Offer/Booking/Payment ids) · lifecycle state vocabulary  
**Does not hold:** PaymentAttempt details · Booking confirmation authority

**Note:** Obligation is the bridge concept between commerce events and settlement — still **no calculation** in T002.

### 3.4 Settlement Period

**Intent:** a closed or open time window used to group obligations for reconciliation.

**Holds (directional):** period bounds · status (Open/Closed/…) · agency or platform scope  
**Does not hold:** PSP settlement batches · traveler refund rails

### 3.5 Settlement Record

**Intent:** statement-like aggregation of obligations for a period (balances / nets as future facts).

**Holds (directional):** period reference · included obligation references · statement status  
**Does not hold:** live Payment status machine · bank confirmation as PaymentSucceeded

### 3.6 Payout Instruction

**Intent:** an approved (or draft) instruction to move settled net to an Agency destination.

**Holds (directional):** settlement record reference · destination reference · instruction status · actor/approval placeholders  
**Does not hold:** Booking mutations · traveler refund semantics

---

## 4. Ownership confirmation

| Domain | Owns | Does not own |
|--------|------|--------------|
| **Commercial Finance** (future module) | Agreement · Rule Context · Obligation · Settlement Period/Record · Payout Instruction | Traveler price · Booking lifecycle · Traveler Payment rails · Offer governance |
| **AgencyMarketplace** | AgencyOffer · governance · operational audit | Commission math · settlement statements |
| **Pricing** | Price / Quote traveler amounts | Commission entitlement |
| **Booking** | Reservation lifecycle | Payout / settlement close |
| **Payment** | Traveler money evidence / attempts / refunds | Agency payout batches as PaymentSucceeded |

---

## 5. Future event inputs (identify only)

| Input family | Examples | Processing in T002 |
|--------------|----------|--------------------|
| AgencyOffer events | Submitted / Approved / Published / Suspended / Retired | **Not implemented** — reference only |
| Booking events | Created / Confirmed / Cancelled | **Not implemented** |
| Payment evidence | Succeeded / Failed / Refunded | **Not implemented** |
| Cancellation / refund events | Booking cancel + Payment refund correlation | **Not implemented** |

Commercial Finance may later **subscribe/read** these as evidence. It must not become the writer of Booking/Payment state.

---

## 6. Business unknowns (still blocking engines)

1. Commission models (flat / tier / net-gross / channel-specific)
2. Agency contract artifacts and effective dating
3. Tax / withholding handling
4. FX policy across offer vs settlement vs payout currency
5. Settlement schedules and holdbacks
6. Payout approval matrix and destination KYC depth
7. Clawback / dispute binding to Booking/Payment evidence
8. Reporting consumers (ops vs accounting) without fake KPI UI

---

## 7. Risks

| Risk | Mitigation |
|------|------------|
| Turning vocabulary into premature schema/migrations | Docs-only T002; no EF/modules yet |
| Embedding formulas inside Rule Context | Rule Context = context only until Architect authorizes engines |
| Collapsing PaymentSucceeded into Settlement Record | Explicit Settlement ≠ Payment |
| Using P38 Offer audit as financial ledger | Audit ≠ Financial Ledger |
| Agency self-serve payout without approvals | Payout Instruction requires future Admin approval path |

---

## 8. Explicitly out of scope (T002)

- Commission calculation engine
- Settlement jobs
- Payout APIs
- Financial reports
- Bank integrations
- Production financial code / migrations

---

## 9. Suggested next themes (not authorized)

1. Module/skeleton naming + empty contracts (still no math)
2. Obligation state vocabulary refinement
3. Access permissions for finance ops
4. Business-resolution tasks for unknowns
5. Only then engines / jobs under explicit envelopes
