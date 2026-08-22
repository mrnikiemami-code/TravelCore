# P39 — Commercial Finance Decisions Locked

| Field | Value |
|-------|--------|
| Task-ID | `TC-P39-T005` |
| Phase | P39 — Multi-Agency Commercial Finance Foundation |
| Date | 2026-08-22 |
| Type | Business decision lock (**docs only**) |
| Depends on | `TC-P39-T001`–`T004` ACCEPTED · User/Architect answers recorded |
| HEAD baseline | `cf3a88b` |
| Status | **DECISIONS_LOCKED** — formulas/tax rates/FX provider still deferred |

---

## 1. Purpose

Normalize Architect/User Commercial Finance answers into **canonical policy decisions**, map to Q1–Q38 intake, classify remaining unknowns, and produce an **implementation readiness matrix** — without implementing financial engines.

---

## 2. Normalized Architect/User decisions

| Domain | User answer (condensed) | Normalized token |
|--------|-------------------------|------------------|
| Commission model | 1-D,E | `AGENCY_SPECIFIC_WITH_OFFER_OVERRIDE` |
| Commission base | 2-B (paid amount) | `PAID_AMOUNT` |
| Entitlement trigger | 3-C (Payment succeeded) | `PAYMENT_SUCCEEDED` |
| Cancel/refund/clawback | 4-هر سه | `PRE_SETTLEMENT_CANCEL` · `POST_SETTLEMENT_REVERSAL` · `PROPORTIONAL_PARTIAL_REFUND` |
| Settlement cadence | 5-E (per agency) | `AGENCY_CONFIGURABLE_CADENCE` |
| Settlement approval | 6-بله | `ADMIN_APPROVAL_REQUIRED` |
| Negative balance | 7-B | `OFFSET_AGAINST_FUTURE_PAYABLES` |
| Multi-currency | 8-بله | `SOURCE_CURRENCY_OBLIGATION_WITH_SETTLEMENT_CONVERSION` |
| FX source | 9-configurable | `CONFIGURABLE_FX_AUTHORITY` |
| Tax / invoice | 10-بله | `MARKET_CONFIGURABLE_TAX_INVOICE_POLICY` |
| Payout mode | 11-B | `SEMI_AUTOMATED_ADMIN_APPROVED_PAYOUT` |
| Market model | 12-shared core + market policies | `SHARED_CORE_WITH_MARKET_SPECIFIC_POLICIES` |

---

## 3. Q1–Q38 classification table

| Q | Topic | Status | Locked / derived value |
|---|-------|--------|------------------------|
| Q1 | Commission model kinds | **LOCKED** (structure) | Agency-specific terms + Offer override; formula kinds (%/fixed/tier) per agreement — **no default %** |
| Q2 | Commission base | **LOCKED** | `PAID_AMOUNT` |
| Q3 | Vertical base mapping | **STILL_UNKNOWN** | No per-vertical exception list supplied |
| Q4 | Term ownership / precedence | **DERIVED_FROM_LOCKED** | Offer override → Agency agreement → (no platform default until defined) |
| Q5 | Entitlement trigger | **LOCKED** | `PAYMENT_SUCCEEDED` |
| Q6 | Holdback before Approved | **STILL_UNKNOWN** | Auto vs manual Pending→Approved not specified |
| Q7 | Cancel before payment | **DERIVED_FROM_LOCKED** | No obligation (trigger is PaymentSucceeded) |
| Q8 | Full refund pre-settlement | **LOCKED** | Cancel/remove obligation |
| Q9 | Partial refund | **LOCKED** | Proportional commission reduction |
| Q10 | Cancel after confirm pre-travel | **DERIVED_FROM_LOCKED** | Same as Q8/Q9 refund rules |
| Q11 | Post-settlement clawback | **LOCKED** | Reversal/clawback in later period |
| Q12 | Idempotency | **DERIVED_FROM_LOCKED** | Strict idempotent consumption per source event (architecture-safe default) |
| Q13 | Settlement cadence | **LOCKED** | Per-agency configurable |
| Q14 | Period cut-off rule | **STILL_UNKNOWN** | Approved-at vs event-at timestamp not specified |
| Q15 | Settlement approval | **LOCKED** | Admin approval required |
| Q16 | Minimum payout threshold | **STILL_UNKNOWN** | No minimum amount supplied |
| Q17 | Negative balance | **LOCKED** | Offset against future payables |
| Q18 | Closed-period correction | **DERIVED_FROM_LOCKED** | Reversed + explicit correction; no period reopen |
| Q19 | Obligation currency | **DERIVED_FROM_LOCKED** | Source / charge currency retained on obligation |
| Q20 | Settlement currency | **STILL_UNKNOWN** (partial) | May differ from obligation; per-market/agency specifics not supplied |
| Q21 | Settlement ≠ charge currency | **LOCKED** | Yes — conversion at settlement allowed |
| Q22 | FX authority | **LOCKED** (policy) | Configurable; **provider identity STILL_UNKNOWN** |
| Q23 | FX timestamp | **STILL_UNKNOWN** | Settlement close vs payout vs trigger not chosen |
| Q24 | Rounding / Toman display | **DEFERRED_NON_BLOCKING** | Money ADR: Toman = display only, not CurrencyCode |
| Q25 | Tax on commission | **MARKET_SPECIFIC_UNKNOWN** | Core configurable; exact rates deferred |
| Q26 | Withholding | **MARKET_SPECIFIC_UNKNOWN** | Per-market legal confirmation required |
| Q27 | VAT Iran/UAE | **MARKET_SPECIFIC_UNKNOWN** | Separate market rules deferred |
| Q28 | Invoice ownership | **STILL_UNKNOWN** | Configurable posture locked; artifact owner not chosen |
| Q29 | Invoice number storage | **STILL_UNKNOWN** | Statement-only vs invoice ref not chosen |
| Q30 | Payout execution mode | **LOCKED** | Semi-automated |
| Q31 | Payout approval matrix | **DERIVED_FROM_LOCKED** | Admin approval (aligns with settlement + semi-auto payout) |
| Q32 | Payout schedule | **STILL_UNKNOWN** | Immediate vs delay vs batch not specified |
| Q33 | Beneficiary KYC | **STILL_UNKNOWN** | Depth per market not supplied |
| Q34 | Payout currency | **DERIVED_FROM_LOCKED** | Expected = settlement currency unless market policy overrides |
| Q35 | Payout failure/retry | **STILL_UNKNOWN** | Manual vs automated retry not specified |
| Q36 | Global vs market rules | **LOCKED** | Shared core + Iran/UAE market policies |
| Q37 | Iran constraints | **MARKET_SPECIFIC_UNKNOWN** | Tax, rails, invoicing — legal/accounting input required |
| Q38 | UAE constraints | **MARKET_SPECIFIC_UNKNOWN** | Tax, rails, invoicing — legal/accounting input required |

