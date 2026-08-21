# P33 — Tour Commerce Data Contracts

| Field | Value |
|-------|--------|
| Document | `docs/plans/P33-tour-commerce-data-contracts.md` |
| Task-ID | `TC-P33-T003` |
| Phase | P33 — Commercial Product Readiness |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Nature | Domain analysis / planning only |
| Related | [`P33-tour-first-commerce-slice.md`](P33-tour-first-commerce-slice.md) · [`P33-commercial-readiness-plan.md`](P33-commercial-readiness-plan.md) |
| Forbidden | Code · fake departures/prices · Booking/Payment modifications · FE/BE/DB/migrations/seed |

---

## 1. Analysis summary

The Tour-first E2E slice is **contract-rich but data/composition-poor**.

| Layer | Reality |
|-------|---------|
| Module engines | Tour / Pricing / Booking / Payment largely **exist** |
| Public contracts | Many **already defined** (see capability map) |
| DEMOFEED catalog | TourProduct + media **exist**; sellable **TourDeparture + Price** for those products typically **missing** |
| Public FE | Discovery/detail **exist**; departure→price→booking→payment funnel **not composed** |

**Do not** invent missing pieces with fake data, FE-only state, duplicated models, or ownership shortcuts.

---

## 2. Target journey ↔ data needs

```text
Discover Tour          → TourProduct public catalog + media
Tour Detail            → TourProduct facts + media presentation
Select TourDeparture   → Published departures for TourProduct
Pricing honesty        → Public price summary for selected Departure (or honest empty)
Booking initiation     → TourDepartureId + passengers/contact → Pending + Quote snapshot
Payment boundary       → Booking payment read/attempt (provider or honest stop)
Confirmation state     → Real Booking/Payment status fields only
```

---

## 3. Current capability map

### 3.1 TourProduct (Tour)

| Capability | Status | Notes |
|------------|--------|-------|
| Identity / code / slug | ✅ | Public by-slug / by-code / listing |
| Content / translations | ✅ | |
| Package / catalog facts | ✅ | services/policies/requirements surfaces |
| Media Cover/Gallery | ✅ | + P32 DEMOFEED enrich |
| DEMOFEED products | ✅ | e.g. demofeed tour Tehran/Istanbul |

**Ownership:** Tour owns TourProduct identity, content, package definition, media links.

### 3.2 TourDeparture (Tour)

| Capability | Status | Notes |
|------------|--------|-------|
| Domain aggregate (schedule, capacity, transport, accommodation) | ✅ | P11 |
| Admin CRUD `/api/tour/departures` | ✅ | Auth policies |
| Public published list by product | ✅ | `GET /api/tour/products/{tourProductId}/departures/published` |
| Public published by id | ✅ | `ITourDeparturePublicQuery.GetPublishedByIdAsync` (used by Booking) |
| Contract note | ✅ | **Published ≠ bookable** (P11-R8) |
| DEMOFEED sellable Published departure | ⚠️ Missing / not evidenced for Public slice | Catalog products alone are insufficient |

**Ownership:** Tour owns when / capacity / operational availability facts. Pricing/Booking must not own Departure tables.

### 3.3 Pricing

| Capability | Status | Notes |
|------------|--------|-------|
| Price on logical `TourDeparture` target | ✅ | P12-R3 |
| Admin price write | ✅ | `/api/pricing/prices` |
| Public summary by target | ✅ | `GET /api/pricing/public/summaries?targetType&targetId` |
| Public summary by departure | ✅ | `GET /api/pricing/public/tour-departures/{tourDepartureId}` |
| Quote issuance for Booking | ✅ | Used inside public Booking initiation (`IssueForTourDepartureAsync`) |
| DEMOFEED Price for a Published departure | ⚠️ Missing / not evidenced | |

**Ownership:** Pricing owns price calculation and quote generation. No Booking ownership of Quote SoT.

### 3.4 Booking

| Capability | Status | Notes |
|------------|--------|-------|
| Public initiation | ✅ | `POST /api/booking/public/initiations` |
| Request needs | ✅ | `TourDepartureId` + Contact + Passengers (+ Idempotency) |
| Pending + access token | ✅ | `X-TravelCore-Booking-Access-Token` |
| Public read | ✅ | `GET /api/booking/public/{bookingId}` |
| Issues Quote at initiation | ✅ | Monetary snapshot on response |
| Capacity check vs Departure | ✅ | Uses Tour public query |
| Confirm endpoint | ⚠️ Historically deferred / boundary flags | Do not invent Confirm in this task |
| Payment compose endpoints | ✅ Present on public Booking surface | Provider may still be NONE |

**Ownership:** Booking owns customer intent and reservation lifecycle. Does not own Price tables or TourDeparture tables.

### 3.5 Payment

| Capability | Status | Notes |
|------------|--------|-------|
| Module + provider abstraction | ✅ | P20 |
| Booking-scoped payment reads/attempts | ✅ (wired via Booking public) | |
| Production Payment Provider | ❌ NONE | Honest stop or Architect-approved sandbox |

**Ownership:** Payment owns payment boundary and settlement lifecycle. Payment Succeeded ≠ automatic tourism “sold” storytelling without Booking state rules.

### 3.6 Frontend Public

| Capability | Status |
|------------|--------|
| Tour discovery/detail + media | ✅ |
| List published departures on detail | ❌ not composed |
| Show Pricing summary honesty | ❌ not composed |
| Booking initiation UX | Partial / fixture / prepare shells — not DEMOFEED E2E evidenced |
| Payment UX | Module pages exist; not Tour-first DEMOFEED evidenced |

