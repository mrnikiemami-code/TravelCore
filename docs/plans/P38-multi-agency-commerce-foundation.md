# P38 — Multi-Agency Commerce Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T001` |
| Phase | P38 — Multi-Agency Commerce |
| Date | 2026-08-21 |
| Type | Architecture / commerce planning (no implementation) |
| Prior gate | `TC-P37-GATE` · **PASS WITH KNOWN LIMITATIONS** · **ACCEPTED** · Option B locked |

## 1. Purpose

Define the first multi-agency commerce slice so TravelCore becomes a true marketplace:

```text
Tour Product
    + Multiple Agencies
    + Agency Offers
    + Customer Selection
```

This document plans **contracts and ownership**. It does **not** authorize Offer UI, commission engines, or booking changes in T001.

---

## 2. Marketplace commerce model

### Relationship chain

```text
TourProduct
    │
    ├─ TourDeparture(s)          (schedule / inventory facts of the product)
    │
    └─ AgencyOffer(s)            (commercial proposals by agencies)
            │
            ├─ Agency (Party: TravelAgency)
            ├─ Offer terms (price direction, inclusions, validity)
            └─ Sales channel / visibility rules
                    │
                    ▼
            Customer Selection   (choose an offer on the public journey)
                    │
                    ▼
            Quote → Booking → Payment   (existing ownership chain)
```

### Locked inequalities

| Rule | Implication for P38 |
|------|---------------------|
| TourProduct ≠ TourDeparture | Offers attach to product (and optionally constrain departures) — they do not replace departures |
| Price ≠ Quote | Agency offer price direction ≠ traveler quote snapshot |
| Quote ≠ Booking | Selecting an offer does not create a booking |
| Booking ≠ Payment | Agency commission/settlement ≠ payment success theater |

### What “one tour, many agencies” means

- Public detail shows **one TourProduct** fact set (itinerary, media, destinations).
- Commercial choice presents **multiple AgencyOffers** (when more than one is published).
- Customer selects **which agency commercial path** initiates booking — not which fake inventory row.

---

## 3. Agency Offer concept (foundation)

### Definition (direction)

**AgencyOffer** — a commercial proposal by an Agency Party over a TourProduct (and optionally scoped TourDepartures), carrying terms that Pricing/Booking may later consume.

### Directional attributes (not schema yet)

| Concern | Direction | Honesty |
|---------|-----------|---------|
| Agency-specific pricing | Offer references Pricing ownership; may express agency margin/commission inputs later | No invented rates in UI |
| Agency terms | Cancellable policies / inclusions text as content facts | No fake “best seller” badges |
| Availability ownership | Departure availability remains Tour/Inventory ownership; Offer must not invent seats | FE ≠ SoT |
| Sales channel | Public / Agency-portal-only / private | Explicit channel flags later |
| Commission | Settlement direction between platform and agency | Empty/honest until contracts exist |

### Explicit non-goals for early P38 slices

- Full settlement ledger
- Dynamic yield engine
- Fake multi-agency demo rows without Party/Access binding

---

## 4. Customer experience impact (no final UI)

### Traveler sees

1. Tour product merchandising (unchanged ownership).
2. A **selection step** when ≥2 published AgencyOffers exist.
3. Trust cues tied to **real agency party facts** (name/profile when available) — not invented ratings.
4. Booking initiation that carries **selected Offer identity** into Quote/Booking inputs.

### When only one offer exists

- UI may collapse selection (single path) without pretending multi-agency competition.
- Still store Offer identity for auditability.

### Forbidden UX

- Role-toggle from Public to Agency on the same page.
- Showing multiple “sellers” that are hardcoded demo strings.
- Confirming booking because an offer was selected.

---

## 5. Domain boundary review

| Module | Owns | P38 touch |
|--------|------|-----------|
| Tour | TourProduct, TourDeparture, publish facts | Offer **references** product/departure; does not own commercial terms |
| Party / Agency | Agency party identity | Offer **belongs to** Agency Party |
| Access | Who may manage offers for an agency | Permission-aware Agency Portal / Admin |
| Pricing | Price facts / summaries | Offer may point to price policy; Quote remains Pricing |
| Booking | Booking lifecycle | Must accept Offer reference on initiation (future contract) |
| Payment | Payment lifecycle | Unchanged; Booking ≠ Payment preserved |
| FE Public / Agency / Admin | Presentation | Surfaces host Offer UX later; FE never SoT |

### Future contracts (named for later tasks)

1. **AgencyOffer contract** (identity, agencyId, tourProductId, optional departure scope, channel, status).
2. **Offer → Quote input** mapping.
3. **Booking initiation** extension: `agencyOfferId` (or equivalent) required when multi-offer.
4. **Admin approval / publish** states for offers (direction only until Access ready).

---

## 6. Agency Portal & Admin impact

### Agency Portal (`/agency`)

Future capabilities (post-T001):

- Offer management for own Party
- Sales workflow: select product → define offer terms → submit/publish (per Access)
- Bookings/customers filtered by agency relationship

### Admin Console (`/admin`)

Future capabilities:

- Agency management + commercial relationship
- Offer approval / policy rules
- Cross-agency operational views (not CRUD dump of tables)

P37 foundations already provide IA slots for these — P38 fills commercial meaning.

---

## 7. Migration safety

### Reuse

- Existing TourProduct / TourDeparture public commerce path (P33–P36)
- Agency / Admin / Customer shells (P37)
- Booking initiation + Payment honesty (P33–P34)
- Identity ≠ Party ≠ Access (P24/P25 lineage)

### Missing today

- AgencyOffer domain + APIs
- Multi-offer public selection UX
- Offer-aware booking initiation
- Commission/settlement facts

### Risks

| Risk | Mitigation |
|------|------------|
| Collapsing Offer into TourDeparture | Keep Offer as commercial layer; departures stay schedule |
| Fake multi-agency demo data | Only real Party-bound offers; honest empty when none |
| Breaking single-path booking | Support single-offer collapse; dual-path feature flag / gradual |
| Pricing ownership bypass | Quotes still from Pricing; Offer feeds inputs only |
| Scope explosion into settlements | Slice: Offer publish → customer select → booking init first |

---

## 8. Recommended implementation roadmap (for Architect)

| Slice | Intent | Notes |
|-------|--------|-------|
| **T002** | AgencyOffer domain contracts + ADR/sketch | Docs / API shapes; no UI theater |
| **T003** | Minimal Offer persistence + admin/agency create (honest) | No fake KPIs |
| **T004** | Public multi-offer selection on Tour detail | Trust + selection only |
| **T005** | Booking initiation carries Offer | Preserve Booking ≠ Payment |
| Later | Commission / settlement / approval workflows | After selection path works |

Exact Task-IDs remain Architect-authorized.

---

## 9. Acceptance for T001

PASS when:

- Commerce model chain documented
- AgencyOffer defined as direction (not implemented)
- Customer/Agency/Admin impacts named
- Domain ownership + future contracts listed
- Migration risks + roadmap recommendation recorded
- SoT updated; no product code changed