**Summary counts:** LOCKED 14 · DERIVED 11 · STILL_UNKNOWN 11 · MARKET_SPECIFIC_UNKNOWN 5 · DEFERRED_NON_BLOCKING 1

---

## 4. Commission policy lock

### 4.1 Structure

- **Agency-specific** commission terms via Commission Agreement (per AgencyProfile)
- **Offer-level override** allowed on AgencyOffer commercial context
- **No platform-wide default percentage** until explicitly defined by Architect

### 4.2 Precedence (conceptual)

```text
Offer override (if present)
    ↓
Agency Commission Agreement
    ↓
Platform default policy (NOT DEFINED — do not invent)
```

### 4.3 Calculation inputs (locked)

| Input | Value |
|-------|-------|
| Base amount | **Paid amount** (Payment succeeded evidence) |
| Trigger event | **PaymentSucceeded** → Obligation **Created** |
| Formula values | **NOT LOCKED** — % / fixed / tier per agreement only |

### 4.4 Unresolved commission facts

- Per-vertical base exceptions (Q3)
- Holdback / auto-approve for Pending→Approved (Q6)
- Default formula when agreement silent

---

## 5. Refund / clawback policy lock

| Scenario | Behavior | History rule |
|----------|----------|--------------|
| Cancel before PaymentSucceeded | No obligation created | N/A |
| Full refund before settlement close | Obligation **Cancelled** / removed from eligible set | No rewrite of Payment evidence |
| Partial refund before settlement | **Proportional** commission reduction | Adjustment via explicit obligation change — not silent mutation |
| After settlement (Settled) | **Reversal/clawback** in later period | Reversed + correction obligation; immutable settled record |

**Idempotency:** strict idempotent consumption of PaymentSucceeded / refund correlation events.

**Unresolved:** exact proportional rounding when obligation currency ≠ settlement currency (Q23/Q24 interaction).

---

## 6. Settlement policy lock

| Rule | Value |
|------|-------|
| Cadence | **Per-agency configurable** |
| Close approval | **Admin approval required** |
| Negative net | **Offset against future payables** |
| Closed-period fix | **Reversed + later-period correction** — no silent reopen |

**Unresolved:** cut-off timestamp rule (Q14), minimum payout threshold (Q16).

---

## 7. Currency / FX policy lock

| Rule | Value |
|------|-------|
| Obligation currency | May retain **source/charge currency** |
| Settlement currency | May **differ** — conversion at settlement |
| FX authority | **Configurable** — provider/source **deferred** |
| IRR/Toman | **Toman = display unit only**; CurrencyCode remains authoritative (Money ADR) |

**Unresolved:** FX timestamp (Q23), exact settlement currency selection per agency/market (Q20), rounding at conversion boundary.

---

## 8. Tax / invoice market split

