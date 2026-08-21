# P33 — Commercial Product Readiness Plan

| Field | Value |
|-------|--------|
| Document | `docs/plans/P33-commercial-readiness-plan.md` |
| Task-ID | `TC-P33-T001` |
| Phase | P33 — Commercial Product Readiness Review |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Inputs | `P32-GATE/GATE-REVIEW.md` · P30–P32 SoT · ROADMAP P10–P22 · PROJECT-STATE · RECOVERY-CONTEXT |
| Nature | **Analysis / planning only** — no product code · no architecture redesign · no commercial engines |

---

## 1. Question

> What prevents TravelCore from becoming a production-ready sellable travel commerce platform?

**Short answer:** TravelCore is now a **credible commercial demo product** (experience + catalog + honest media). It is **not** yet a **sellable production commerce platform** because the Public conversion path still lacks wired, production-backed **price → departure/availability → booking → payment → confirmation** for the demo catalog, and several operational/supplier/production dependencies remain deferred by design.

```text
Demo Product          ✅ (P30–P32 ACCEPTED WITH KNOWN LIMITATIONS)
        ↓
Sellable Product      ⚠️ gaps below (composition + real rates + end-to-end UX)
        ↓
Production Commerce   ⏳ providers · suppliers · ops · compliance at scale
```

---

## 2. Current state (accepted context)

| Layer | Status | Evidence |
|-------|--------|----------|
| Product Experience Foundation | ✅ P30 FOUNDATION ACCEPTED | shells, DS 2.0, Public/Admin/Agency foundations |
| Commercial Demo UX | ✅ P31 ACCEPTED WITH KNOWN LIMITATIONS | Home / Hotel / Tour commerce chrome |
| Demo data + media | ✅ P32 ACCEPTED WITH KNOWN LIMITATIONS | DEMOFEED · Hotel/Tour media · Destination Cover · Home covers |
| Architecture modules (historical) | ✅ many COMPLETE | Pricing P12 · Tour Booking P19 · Payment P20 · Hotel Booking P21 · Flight P22 · etc. |
| Honesty posture | ✅ | No fake prices/availability/reviews on Public demo surfaces |

### What exists today on Public demo surfaces

- Professional Home marketplace with **real Destination covers**
- Hotel discovery listing/detail with **enriched covers** (+ stars)
- Tour discovery listing/detail with **enriched covers** (destination-scoped)
- Removable DemoFeed (tool-side; not a permanent product module)
- Preserved boundaries: Place ≠ HotelBooking · Tour ≠ Pricing/Booking ownership conflation · Media technical vs domain semantic ownership

### What “exists in modules” but is not the demo sell path

Backend phases delivered **engines and contracts** for Pricing, Booking, Payment, HotelBooking, FlightBooking, etc. That does **not** mean Public DEMOFEED catalog journeys are end-to-end sellable. Current Public Hotel/Tour commerce polish deliberately stays honest: catalog presentation without fabricating rates or inventory.

---

## 3. Remaining gaps (by domain)

### 3.1 Product Experience

| Gap | Why it blocks sellability | Severity |
|-----|---------------------------|----------|
| Search / discovery quality | Home search is route-guidance, not a production search engine; Tour listing is destination-scoped; global browse / relevance incomplete for “find & buy” | **High** for conversion |
| Conversion flows | Detail sticky/actions and “ادامه به رزرو” are presentation / prepare-path oriented; not a complete Public sell funnel on DEMOFEED entities | **Critical** |
| Detail page maturity | Gallery density (esp. Hotel slots, Destination Gallery deferred Option A), trust blocks, itinerary/commerce density still PARTIAL vs North Star | **Medium** |
| Trust elements | Honesty copy is strong; production trust (reviews policy, supplier identity, cancellation clarity on Public) not fully productized for sell | **Medium** |
| Destination Gallery | Cover-only by P32 Option A — OK for demo; weaker for destination landing sell pages | **Low–Medium** (deferred) |

