# P33 — Tour-First Commerce Slice (Architecture)

| Field | Value |
|-------|--------|
| Document | `docs/plans/P33-tour-first-commerce-slice.md` |
| Task-ID | `TC-P33-T002` |
| Phase | P33 — Commercial Product Readiness |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Nature | Architecture / contract planning only |
| Related | [`P33-commercial-readiness-plan.md`](P33-commercial-readiness-plan.md) |
| Forbidden in this task | Booking/Payment implementation · fake prices · supplier integrations · FE/BE/DB changes |

---

## 1. Recommendation

**Adopt Tour-first narrow E2E as the first sellable slice.**

| Why Tour-first | Why not Hotel-first |
|----------------|---------------------|
| Buyable Pricing target is already **TourDeparture** (P12-R3) | HotelBooking requires supplier/source; production source = NONE (P21) |
| Tour Public catalog + media already demo-proven (P31/P32) | Place catalog ≠ HotelBooking; harder honesty story with zero rates |
| Booking module public initiation already exists (`/api/booking/public`) | HotelBooking token/journey is separate and heavier |
| Clear ownership chain Tour → Departure → Price → Quote → Booking → Payment | Hotel path risks inventing rates to “look sellable” |

**Slice goal:** one traveler can go from **discover tour → commercial option → booking intent → payment boundary → confirmation-capable state** using **real module boundaries**, with **honest unavailable** when offers/providers are missing.

---

## 2. User journey (narrow MVP)

```text
1. Discover tour          Public Tour listing (destination-scoped OK for MVP)
2. View tour details      Public Tour detail + media (P32)
3. Select commercial option
                          Choose an available TourDeparture (not TourProduct price)
4. See price honesty      Pricing public price summary for that Departure — or honest empty
5. Create booking intent  Booking public initiation → Pending (existing P19 posture)
6. Payment boundary       Payment attempt only if Architect-approved provider/sandbox label
7. Confirmation state     Real Booking/Payment states — never fake “Confirmed”
```

### MVP acceptance (definition)

All must hold for **one** Tour journey:

1. Discoverable TourProduct with honest media
2. At least one **TourDeparture** selectable (or honest “no departures”)
3. Price shown only from **Pricing** for that Departure (or honest “no price”)
4. Booking initiation creates **Pending** via Booking module (opaque access token rules)
5. Payment step either runs labeled sandbox/provider **or** stops with honest boundary message
6. No fabricated availability, rates, or confirmation
7. Evidence under `docs/product-experience/evidence/`

---

## 3. Domain ownership map (invariants)

```text
TourProduct     ≠  TourDeparture
Price           ≠  Quote
Quote           ≠  Booking
Booking         ≠  Payment
PublicExperience ≠ Booking SoT
Pending         ≠  Confirmed
BookingId       ≠  Access credential
```

| Concept | Owner module | Notes |
|---------|--------------|-------|
| TourProduct / media / catalog | **Tour** | Discovery & presentation |
| TourDeparture / capacity / schedule | **Tour** | Buyable commercial option |
| Price (authoritative money) | **Pricing** | `TargetType=TourDeparture` + `TargetId` Guid — no Tour FK |
| Quote (calculation snapshot) | **Pricing** | Not Booking; no Payment |
| Booking aggregate / Pending | **Booking** | Public initiation + access token |
| Payment attempt / lifecycle | **Payment** | Not Price; not Booking confirmation by itself |
| Public UI composition | **Frontend Public Experience** | Composer only — not SoT |

---

## 4. API / contract boundaries (reuse-first)

### Existing (reuse — do not redesign)

| Concern | Known boundary |
|---------|----------------|
| Pricing public summary | P12-R8 public read-only price summary by logical target (TourDepartureId) — **no Book Now in Pricing** |
| Booking public | `POST /api/booking/public/initiations` · `GET /api/booking/public/{bookingId}` · header `X-TravelCore-Booking-Access-Token` |
| Booking composition constants | `PublicBookingCompositionBoundary` — Confirm endpoint historically deferred; Payment endpoint flag present — verify before wiring UI |
| Tour Public discovery/detail | Existing Public Tour APIs used by P31/P32 FE |
| Media presentation | App-proxy URLs only |

### Composition gaps (likely — Architect to authorize)

| Gap | Description |
|-----|-------------|
| Tour detail → Departures list | Public FE may need a read model of published Departures for a TourProduct |
| Departure → Price summary compose | FE/BFF composition calling Pricing public summary without leaking cross-module ownership |
| Detail CTA → Booking initiation payload | Correct Quote/Departure/occupancy inputs without inventing prices in FE |
| Pending → Payment handoff | Follow Booking/Payment contracts; no Confirm shortcut |

**No new commercial engine in this plan.** Prefer composition + honesty over new modules.

---

## 5. Required modules (MVP)