```text
Commercial Finance Core
    |
    +-- IranPolicy  (tax / invoice / withholding — CONFIGURABLE, rules UNKNOWN)
    |
    +-- UAEPolicy   (tax / invoice / withholding — CONFIGURABLE, rules UNKNOWN)
```

| Posture | Status |
|---------|--------|
| Core supports market-specific tax/invoice configuration | **LOCKED** |
| Exact Iran VAT/withholding/invoice rules | **MARKET_SPECIFIC_UNKNOWN** |
| Exact UAE VAT/withholding/invoice rules | **MARKET_SPECIFIC_UNKNOWN** |
| Commercial Obligation ≠ Invoice | **LOCKED** (unchanged) |

**Do not invent tax rates or invoice templates.**

---

## 9. Payout policy lock

| Rule | Value |
|------|-------|
| Mode | **Semi-automated** |
| System prepares | Settlement Record net + Payout Instruction draft |
| Human gate | **Admin approval** before any money movement |
| Bank/API rails | **Deferred** — external provider integration blocked |
| Booking impact | **None** — Payout ≠ Booking |

**Unresolved:** payout schedule (Q32), beneficiary KYC depth (Q33), failure/retry (Q35).

---

## 10. Market policy model (locked)

```text
SHARED_CORE_WITH_MARKET_SPECIFIC_POLICIES

CommercialFinance.Core
    ├── CommissionPolicy (structure locked; formulas per agreement)
    ├── ObligationLifecycle (P39-T003)
    ├── SettlementPolicy (cadence configurable per agency)
    ├── FxPolicy (configurable authority)
    ├── TaxInvoicePolicy (market slots)
    └── PayoutPolicy (semi-auto + admin approve)

MarketPolicies/
    ├── IranPolicy
    └── UAEPolicy
```

Per-agency override remains compatible with Q13/Q36 (agency cadence within market policy bounds).

---

## 11. Remaining unresolved facts

### Business rule gaps (non-market)

- Q3 vertical base mapping
- Q6 holdback / auto-approve matrix
- Q14 settlement cut-off rule
- Q16 minimum payout threshold
- Q20 settlement currency selection rules
- Q23 FX timestamp
- Q28–Q29 invoice artifact ownership
- Q32–Q35 payout schedule, KYC, retry

### Market / legal gaps

- Q25–Q27 Iran/UAE tax, VAT, withholding
- Q37 Iran market constraints (rails, compliance)
- Q38 UAE market constraints

### External provider gaps

- FX provider / rate feed identity
- Payout bank rails (Iran + UAE)
- Production PSP alignment (see P35 intake)

---

## 12. Implementation readiness matrix

| Capability | Classification | Notes |
|------------|----------------|-------|
| Commission contracts (Agreement, Rule Context) | **READY_FOR_SKELETON_ONLY** | No default % fields as SoT |
| Commercial Obligation persistence | **READY_FOR_SKELETON_ONLY** | Lifecycle enum + refs only |
| Commission calculation skeleton | **READY_FOR_SKELETON_ONLY** | Interface/hooks; **BLOCKED** on formula values |
| Commission calculation engine | **BLOCKED_ON_BUSINESS_RULE** | Q3, Q6 + agreement content |
| Settlement Period / Record contracts | **READY_FOR_SKELETON_ONLY** | Open/Closed + approval flags |
| Settlement calculation / close job | **BLOCKED_ON_BUSINESS_RULE** | Q14, Q16 cut-off/threshold |
| Payout Instruction persistence | **READY_FOR_SKELETON_ONLY** | Status + approval; no bank fields |
| Actual payout execution | **BLOCKED_ON_EXTERNAL_PROVIDER** | Bank rails deferred |
| Tax calculation | **BLOCKED_ON_MARKET_LEGAL_FACT** | Q25–Q27, Q37–Q38 |
| FX conversion execution | **BLOCKED_ON_EXTERNAL_PROVIDER** | Q22 provider + Q23 timestamp |
| Market policy modules (Iran/UAE slots) | **READY_FOR_SKELETON_ONLY** | Empty policy containers |
| Admin finance ops UI | **BLOCKED_ON_BUSINESS_RULE** | No fake KPIs; after skeleton envelope |

---

## 13. Recommended next authorized task

**`TC-P39-T006`** — Commercial Finance Contracts + Persistence Skeleton

Expected scope (when enveloped):

- Domain contracts for Agreement · Rule Context · Obligation · Settlement Period/Record · Payout Instruction
- Persistence skeleton / migrations for locked concepts only
- Market policy placeholder types (Iran/UAE)
- Read-only evidence port interfaces
- **No** commission formulas · **no** settlement jobs · **no** payout APIs · **no** FX/tax execution

---

## 14. Explicitly out of scope (T005)

- Backend/frontend production code
- Migrations (T005 is docs-only; T006 may add if enveloped)
- Commission percentages / tax rates / FX rates
- Settlement schedulers / payout processors
- Bank or accounting integrations
