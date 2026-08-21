# P38 — AgencyOffer Contracts

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T002` |
| Phase | P38 — Multi-Agency Commerce |
| Date | 2026-08-21 |
| Type | Contract / architecture planning (no persistence · no UI) |
| Depends-On | `TC-P38-T001` ACCEPTED |

## 1. AgencyOffer responsibility

**AgencyOffer** is the commercial proposal of an **Agency Party** over a **TourProduct** (optionally scoped to one or more **TourDeparture** ids).

| It is | It is not |
|-------|-----------|
| A sellable commercial path for customer selection | A TourDeparture (schedule/inventory) |
| An input to Pricing/Quote when selected | A Price or Quote itself |
| Referenced by Booking initiation | A Booking |
| Bound to Party/Access for management | A Payment or settlement row |

### Ownership

| Concern | Owner |
|---------|-------|
| Offer identity + lifecycle status | Future **AgencyOffer** capability (contract first; module placement Architect-confirmed at persistence) |
| TourProduct / TourDeparture facts | **Tour** |
| Agency identity / relationship | **Party** |
| Who may create/publish for an agency | **Access** |
| Quote computation | **Pricing** |
| Booking lifecycle | **Booking** (stores offer reference) |
| Payment lifecycle | **Payment** (unchanged) |

### Lifecycle direction (states — not implemented)

```text
Draft → Submitted → Published → Suspended → Retired
         ↘ Rejected (admin)
```

Published offers are the only ones eligible for public customer selection (unless channel = private).

---

## 2. Future contracts (shapes — not APIs)

### 2.1 Create offer (Agency Portal / Admin)

```text
CreateAgencyOfferCommand
  agencyPartyId
  tourProductId
  departureScope: ALL | LIST[departureId]
  channel: Public | AgencyPortal | Private
  termsRef?          // content/policy pointers — not invented copy
  pricingPolicyRef?  // points into Pricing ownership — not inline fake amounts
  commissionPolicyRef? // direction only
```

### 2.2 Publish offer

```text
PublishAgencyOfferCommand
  offerId
  actorAccessContext
→ requires Access permission + (optional) Admin approval policy later
```

### 2.3 Retrieve offers (Public / Agency / Admin)

```text
ListAgencyOffersQuery
  tourProductId?
  agencyPartyId?
  status?
  channel?
→ returns OfferSummary[] (ids, agencyPartyId, productId, status, channel)
  // NEVER invent counts or “12 competing agencies”
```

### 2.4 Customer selection

```text
SelectedAgencyOffer
  offerId
  tourProductId
  agencyPartyId
  (optional) preferredDepartureId
→ carried into Quote request / Booking initiation
```

Selection ≠ Booking. Selection ≠ Payment.

---

## 3. Domain ownership confirmation

| Module | Owns | Offer relationship |
|--------|------|--------------------|
| Tour | TourProduct, TourDeparture | Offer **references**; does not duplicate itinerary |
| Party/Agency | Agency identity | Offer **belongs to** agency party |
| Pricing | Quote, price calculation | Offer may supply policy refs; Quote remains Pricing |
| Booking | Booking aggregate | Initiation accepts `agencyOfferId` when multi-offer path used |
| Payment | Payment | Unchanged; Booking ≠ Payment |

---

## 4. Attribute direction (no business values invented)

| Attribute | Direction |
|-----------|-----------|
| agency | Required Party id |
| tour product | Required TourProduct id |
| optional departure scope | ALL or explicit departure ids |
| commercial terms | References / structured fields later — not fake marketing bullets |
| availability direction | Still Tour/Inventory; Offer must not invent seats |
| sales channel | Public / AgencyPortal / Private |
| commission direction | Policy reference only until settlement phase |
| status lifecycle | Draft…Published…Retired as above |

---

## 5. Compatibility

- Existing **single-path** public booking (product → departure → prepare → pending) remains valid.
- When **0–1** published Public offers: UI may omit multi-offer chooser.
- When **≥2** published Public offers: customer selection step required before Quote/Booking.
- Do **not** require every historical booking to backfill Offer ids until a migration task exists.

---

## 6. Risks

| Risk | Anti-pattern | Guard |
|------|--------------|-------|
| Offer = Departure | Cloning schedule into offer | Keep departure as Tour fact; Offer only scopes |
| Offer = Price | Storing traveler price on Offer as SoT | Pricing owns Quote |
| Offer = Booking | Creating booking on “select offer” | Explicit initiation API |
| Premature settlement | Building commission ledger in T003 | Slice: publish → select → book first |

---

## 7. Acceptance for T002

PASS when contracts, ownership, compatibility, and risks are documented; SoT updated; **no** persistence/UI code.