---

## 4. Missing capabilities (for Tour-first MVP)

| ID | Missing | Owner to extend later | Must not fake |
|----|---------|------------------------|---------------|
| M1 | At least one **Published** TourDeparture for a Public TourProduct | Tour (admin/seed — Architect-authorized) | No FE-only departure |
| M2 | At least one **Price** targeting that Departure | Pricing (admin) | No hardcoded FE money |
| M3 | Public FE composition: departures list → price summary → initiate | Frontend + existing APIs | No duplicated Booking model |
| M4 | Honesty UX for empty departures / 404 price / provider NONE | Frontend + copy | No fake Confirmed |
| M5 | Architect decision: sandbox Payment vs stop-before-money | Payment/Architect | No silent production provider |

**Important:** M1/M2 are **data readiness**, not new domain engines. They still require Architect-authorized tasks (no seed invented in T003).

---

## 5. Required entities / contracts (MVP)

### Entities (reuse)

| Entity | Module | MVP need |
|--------|--------|----------|
| TourProduct | Tour | Existing Public product |
| TourDeparture (Published) | Tour | One sellable option |
| Price (target TourDeparture) | Pricing | One authoritative price |
| Quote | Pricing | Created at Booking initiation |
| Booking (Pending…) | Booking | Initiation response |
| Payment attempt (optional path) | Payment | Boundary only if authorized |

### HTTP contracts to compose (existing)

| Step | Contract |
|------|----------|
| Discover/Detail | Existing Tour Public product APIs + media presentation |
| Departures | `GET /api/tour/products/{tourProductId}/departures/published` |
| Price honesty | `GET /api/pricing/public/tour-departures/{tourDepartureId}` → 200 summary **or** 404 honest empty |
| Initiate | `POST /api/booking/public/initiations` with `TourDepartureId`, contact, passengers |
| Read booking | `GET /api/booking/public/{bookingId}` + access token header |
| Payment | Existing Booking public payment endpoints — only if provider path authorized |

### Composition rules

1. FE may **compose** reads; FE must not become SoT for Price/Quote/Booking.
2. Show money **only** from Pricing summary or Booking monetary snapshot after initiation.
3. Empty published list ⇒ no initiate CTA (or disabled + honest message).
4. Price 404 ⇒ show unavailable; do not invent.
5. Published Departure without Price is valid honesty state.
6. Published ≠ bookable remains true until Booking initiation succeeds under module rules.

---

## 6. Ownership boundaries (locked)

```text
TourProduct     → Tour
TourDeparture   → Tour
Price / Quote   → Pricing
Booking         → Booking
Payment         → Payment
Public UI       → Composition only
DemoFeed        → Removable feeder (not Rate SoR)
```

| Anti-pattern | Why forbidden |
|--------------|---------------|
| Price on TourProduct in FE | Violates P12-R3 |
| Duplicate Quote in Tour | Ownership leak |
| Booking inventing Departure capacity | Must query Tour |
| FE storing “confirmed” without Booking | Fake commerce |
| Seed fake rates in UI for screenshots | Honesty breach |

---

## 7. MVP scope vs deferred

### MVP (minimum for “slice real”)

1. One Public TourProduct (can be DEMOFEED)
2. One Published TourDeparture for it
3. One Price for that Departure
4. Public FE path: detail → select departure → show price or empty → initiate Pending
5. Payment: **either** labeled sandbox attempt **or** explicit stop with honest boundary
6. Evidence pack (API + screenshots)

### Deferred

- Multiple departures / complex occupancy UX
- Confirm productization if still deferred by Booking boundary
- Production Payment provider
- Hotel/Flight sell slices
- Destination Gallery
- Global tour search engine
- Agency offer routing as primary path
- FX conversion

---

## 8. Risks

| Risk | Mitigation |
|------|------------|
| Treating Published as bookable without Booking | Keep P11-R8; only Booking initiation proves book path |
| Seeding fake prices for demos | Architect-authorized real Pricing rows only |
| Skipping public departures API and hardcoding Guid | Forbidden |
| Expanding MVP to full Confirm+Provider in one leap | Split Architect tasks |
| DemoFeed becoming permanent Rate SoR | Removable; production content later |

---

## 9. ADR requirements (candidates)

| Candidate | Topic |
|-----------|--------|
| ADR-D1 | Public Tour commerce composition sequence (Tour → Pricing → Booking → Payment) |
| ADR-D2 | Honesty matrix: no departure / no price / no provider |
| ADR-D3 | DEMOFEED vs production data for sellable Departures/Prices |

Do **not** create ADR files in this task.

---

## 10. Recommended next (Architect file only)

1. ACCEPT this contracts map (or revise).
2. Authorize a **data readiness** task (TourDeparture + Price for one Public product) — not FE-only.
3. Then authorize **Public composition** task (FE wiring to existing APIs).
4. Then authorize **Payment boundary decision** task.

No implementation from this document alone.

---

## 11. Cursor conclusion

| Field | Value |
|-------|--------|
| Finding | Engines/contracts largely exist; MVP blocked by missing sellable Departure/Price data + Public FE composition + provider decision |
| Created | `docs/plans/P33-tour-commerce-data-contracts.md` |
| Product code | **None** |
| Next | AWAITING_ARCHITECT_REVIEW |