### 3.2 Commercial Capabilities

| Gap | Why it blocks sellability | Severity |
|-----|---------------------------|----------|
| Real pricing on Public catalog | Price/Quote exist as modules (P12) but DEMOFEED Hotel/Tour Public surfaces do not present buyable prices | **Critical** |
| Departures / buyable targets | TourDeparture is the buyable Pricing target (P12-R3); DEMOFEED tours are product-level demo cards without a sellable departure offer path | **Critical** |
| Booking flow (Public E2E) | Tour Booking (P19) / Hotel Booking (P21) modules exist; Public demo catalog is not a completed initiate→reserve→confirm journey for DEMOFEED | **Critical** |
| Payment flow | Payment module (P20) exists; **Production Payment Provider = NONE** (P21/P22 locks) | **Critical** for production money |
| Customer journey continuity | Catalog → offer → passenger/guest → pay → confirmation → post-booking ops not unified as one Public commerce story | **Critical** |

### 3.3 Operational Capabilities

| Gap | Why it blocks sellability | Severity |
|-----|---------------------------|----------|
| Admin inventory / rate ops | Admin foundations exist; day-to-day inventory/rate operations for live sell not demo-proven as a sellable agency workflow | **High** |
| Agency operations | Agency marketplace (P13) / shells exist; sales desk workflow for DEMOFEED→live sell incomplete | **High** |
| Supplier / source posture | Named Hotel/Flight suppliers NONE; zero sources must not fabricate availability (P21/P22 locks) | **Critical** for production |
| Refunds / amendments | Partial refund / rebooking / pay-later often DEFERRED | **Medium** (post-MVP) |

### 3.4 Architecture Readiness

| Check | Verdict |
|-------|---------|
| Boundary violations in P30–P32 demo path | **No known violations** — Media / Place / Tour / Destination Cover / DemoFeed rules held |
| Missing ADRs for next sell path | Likely need **Architect-authorized** ADRs when wiring Public catalog ↔ Pricing/Booking (do not invent in this task) |
| Technical risks | Runtime DemoFeed media blob sync; EF query translation sensitivity (lesson from T004); provider/supplier absence; FX conversion still deferred |
| Over-building risk | Treating module COMPLETE as Public sell-ready would violate honesty rules and P21/P22 “no fabricate” locks |

---

## 4. Priorities (recommended order)

Priority is **Architect-gated**. This list is analysis only — **not** an execution queue.

| Priority | Theme | Intent |
|----------|--------|--------|
| **P0** | Commercial composition map | Explicit SoT: which Public entities become buyable (TourDeparture vs Place stay vs HotelBooking initiation) without inventing prices |
| **P1** | Sellable Tour path (narrow) | One DEMOFEED (or real) TourDeparture with Price summary → Quote → Booking prepare → Payment sandbox → confirmation — **honest empty if unavailable** |
| **P2** | Sellable Hotel path (narrow) | One HotelBooking initiation path that respects zero-supplier rules OR Architect-authorized demo source — never fake rates |
| **P3** | Discovery upgrade | Search/browse improvements that feed the sell path (not cosmetic-only) |
| **P4** | Ops readiness | Admin/Agency workflows to publish offers, monitor bookings, handle cancellations |
| **P5** | Production providers | Real Payment provider · hotel/flight sources · observability/runbooks already largely P28/P29 |
| **P6** | Experience polish | Destination Gallery, denser galleries, trust UX — after sell path exists |

---

## 5. Recommended phases (proposal for Architect)

Names are **proposals**. Do **not** execute until Architect issues `.task.md` / `.gate.md`.

### Phase A — Commercial Composition & Honesty Guard (docs + contracts)

- Map Public surfaces → module APIs (Pricing / Booking / HotelBooking / Payment)
- Lock “when to show price” vs “honest unavailable”
- Explicit anti-patterns: no hardcoded prices, no fake availability

