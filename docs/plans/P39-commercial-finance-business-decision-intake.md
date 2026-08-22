# P39 — Commercial Finance Business Decision Intake

| Field | Value |
|-------|--------|
| Task-ID | `TC-P39-T004` |
| Phase | P39 — Multi-Agency Commercial Finance Foundation |
| Date | 2026-08-22 |
| Type | Business decision intake (**docs only**) |
| Depends on | `TC-P39-T001`–`T003` ACCEPTED |
| HEAD baseline | `7cc7151` |
| Status | **AWAITING_BUSINESS_ANSWERS** — no monetary values invented |

---

## 1. Purpose

Convert accepted Commercial Finance boundaries and Commercial Obligation lifecycle into a **precise, answerable questionnaire** for Commission, Settlement, Reversal/Clawback, Currency, Tax/Invoice, and Payout — so future implementation does not guess business rules.

**This document asks questions. It does not choose answers.**

---

## 2. Locked architecture facts (non-negotiable)

| Fact | Source |
|------|--------|
| Commission ≠ Pricing | P39-T001 |
| Settlement ≠ Payment | P39-T001 |
| Payout ≠ Booking | P39-T001 |
| Commercial Obligation ≠ Invoice | P39-T003 |
| Audit ≠ Financial Ledger | P38/P39 |
| PaymentSucceeded ≠ SettlementClosed | P39-T003 |
| Approved ≠ Paid | P39-T003 |
| Commercial Finance consumes evidence; does not own Booking/Payment/Offer | P39-T002/T003 |
| Obligation lifecycle: Created → Pending → Approved → Settled; Cancelled / Reversed | P39-T003 |
| Commission produces obligations; Settlement groups obligations | P39-T003 |
| Corrections use Reversed + explicit correction obligation — no history rewrite | P39-T003 |
| TravelCore Money ADR: one authoritative currency per money value; no twin SoR | P12 |
| Multi-currency platform (IRR · AED · USD among traveler contexts) | P35 intake |

---

## 3. Remaining business decisions — summary

| Domain | Decision count | Blocks |
|--------|----------------|--------|
| Commission model + base | Q1–Q4 | Commission engine, obligation amount fields |
| Commission trigger | Q5–Q6 | Event handlers, auto Created transition |
| Cancellation / refund / clawback | Q7–Q12 | Cancelled/Reversed rules, partial obligations |
| Settlement model | Q13–Q18 | Period scheduler, close rules, negative balance |
| Currency / FX | Q19–Q24 | Multi-currency obligation, settlement statement |
| Tax / invoice boundary | Q25–Q29 | Withholding fields, invoice workflow (not ledger) |
| Payout model | Q30–Q35 | Payout Instruction approval, bank timing |
| Iran / UAE market split | Q36–Q38 | Market-specific policy modules |

---

## 4. Iran / UAE considerations (questions only)

| Topic | Why split may be required |
|-------|---------------------------|
| Tax / withholding | Iran VAT/withholding rules vs UAE VAT treatment may differ |
| Invoice expectations | Agency/platform invoice norms differ by jurisdiction |
| Settlement currency | IRR/Toman display vs AED settlement preference (see P35) |
| Payout rails | Iranian bank rails vs UAE/international beneficiary accounts |
| Regulatory reporting | Unknown compliance obligations per market — must be supplied |
| Refund timing | Traveler refund rails (P35) may affect clawback timing differently |

**Do not assume one model applies to both markets.**

---

## 5. Exact numbered questionnaire

Answer in one Architect/User message. Use option letters where provided; add notes where needed.

### A. Commission model

**Q1 — Commission calculation model (select all that may apply):**
- A) Percentage of a defined base
- B) Fixed amount per commercial event
- C) Tiered (volume/period thresholds)
- D) Agency-specific agreement terms
- E) Product/Offer override on top of agreement
- F) Mixed model (specify combination rules)
- G) Other (describe)

**Q2 — Commission base amount (choose primary; note exceptions):**
- A) Traveler gross (pre-discount)
- B) Traveler net (after discounts)
- C) Quote snapshot amount
- D) Booking confirmed amount
- E) Payment succeeded amount (paid amount)
- F) Other explicit base (describe)

**Q3 — If base differs by product vertical (Tour/Hotel/Flight/Package), list mapping or defer rule.**

**Q4 — Who defines commission terms for a given Agency?**
- A) Platform standard agreement only
- B) Negotiated per AgencyProfile
- C) Per AgencyOffer at publish time
- D) Hybrid (describe precedence)

### B. Commission trigger