| Module | Role in slice | Status |
|--------|---------------|--------|
| Tour | Product + Departure + Public catalog | COMPLETE historically · DEMOFEED products exist |
| Pricing | Price / Quote · public summary | COMPLETE (P12) |
| Booking | Public initiation Pending | COMPLETE (P19) · Confirm deferred historically |
| Payment | Attempt / provider abstraction | COMPLETE (P20) · **Production provider NONE** |
| Media | Covers on Tour | COMPLETE + P32 enrich |
| Frontend Public | Journey UX | Partial — discovery/detail exist; sell funnel not complete |

### Missing domain capabilities (for MVP, not infinite backlog)

1. **Demo/seed of one sellable TourDeparture** linked to DEMOFEED TourProduct (Architect-authorized seed — not fake price)
2. **At least one Price** for that Departure (real Pricing write path / admin) — or slice stops at honest empty
3. **Public composition contract** documenting FE call sequence
4. **Sandbox/labeled Payment path** decision (or stop before money)
5. Possible **ADR(s)** if composition introduces new cross-module rules

---

## 6. Implementation phases (proposal only)

Do **not** execute until Architect issues task files.

| Phase | Name | Outcome |
|-------|------|---------|
| **S0** | Contract map | Document exact endpoints + DTO fields + honesty states (this doc + follow-up if needed) |
| **S1** | Data readiness | One TourProduct + one TourDeparture + Price (no fake amounts — real Pricing entries) |
| **S2** | Public composition read | Detail shows Departures + price summary or honest empty |
| **S3** | Booking intent | Initiate Pending + access token UX (private pages) |
| **S4** | Payment boundary | Sandbox/provider **or** explicit stop with honest message |
| **S5** | Evidence GATE | Screenshots + API notes; Architect ACCEPT |

Hotel-first and Flight remain **out of this slice**.

---

## 7. What belongs in MVP vs deferred

### In MVP

- TourProduct discovery/detail (existing)
- TourDeparture selection
- Pricing summary honesty
- Booking Pending initiation
- Payment boundary decision (sandbox or stop)
- Evidence pack

### Deferred (explicit)

- Destination Gallery polish
- Global tour browse / Search engine upgrade
- Hotel sell slice
- Production Payment provider
- Confirm endpoint productization if still deferred
- Partial refunds / amendments / multi-currency FX conversion
- Agency marketplace sell desk
- Fake “Book Now” with hardcoded prices

---

## 8. Risks

| Risk | Mitigation |
|------|------------|
| Pricing TourProduct instead of TourDeparture | Enforce P12-R3 in contracts/reviews |
| Showing invented prices in FE | Forbid; empty/unavailable only |
| Treating Pending as Confirmed | UI copy + state machine discipline |
| Skipping Quote | Follow Pricing ownership; do not invent Booking-owned quotes |
| Payment success = Booking confirmed | Keep module semantics (Payment Succeeded ≠ auto-Confirmed unless Architect lock says otherwise) |
| Expanding slice to Hotel mid-flight | Architect gate; new file required |
| DemoFeed as permanent rate SoR | Removable feeder; production content separate |

---

## 9. Forbidden shortcuts

1. Hardcoded prices / availability / “Confirmed” screenshots
2. FE calling Media/storage keys directly
3. Tour module owning Price tables
4. Pricing owning Booking or Payment
5. PublicExperience becoming Booking SoT
6. Supplier integrations invented for Tour slice
7. HotelBooking conflated into Tour journey
8. Auto-starting S1–S5 without Architect `.task.md`

---

## 10. ADR requirements (candidates)

Architect decides which become formal ADRs. Candidates:

| Candidate | Topic |
|-----------|--------|
| ADR-C1 | Public Tour commerce composition boundary (Tour ↔ Pricing ↔ Booking ↔ Payment) |
| ADR-C2 | Honesty states for missing Departure / Price / Provider |
| ADR-C3 | Sandbox Payment labeling vs production provider gate |

Do **not** invent ADR numbers/files in this task.

---

## 11. Architecture findings summary

1. **Reuse dominates** — engines largely exist; gap is **Public composition + sellable Departure/Price data + provider decision**.
2. **Tour-first is the lowest-risk sellable wedge** aligned with Pricing’s TourDeparture target.
3. **Honesty remains the commercial brand** — empty states are valid MVP outcomes when data/providers missing.
4. **Confirm/Payment productionization** may still need Architect locks before claiming “sold.”

---

## 12. Cursor conclusion

| Field | Value |
|-------|--------|
| Commerce slice recommendation | **Tour-first narrow E2E** (Discover → Departure → Price honesty → Booking Pending → Payment boundary) |
| Created document | `docs/plans/P33-tour-first-commerce-slice.md` |
| Product code | **None** |
| Next | AWAITING_ARCHITECT_REVIEW — wait for next authorized `.task.md` / `.gate.md` only |