### Phase B — Narrow Sellable Slice (Tour-first recommended)

- One end-to-end Tour sell journey with real module boundaries
- Evidence screenshots + API notes
- Prefer TourDeparture + Pricing public summary (P12-R8) over inventing product-level prices

### Phase C — Narrow Sellable Slice (Hotel)

- HotelBooking public journey aligned with P21-R8 token rules
- Supplier NONE ⇒ honest failure path must remain correct

### Phase D — Discovery & Conversion UX

- Only after A–C direction accepted
- Search, listing density, detail conversion, mobile funnel

### Phase E — Production Commerce Hardening

- Payment provider · supplier integrations · agency ops · refunds policy execution
- Compliance / PCI posture (no card collection remains)

```text
Demo Product (P30–P32)
        ↓
A Composition / honesty
        ↓
B Tour sellable slice
        ↓
C Hotel sellable slice
        ↓
D Discovery / conversion UX
        ↓
E Production providers & ops
        ↓
Production Commerce
```

---

## 6. Risks

| Risk | Mitigation |
|------|------------|
| Inventing fake prices to “look sellable” | Forbidden — honesty > North Star density |
| Conflating Place catalog with HotelBooking | Keep Place ≠ HotelBooking invariant |
| Conflating TourProduct with TourDeparture pricing | Keep P12-R3 buyable target rules |
| Treating module COMPLETE as Public E2E ready | Require live evidence on Public surfaces |
| Parallel phase sprawl | One Architect file at a time (Pipeline V3) |
| DemoFeed becoming permanent commerce SoR | Keep DemoFeed removable; production content elsewhere |

---

## 7. Dependencies

| Dependency | Notes |
|------------|-------|
| Architect ACCEPT of this plan | Required before any P33 implementation task |
| Existing Pricing / Booking / Payment / HotelBooking modules | Reuse; do not redesign |
| P32 media/demo catalog | Useful demo inventory; not a substitute for rates |
| Payment provider decision | Blocks production money movement |
| Supplier / inventory source decision | Blocks production hotel/flight sell |
| Pipeline discipline | No auto-start of next units without `.task.md` / `.gate.md` |

---

## 8. Explicit non-goals (this task / immediate P33)

- No frontend / backend / database / migration / product feature implementation in T001
- No architecture redesign
- No adding commercial engines in this document’s execution
- No inventing T002+ task IDs as executable work

---

## 9. Success criteria for “Sellable Product” (definition proposal)

A **Sellable Product** slice exists when **all** hold for at least one Public journey:

1. Catalog entity discoverable with honest media
2. Buyable offer/price shown from Pricing (or honest unavailable)
3. Booking can be initiated without fabricated inventory
4. Payment attempt can run against an Architect-approved provider **or** documented sandbox with clear non-production label
5. Confirmation / failure states are real module states
6. Evidence pack exists under `docs/product-experience/evidence/`
7. Boundaries and honesty rules still hold

**Production Commerce** additionally requires production providers/suppliers, ops runbooks, and Architect GATE.

---

## 10. Recommendations to Architect

1. **ACCEPT** this readiness plan as the P33 planning baseline (or revise priorities).
2. Authorize **Phase A** next as documentation/contract mapping — not a big-bang Booking rewrite.
3. Prefer **Tour-first narrow E2E** before Hotel (clearer Pricing↔TourDeparture story).
4. Keep **DemoFeed removable**; do not make it the production rate source.
5. Do not treat Destination Gallery or further media polish as the primary blocker to sellability.

---

## 11. Cursor conclusion

| Field | Value |
|-------|--------|
| Analysis summary | Demo product ready; sellable/production blocked by unwired Public conversion + missing production providers/suppliers + discovery/ops gaps |
| Created document | `docs/plans/P33-commercial-readiness-plan.md` |
| Product code | **None** |
| Next | AWAITING_ARCHITECT_REVIEW — wait for next authorized `.task.md` / `.gate.md` only |