**Q5 — When is a Commercial Obligation Created (entitlement trigger)?**
- A) Booking created
- B) Booking confirmed
- C) Payment succeeded
- D) Travel completed / service delivered
- E) Other explicit event (describe)
- F) Different trigger per vertical (describe)

**Q6 — Is there a holdback window before Pending → Approved?**
- A) No — auto-approve on trigger
- B) Yes — fixed days after trigger (supply N)
- C) Yes — until travel completion
- D) Manual ops approval always
- E) Hybrid (describe)

### C. Cancellation / refund / clawback

**Q7 — Booking cancelled before Payment succeeded:**
- A) No obligation ever created
- B) Obligation Created then Cancelled if already Created
- C) Other (describe)

**Q8 — Full refund after Payment succeeded (pre-settlement):**
- A) Obligation Cancelled
- B) Obligation never Approved
- C) Obligation Approved then Cancelled
- D) Other (describe)

**Q9 — Partial refund after Payment succeeded:**
- A) Split into reduced obligation + Cancelled remainder
- B) Single obligation adjusted (requires Reversed path)
- C) Not supported — full refund only
- D) Other (describe)

**Q10 — Cancellation after Booking confirmed but before travel:**
- A) Same as Q8/Q9
- B) Different rule (describe)

**Q11 — Post-settlement correction (clawback after Settled):**
- A) Reversed on original + new negative correction obligation
- B) Deduct from next Settlement Period only
- C) Manual finance case — no automation
- D) Other (describe)

**Q12 — Idempotency: duplicate PaymentSucceeded / BookingConfirmed events:**
- A) Strict idempotency key per source event
- B) Allow duplicate detection window (describe)
- C) Undecided — recommend safe default: idempotent consumption

### D. Settlement model

**Q13 — Settlement cadence:**
- A) Weekly
- B) Bi-weekly
- C) Monthly
- D) Per-agency configurable
- E) Manual/on-demand only
- F) Other (describe)

**Q14 — Cut-off rule (what events belong to period N):**
- A) Approved before period end timestamp
- B) Trigger event timestamp within period
- C) Other (describe)

**Q15 — Settlement approval:**
- A) Auto-close when period ends
- B) Finance ops must approve close
- C) Agency must acknowledge statement
- D) Hybrid (describe)

**Q16 — Minimum payout threshold:**
- A) No minimum
- B) Fixed minimum (supply amount + currency per market)
- C) Carry forward until threshold met
- D) Other (describe)

**Q17 — Negative net balance (agency owes platform):**
- A) Carry forward to next period
- B) Immediate collection process (out of TravelCore scope — describe handoff)
- C) Offset against future commissions only
- D) Other (describe)

**Q18 — Correction to a closed period:**
- A) Never reopen — use Reversed + next-period adjustment
- B) Reopen with audit trail (who approves?)
- C) Manual exception only
- D) Other (describe)

### E. Currency / FX

**Q19 — Commission obligation currency:**
- A) Same as traveler charge currency
- B) Agency contract currency
- C) Platform reporting currency
- D) Per-agreement (describe)

**Q20 — Settlement currency:**
- A) Same as commission obligation currency
- B) Fixed per market (supply IRR vs AED vs USD)
- C) Agency-selectable from allowed list
- D) Other (describe)

**Q21 — May settlement currency differ from charge currency?**
- A) Yes — FX conversion required
- B) No — must match
- C) Market-specific (Iran vs UAE)

**Q22 — FX authority / source (if conversion required):**
- A) Central bank rate
- B) Platform-configured rate table
- C) PSP settlement rate
- D) Agreement-locked rate
- E) Undecided — blocks FX fields

**Q23 — FX timestamp:**
- A) Trigger event time
- B) Settlement period close time
- C) Payout instruction time
- D) Other (describe)

**Q24 — Rounding policy + IRR/Toman display:**
- A) Follow Money ADR minor units only
- B) Display Toman; store IRR (describe conversion)
- C) Market-specific (Iran vs UAE)
- D) Undecided

### F. Tax / invoice boundary

**Q25 — Tax on commission:**
- A) None modeled in TravelCore
- B) Rate supplied per market/agency (supply source)
- C) Calculated externally — TravelCore stores reference only
- D) Undecided

**Q26 — Withholding on agency payout:**
- A) Not applicable
- B) Applicable — rules per market (describe source)
- C) Reference only — execution external
- D) Undecided

**Q27 — VAT treatment (Iran vs UAE):**
- A) Separate rules per market (describe or defer)
- B) Not in scope for TravelCore
- C) Undecided

**Q28 — Invoice artifact ownership:**
- A) Platform issues invoice to agency
- B) Agency self-invoices platform
- C) No invoice — statement only (Obligation ≠ Invoice)
- D) External accounting system
- E) Undecided

