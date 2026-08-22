# P39 — Commercial Obligation Lifecycle Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P39-T003` |
| Phase | P39 — Multi-Agency Commercial Finance Foundation |
| Date | 2026-08-22 |
| Type | Domain / lifecycle design (**docs only**) |
| Depends on | `TC-P39-T001` ACCEPTED · `TC-P39-T002` ACCEPTED |
| HEAD baseline | `7d70ab1` |

## 1. Purpose

Define the **candidate lifecycle**, **evidence boundaries**, and **relationships** for **Commercial Obligation** — the bridge concept between commerce events and future settlement — without implementing calculation, settlement jobs, payout processing, or ledger code.

```text
Evidence (read-only)
    ↓
Commission (future) → Commercial Obligation lifecycle
    ↓
Settlement Period (future) → Settlement Record
    ↓
Payout Instruction (future)
```

---

## 2. Locked inequalities (carry forward)

| Rule | Status |
|------|--------|
| Commission ≠ Pricing | Locked |
| Settlement ≠ Payment | Locked |
| Payout ≠ Booking | Locked |
| Commercial Obligation ≠ Invoice | Locked |
| Audit ≠ Financial Ledger | Locked |
| AgencyOffer ≠ Financial Transaction | Locked |

---

## 3. Commercial Obligation — definition (recap)

A **Commercial Obligation** is a single owed/receivable commercial duty arising from a commercial event path (typically AgencyOffer + Booking + Payment evidence).

**Holds (directional):** obligation kind · parties (platform / agency) · currency intent · source references (Offer / Booking / Payment ids) · lifecycle state · evidence snapshot references  
**Does not hold:** traveler Price authority · Booking confirmation authority · PaymentAttempt / PSP payloads · invoice line items · accounting journal entries

---

## 4. Lifecycle — candidate states

### 4.1 State machine (vocabulary only)

```text
                    ┌─────────────┐
                    │   Created   │  obligation record drafted from evidence path
                    └──────┬──────┘
                           │
                           ▼
                    ┌─────────────┐
                    │   Pending   │  awaiting review / policy / completeness
                    └──────┬──────┘
                           │
              ┌────────────┼────────────┐
              ▼            │            ▼
       ┌─────────────┐     │     ┌─────────────┐
       │  Cancelled  │     │     │  Approved   │  eligible for settlement grouping
       └─────────────┘     │     └──────┬──────┘
                           │            │
                           │            ▼
                           │     ┌─────────────┐
                           │     │   Settled   │  included in a closed Settlement Record
                           │     └──────┬──────┘
                           │            │
                           │            ▼
                           │     ┌─────────────┐
                           └────►│  Reversed   │  post-settlement correction / clawback path
                                 └─────────────┘
```

### 4.2 State descriptions

| State | Intent | Entry triggers (future) | Exit / notes |
|-------|--------|-------------------------|--------------|
| **Created** | Obligation entity exists with source references attached | Commission evaluation produces a duty record | Moves to Pending when validation queue opens |
| **Pending** | Awaiting completeness, policy, or ops review before approval | Missing evidence · dispute · holdback window | → Approved · Cancelled |
| **Approved** | Commercially accepted as owed/receivable; eligible for period grouping | Ops/finance approval · auto-approve rules (TBD) | → Settled when assigned to closed Settlement Record · → Cancelled if voided pre-settlement |
| **Settled** | Obligation net position captured in a Settlement Record for a period | Settlement close includes this obligation | → Reversed only via explicit reversal obligation (not Booking undo) |
| **Cancelled** | Obligation voided before settlement close | Booking cancel + refund path · manual void · evidence invalidation | Terminal |
| **Reversed** | Corrective state after settlement (clawback / adjustment obligation) | Dispute resolution · post-settlement correction | May spawn linked reversal obligation; terminal for original |

### 4.3 Lifecycle rules (design locks)

1. **State transitions are Commercial Finance owned** — Booking/Payment modules do not write obligation states.
2. **Evidence changes do not silently mutate Settled obligations** — corrections use Reversed + new obligation or adjustment record (mechanism TBD).
3. **Cancelled is pre-settlement only** — post-settlement corrections use Reversed path.
4. **Approved ≠ Paid** — approval is commercial acceptance; payout is a separate future instruction.
5. **Settled ≠ PaymentSucceeded** — settlement groups obligations; traveler payment remains in Payment domain.

### 4.4 Not in scope (T003)

- Persistence schema / EF entities
- State transition APIs or background workers
- Automatic transition rules from Booking/Payment webhooks
- Idempotency keys / outbox implementation

---

## 5. Evidence boundary

Commercial Finance **may consume** evidence from source domains. It **must not become owner** of those domains.

### 5.1 Evidence sources

| Source domain | Evidence examples | Consumption pattern | Ownership rule |
|---------------|-------------------|---------------------|----------------|
| **AgencyOffer** | Offer id · publication status · agency id · product refs | Correlates obligation to sellable commercial context | Marketplace owns offer lifecycle; Finance reads snapshots |
| **Booking** | Booking id · Confirmed/Cancelled · dates · traveler refs | Confirms commercial event occurred / was voided | Booking owns reservation state; Finance never confirms bookings |
| **Payment** | Payment id · Succeeded/Failed/Refunded · amount/currency evidence | Confirms traveler money evidence (not agency payout) | Payment owns PSP rails; Finance does not mutate PaymentAttempt |
| **Cancellation / Refund** | Cancel reason · refund correlation ids · partial/full | Drives Cancelled / Reversed eligibility | Refund execution stays Payment; Finance records commercial consequence |

