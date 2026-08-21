# P33 — Tour Commerce Implementation Roadmap

| Field | Value |
|-------|--------|
| Document | `docs/plans/P33-tour-commerce-implementation-roadmap.md` |
| Task-ID | `TC-P33-T004` |
| Phase | P33 — Commercial Product Readiness |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Nature | Implementation planning only |
| Inputs | `P33-tour-first-commerce-slice.md` · `P33-tour-commerce-data-contracts.md` |
| Forbidden here | FE/BE/DB/migrations/seed · fake departures/prices · payment provider integration |

---

## 1. Roadmap summary

**Smallest path to first real Tour sell journey** (honest, architecture-preserving):

```text
I0  Architect gates + honesty locks
I1  Data readiness — one Published TourDeparture + one Price (real modules)
I2  Public composition — FE wires existing APIs (no new engines)
I3  Booking initiation UX — Pending + access token
I4  Payment boundary decision — stop OR labeled sandbox
I5  Evidence GATE — screenshots + API notes
```

Engines already exist. This roadmap is **data + composition + boundary decision**, not a greenfield Booking/Pricing rewrite.

---

## 2. DEMOFEED commercial scenario (proposed)

| Item | Proposal |
|------|----------|
| Product | One existing DEMOFEED TourProduct (e.g. Tehran demo tour) |
| Departure | One **Published** TourDeparture for that product (real Tour admin/API path) |
| Price | One Pricing `Price` with `TargetType=TourDeparture` |
| Honesty | If departure/price/provider missing → empty/unavailable UI — never invent |
| Removability | DemoFeed remains removable; scenario must not require permanent DemoFeed module |

**Minimum data required**

1. `TourProductId` (Public, published catalog)
2. `TourDepartureId` (Status = Published; schedule + capacity facts)
3. `Price` row targeting that Departure (authoritative currency + components/occupancy as Pricing requires)
4. Optional: labeled Payment sandbox config — only if Architect chooses Option B below

---

## 3. TourDeparture lifecycle (required states)

| State | Public visibility | Sell path |
|-------|-------------------|-----------|
| Draft | Hidden | Not selectable |
| **Published** | Listed via public published query | Candidate for initiate (still ≠ bookable until Booking succeeds) |
| Closed / Cancelled | Hidden / null by id | Not selectable |

**MVP rule:** only **Published** departures appear on Public detail. Booking module enforces capacity/publish checks at initiation.

Admin creates/publishes via existing `/api/tour/departures` — no new lifecycle engine in this roadmap.

---

## 4. Pricing readiness (honesty representation)

| Case | Public behavior |
|------|-----------------|
| Summary 200 | Show money from Pricing DTO only |
| Summary 404 / null | “قیمت در دسترس نیست” / equivalent — CTA initiate disabled or warns |
| Multiple occupancy | Show only what Pricing returns — no FE math |
| Currency | Display Price currency; no silent FX |

Price availability is **Pricing SoT**, never FE state.

Quote is created **inside Booking initiation** (existing) — Public FE must not mint quotes.

---

## 5. Booking initiation connection

```text
FE: selected TourDepartureId + contact + passengers
  → POST /api/booking/public/initiations
  → Pending + AccessToken (+ Monetary/Quote snapshot)
  → private booking read pages with X-TravelCore-Booking-Access-Token
```

Dependencies: published Departure + Pricing able to issue Quote for that Departure.

Forbidden: initiate with hardcoded Guids not shown from public published list; fake Confirmed.

---

## 6. Payment boundary options (evaluate)

| Option | Meaning | When to use |
|--------|---------|-------------|
| **A — No provider (recommended first)** | After Pending, UI stops with honest “پرداخت هنوز فعال نیست” | Fastest honest MVP; proves catalog→booking without money theater |
| **B — Labeled sandbox** | Architect-approved sandbox adapter; UI must label non-production | Only after A evidence + Architect file |
| **C — Provider abstraction only** | Docs/config hooks, no traveler-facing success | Prep work; not a sell demo by itself |

**Recommendation:** Implement **I1–I3 + Option A** first. Defer B/C to later Architect tasks. Do **not** fake payment success.

---

## 7. Implementation phases & task breakdown (proposals)

Names are **not executable** until Architect issues `.task.md`.

| Phase | Proposed unit theme | Allowed work (when authorized) | Exit criteria |
|-------|---------------------|--------------------------------|---------------|
| **I1** | Sellable Departure + Price data | Tour admin/API create+publish Departure; Pricing admin create Price; **no fake amounts** | Public published list returns 1; pricing summary 200 |
| **I2** | Public Tour detail composition | FE: load published departures; load price summary; honesty empty states | Evidence screenshots |
| **I3** | Booking initiation UX | FE: form → initiations → Pending view with token | API 201 Pending evidenced |
| **I4** | Payment boundary | Option A stop **or** Architect-chosen B | Documented + UI honest |
| **I5** | Slice GATE | Evidence pack review | Architect ACCEPT |

### Dependencies

```text
I1 → I2 → I3 → I4 → I5
P33-T002/T003 ACCEPT (done) → I1
Payment Option B depends on Architect provider decision ≠ I1
```

---

## 8. Risks

| Risk | Mitigation |
|------|------------|
| Fake Departure/Price to unblock FE | Forbidden; fail honest |
| Big-bang FE+BE+Payment | Stick to I1→I5 sequence |
| Treating Pending as sold | Copy + status discipline |
| DemoFeed as Rate SoR | Removable; production content later |
| Scope creep to Hotel | Separate Architect phase |

---

## 9. Forbidden shortcuts

1. Fake departures, prices, payment success, Confirmed screenshots  
2. Hardcoded TourDeparture Guids in FE  
3. Duplicating Pricing/Booking models in FE state  
4. New commercial engines when APIs exist  
5. Production provider without Architect lock  
6. Seed/migration invented without authorized task  

---

## 10. Required ADRs (candidates)

| Candidate | Topic |
|-----------|--------|
| ADR-R1 | Tour Public commerce composition sequence |
| ADR-R2 | Honesty matrix (no departure / no price / no payment) |
| ADR-R3 | Payment boundary Option A vs sandbox labeling |

Create only when Architect authorizes ADR tasks.

---

## 11. Architecture findings

1. Smallest sellable wedge is **data readiness + FE composition**, not new modules.  
2. Payment can be deferred honestly (Option A) without blocking booking-intent proof.  
3. DEMOFEED is a convenient catalog host, not a license to invent rates.

---

## 12. Cursor conclusion

| Field | Value |
|-------|--------|
| Roadmap | I1 data → I2 compose → I3 booking UX → I4 payment boundary (A first) → I5 GATE |
| Created | `docs/plans/P33-tour-commerce-implementation-roadmap.md` |
| Product code | **None** |
| Next | AWAITING_ARCHITECT_REVIEW — wait for next `.task.md` / `.gate.md` only |
