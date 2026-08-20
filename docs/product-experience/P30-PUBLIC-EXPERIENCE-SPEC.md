# P30 — Public Marketplace Experience Spec

| Field | Value |
|-------|--------|
| Document | `docs/product-experience/P30-PUBLIC-EXPERIENCE-SPEC.md` |
| Status | **LOCKED** by `TC-P30-T002` |
| Audience | Traveler |
| Identity | Public Marketplace |
| Jobs | Discovery · Trust · Commerce Conversion |

---

## 1. Purpose

```text
Discovery + Trust + Commerce Conversion
```

The homepage must no longer resemble a developer landing page.

---

## 2. Global shell (eventual)

As appropriate:

- professional brand / header
- primary navigation
- travel product navigation
- account / auth entry
- locale handling
- search entry
- responsive mobile navigation
- professional footer

---

## 3. Home hierarchy

Required hierarchy intent (sections may be omitted when no valid data — empty handling must preserve quality):

1. Hero / primary travel intent
2. Primary travel search
3. Popular / inspiring destinations
4. Featured tours
5. Hotel discovery
6. Travel stories / UGC trust
7. Trust / value signals
8. Meaningful internal discovery
9. Professional footer

Curated composition — not personalized ML feed.

### Search rules

- clearly expose user intent
- not overwhelm the first viewport
- work on mobile
- keyboard accessible
- preserve locale
- respect Search ownership boundaries
- do not fake unimplemented capabilities

---

## 4. Hotel experience standard

### Listing (when backed by real capability)

- image-forward hotel cards
- hotel identity · location · star/category facts
- review/rating only when authoritative
- useful attributes · sort · filter · result count
- responsive listing · optional future map boundary

### Forbidden fakes

availability · room rates · inventory · discount · rating · review count

### Ownership

- Place = hotel catalog owner
- HotelBooking = booking / availability / rate / reservation owner

### Detail hierarchy

Gallery / visual identity · hotel name · stars/category · location · trusted rating/review when available · primary commercial CTA only when supported · overview · amenities · room/availability boundary when supported · location/map · reviews/UGC when supported · related hotels

Hotel Detail must be a **sales-quality product surface**, not a text document.

---

## 5. Tour experience standard

Preserve: **TourProduct ≠ TourDeparture**

### Listing

- image-forward cards
- clear destination · duration
- primary departure facts when relevant
- hotel / transport facts when authoritative
- clear monetary presentation · strong CTA · scan-friendly comparison

### Detail hierarchy

Hero/Gallery · Destination · Duration · primary product facts · departure information · flight/transport · hotel facts · price/CTA · itinerary · included · excluded · visa/important info where relevant · trust/UGC · related tours

Do not flatten TourProduct and TourDeparture into one UI concept if domain truth distinguishes them.

Tour / Pricing / Booking ownership unchanged.

---

## 6. Destination experience

Discovery hubs — not encyclopedia dumps.

May compose: strong imagery · overview · useful travel facts · tours · hotels · attractions/places · travel stories · internal links

Destination remains distinct from Place · Tour · SEO.

---

## 7. Trust & conversion

Allowed when real: UGC · verified content · real ratings · policy/support/secure payment facts · clear pricing · provenance where relevant

Forbidden: fake urgency · fake booking counters · fake “X users viewing” · fake discounts · fake ratings/reviews · fake scarcity · fake remaining rooms/seats · fake customer logos · fake commercial claims

---

## 8. Responsive / a11y / SEO

Desktop · Tablet · Mobile independently reviewed.

Critical mobile areas: header/nav · search · cards · filter/sort · hotel/tour detail · gallery · booking CTA

SEO is part of product experience (semantic headings, crawlable content, internal links, image semantics) while SEO module ownership remains centralized.

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial lock · `TC-P30-T002` |