**Q29 — Does TravelCore need to store invoice numbers, or only Settlement Record references?**
- A) Statement references only
- B) Invoice number + external link
- C) Full invoice generation (likely out of scope — confirm)
- D) Undecided

### G. Payout model

**Q30 — Payout execution mode:**
- A) Manual export + bank upload
- B) Semi-automated with ops approval
- C) Fully automated API
- D) Phased: manual first, automate later

**Q31 — Payout approval matrix:**
- A) Finance admin only
- B) Dual approval over threshold (supply threshold)
- C) Agency initiates; platform approves
- D) Other (describe)

**Q32 — Payout schedule relative to settlement close:**
- A) Immediate on close
- B) Fixed delay (supply days)
- C) Batch on calendar day
- D) Manual trigger

**Q33 — Beneficiary identity / KYC depth:**
- A) Bank account on AgencyProfile sufficient
- B) Enhanced KYC before first payout
- C) Per-market requirements (Iran vs UAE)
- D) Undecided

**Q34 — Payout currency:**
- A) Same as settlement currency
- B) Agency-selectable
- C) Market-fixed
- D) Undecided

**Q35 — Payout failure / retry:**
- A) Manual retry only
- B) Automated retry N times
- C) Return to Approved settlement balance
- D) Undecided

### H. Iran / UAE market split

**Q36 — Are commission/settlement/payout rules:**
- A) Unified globally with minor tweaks
- B) Separate policy profiles per market (Iran / UAE)
- C) Per-agency override allowed
- D) Undecided

**Q37 — For Iran market specifically, list any known constraints (tax, currency display, payout rails, invoicing) — or mark UNKNOWN.**

**Q38 — For UAE market specifically, list any known constraints — or mark UNKNOWN.**

---

## 6. What each unanswered question blocks

| Questions | Blocks implementation of |
|-----------|-------------------------|
| Q1–Q4 | Commission Rule Context schema, formula placeholders, obligation amount semantics |
| Q5–Q6 | Event subscription design, Pending→Approved automation |
| Q7–Q12 | Cancellation handlers, Reversed obligation rules, idempotency keys |
| Q13–Q18 | Settlement Period scheduler, close API, negative balance handling |
| Q19–Q24 | Multi-currency obligation/settlement fields, FX snapshot storage |
| Q25–Q29 | Tax/withholding metadata, invoice boundary docs vs external systems |
| Q30–Q35 | Payout Instruction workflow, approval permissions, retry policy |
| Q36–Q38 | Market-specific policy modules, Iran/UAE configuration split |

---

## 7. Safe structural defaults (architecture-only — no monetary values)

These may be implemented **before** business answers if Architect authorizes skeleton work:

| Default | Rationale |
|---------|-----------|
| Immutable financial evidence snapshots | Source domains remain SoT |
| Explicit obligation lifecycle states | P39-T003 locked |
| Idempotent event consumption | Prevents duplicate obligations (Q12 recommendation) |
| Correction via Reversed + new obligation | No ledger history rewrite |
| Logical references to Offer/Booking/Payment ids | Evidence boundary locked |
| Settlement Period Open/Closed vocabulary | No schedule assumed |
| Payout Instruction as separate artifact from Payment | Payout ≠ Booking |
| No fake KPI/revenue UI | FE ≠ SoT |
| Admin-only finance ops surfaces | No agency self-payout without approval path |

**Not safe without business answers:** any % rate, fixed fee, tax rate, settlement calendar date, FX rate source, minimum threshold amount.

---

## 8. Safe work that can continue without business answers

1. Empty Commercial Finance module skeleton (namespaces, boundaries) — if enveloped
2. Read-only evidence port interfaces (Offer/Booking/Payment ids)
3. Obligation state enum + transition guards (**without** auto-transition rules)
4. Permission placeholders for finance ops (no payout execute)
5. Additional docs: module naming, access matrix drafts
6. Business answer recording task after questionnaire filled

---

## 9. Recommended next authorized task (not executed here)

After Architect/User answers Q1–Q38:

1. **`TC-P39-T005`** (expected) — Record Business Decisions + resolve BLOCKED vs DEFERRED classification
2. Then skeleton/contracts under explicit envelope — still no formulas until decisions locked
3. Commission engine / settlement jobs — only after decision record + Architect authorization

---

## 10. Explicitly out of scope (T004)

- Financial calculations or example amounts
- Persistence / migrations / APIs
- Settlement jobs / payout processing
- Bank integration
- Accounting entries / ledger
- Tax rate assumptions
- Fake commission percentages or settlement schedules