### 5.2 Evidence flow (read-only)

```text
AgencyOffer ──read──► Offer snapshot ref on Obligation
Booking     ──read──► Booking status ref + event timestamps
Payment     ──read──► Payment evidence ref (traveler-side)
Cancel/Refund ─read──► Void / reversal eligibility signals

Commercial Finance WRITES: Obligation states + future Settlement/Payout records only
```

### 5.3 Evidence integrity rules

1. **Reference, don't embed authority** — obligation stores ids + snapshot hashes/timestamps, not live Booking/Payment aggregates.
2. **Stale evidence handling** — Pending state holds obligations when upstream evidence is incomplete or conflicting.
3. **P38 governance audit is not financial evidence** — offer moderation audit ≠ obligation ledger.
4. **Quote/Price snapshots are not obligation amounts** — Pricing remains traveler-facing; commission amounts are future Commission output.

---

## 6. Commission relationship

### 6.1 Directional rule (locked)

**Commission produces Commercial Obligations.**

```text
Commission Agreement + Rule Context + Evidence path
        ↓
Commission evaluation (future engine — NOT T003)
        ↓
Commercial Obligation (Created)
```

### 6.2 What Commission owns (future)

- Entitlement decision: *whether* an obligation exists
- Obligation kind (platform receivable vs agency payable — taxonomy TBD)
- Party references and currency intent for the commercial duty

### 6.3 What Commission does NOT own (locked)

| Excluded | Owner |
|----------|-------|
| Traveler Price / Quote amounts | Pricing |
| % formulas / tier math / tax rules | Future Commission engine (not authorized) |
| Booking confirmation | Booking |
| Payment capture / refund | Payment |
| Settlement period close | Future Settlement (not authorized) |
| Payout execution | Future Payout (not authorized) |

### 6.4 Obligation ≠ Invoice

| Commercial Obligation | Invoice (out of scope) |
|-----------------------|------------------------|
| Internal commercial duty between platform and agency | External billing document to customer or agency |
| Driven by commerce evidence + commission rules | Driven by accounting / tax presentation rules |
| Lifecycle: Created → … → Settled | Not modeled in T003 |

---

## 7. Settlement relationship

### 7.1 Directional rule (locked)

**Settlement groups Approved obligations into operational periods.**

```text
Settlement Period (Open)
        ↓
Approved Commercial Obligations (eligible)
        ↓
Settlement Record (aggregated statement)
        ↓
Settlement Period (Closed)
        ↓
Obligations → Settled state
```

### 7.2 Settlement Period (recap)

- Time-bounded window (weekly / monthly — schedule TBD)
- Scope: per agency or platform batch (TBD)
- Status vocabulary: Open / Closed (extend later)

### 7.3 Settlement Record (recap)

- Aggregates included obligation references
- Produces net position intent for future Payout Instruction
- **Does not** replace Payment settlement batches or PSP reconciliation

### 7.4 Not in scope (T003)

- Settlement jobs / schedulers
- Reconciliation engine against bank statements
- Automatic period open/close
- Multi-currency netting rules

---

## 8. Ownership assessment

| Concern | Owner | Notes |
|---------|-------|-------|
| Obligation lifecycle states | Commercial Finance (future) | T003 vocabulary only |
| Evidence snapshots | Source domains remain SoT | Finance stores references |
| Commission entitlement | Commercial Finance (future) | Produces obligations; no math in T003 |
| Settlement grouping | Commercial Finance (future) | Groups obligations; ≠ Payment |
| Traveler money | Payment | Unchanged |
| Reservation truth | Booking | Unchanged |
| Sellable offer truth | AgencyMarketplace | Unchanged |
| Traveler price | Pricing | Unchanged |
| Ops governance audit | AgencyMarketplace Admin | Audit ≠ Ledger |

---

## 9. Risks

| Risk | Mitigation |
|------|------------|
| Booking cancel auto-writes obligation Cancelled | Explicit rule: Finance transitions only via subscribed handlers (future); no synchronous Booking coupling in design |
| PaymentSucceeded treated as Settled | Locked: Settled requires Settlement Record close |
| Invoice conflation | Locked: Obligation ≠ Invoice |
| Using governance audit as financial proof | Audit ≠ Financial Ledger |
| Premature state machine code before business rules | T003 docs-only; Pending/Approved criteria remain TBD |
| Reversal without linked obligation | Reversed spawns explicit correction obligation — no silent ledger edits |

---

## 10. Business unknowns (still blocking implementation)

1. Auto-approve vs manual approve matrix for Pending → Approved
2. Partial cancellations and split obligations
3. Multi-currency obligation vs settlement currency
4. Holdback windows before Approved
5. Clawback timing relative to Settled state
6. Idempotency when same Booking+Payment emits duplicate commission events
7. Tax/withholding effect on obligation kind (not formula)

---

## 11. Explicitly out of scope (T003)

- Commission formulas / calculation engine
- Money calculations / amount fields as authoritative
- Financial ledger / accounting integration
- Settlement jobs / reconciliation engine
- Payout processing / bank integration
- Production financial code / migrations / APIs
- UI KPIs / revenue / commission dashboards

---

## 12. Suggested next themes (not authorized)

1. Obligation kind taxonomy + party role refinement
2. Transition guard rules (who may approve / cancel)
3. Empty module skeleton + read-only evidence port interfaces
4. Business-resolution tasks for unknowns §10
5. Only then engines under explicit Architect envelopes
