# P31 — Professional Demo Content Strategy

| Field | Value |
|-------|--------|
| Document | `docs/plans/P31-demo-content-strategy.md` |
| Task-ID | `TC-P31-T002` |
| Phase | P31 — Commercial Demo Experience |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Product code | **NO** (strategy docs only) |
| Companion plan | [`P31-commercial-demo-experience-plan.md`](P31-commercial-demo-experience-plan.md) |
| Feeder plan | [`DEMOFEED-implementation-plan.md`](DEMOFEED-implementation-plan.md) |

---

## 1. Purpose

Define how TravelCore **demo content** must be shaped so a tourism company demo answers:

> Can we show this and make them believe this is a professional travel commerce product?

This strategy governs **what** content exists and **how** it must behave.  
It does **not** authorize UI implementation (that is T003+).

---

## 2. Principles

1. **Commercial feeling without fake commerce** — rich catalog presentation ≠ invented rates/availability/reviews.
2. **Owner paths only** — Destination / Place / Tour / Media write & read ownership unchanged.
3. **DEMOFEED remains temporary** — identifiable, deterministic, replaceable, purgeable.
4. **Clearly demo** — titles/descriptions may include DEMOFEED / Sample markers where needed; never claim production inventory.
5. **No scrape / no competitor copy** — original or properly licensed demo assets only.
6. **Experience consumes owners** — Public UI composes public reads; DemoFeed is not a UI module.
7. **Determinism** — stable `demofeed-*` codes/slugs; idempotent seeds.
8. **Honesty over density** — if a fact is unsupported (price, room rate, review count), omit or label — never fabricate as real.

---

## 3. Demo Content Model

### 3.1 Categories

| Category | Owner | Demo role | Minimum commercial pack (proposed) |
|----------|-------|-----------|--------------------------------------|
| Destinations | Destination | Hierarchy + discovery hubs | ≥2 countries · ≥2 cities · optional region |
| Hotels (Places) | Place | Catalog hotels for listing/detail | ≥4 hotels across demo cities (2+ per primary city preferred) |
| Tours | Tour | Package products for listing/detail | ≥4 packages across demo cities |
| Media | Media | Covers + galleries | Cover per hotel/tour · ≥3 gallery images per detail entity |
| Descriptions | Owner translations | Sales-quality copy | FA primary · EN secondary · AR optional |
| Highlights | Owner fields / composed bullets | Scan-friendly selling points | 3–6 highlights per hotel/tour |
| Trust elements | Experience composition | Conversion trust | Policy/capability bands · **no fake ratings/reviews** |

### 3.2 Identity rules

| Rule | Requirement |
|------|-------------|
| Prefix | All demo entities use `demofeed-` code/slug family |
| Naming | Human-readable DEMOFEED Sample titles allowed |
| Replaceability | Deleting by prefix/strategy restores pre-demo catalog |
| No new SoR schema | No `demofeed` tables / domain columns without ADR |

### 3.3 Explicit non-content (forbidden as “real”)

- Fake availability / room inventory
- Fake prices presented as live rates
- Fake reviews / ratings / review counts presented as UGC truth
- Scraped competitor photography or copy
- Production-partner claims that are not true

---

## 4. Content Quality Rules

### 4.1 Images

| Rule | Requirement |
|------|-------------|
| Resolution | Prefer ≥1600px wide hero/cover candidates; thumbnails derived by Media pipeline |
| Aspect | Consistent card ratio (e.g. 3:2 or 16:9) across hotels/tours |
| Subject | Travel-appropriate: exteriors, destinations, experiences — not logos-only |
| Reject | 1×1 PNG placeholders · broken URLs · watermarked competitor stock |
| Storage | Via Media upload/validation contracts; local demo storage OK for operator demos |
| Gallery | Detail pages need **multiple** assets, not only a single cover |
| Attribution | Keep provenance notes in feeder docs (license / original / generated-for-demo) |

### 4.2 Text

| Rule | Requirement |
|------|-------------|
| Tone | Professional travel commerce — concise, benefit-oriented |
| Length | Card: short blurb · Detail: overview paragraph + bullets |
| Facts | Only state amenities/duration/location when owner fields support them |
| Labels | Prefer “DEMOFEED Sample …” for demo identity clarity in Admin; Public may soften if Architect allows labeled demo mode |
| No hype lies | Avoid “best price guaranteed” / invented scarcity |

### 4.3 Localization

| Locale | Role |
|--------|------|
| `fa` | Primary demo locale (RTL sales narrative) |
| `en` | Secondary for bilingual demos |
| `ar` | Optional stretch — only if translations maintained |

All demo entities used on Public must have at least FA translation; EN strongly recommended for agency sales demos.

### 4.4 SEO considerations

- Stable public slugs under `demofeed-*` or humanized slugs still prefixed/identifiable
- Unique titles/meta descriptions per entity (no duplicated boilerplate across all hotels)
- Destination hubs link hotels/tours without keyword stuffing
- Do not invent schema.org AggregateRating without real data

### 4.5 Consistency

- Same city naming across Destination / Place destinationId / Tour destination links
- Shared visual language (warm travel photography, not random unrelated stock)
- Card fields aligned: image · title · location · meta · CTA
- DEMOFEED inventory countable via feeder `list` commands

---

## 5. Experience Requirements (what T003–T005 need from content)

### 5.1 Home (`TC-P31-T003`)

Needs from content:

- Featured destinations with strong cover imagery
- Featured tours (cards with covers + short blurbs)
- Featured hotels (cards with covers + location)
- Enough density to fill Home bands without empty premium placeholders

Does **not** need: live prices, availability, fake social proof.

### 5.2 Hotel listing (`TC-P31-T004`)

Needs:

- Multiple hotels with covers
- Location/destination linkage
- Category/star only if Place fields set authoritatively
- Filterable attributes only when real

### 5.3 Hotel detail (`TC-P31-T004`)

Needs:

- Gallery (≥3 images)
- Overview + highlights/amenities when present
- Location context
- Related hotels in same city when available
- Primary CTA only to **supported** next steps (catalog → book path honesty)

### 5.4 Tour listing (`TC-P31-T005`)

Needs:

- Multiple published packages with covers
- Destination + duration meta when available
- Browse composition that works with current Tour contracts (destination-scoped or authorized listing path)

### 5.5 Tour detail (`TC-P31-T005`)

Needs:

- Hero/gallery
- Overview · highlights · included/excluded when modeled
- Clear separation: product facts vs missing departure/price (honest empty modules)
- Related tours when available

### 5.6 Destination pages

Needs:

- Destination cover + overview
- Composed hotels/tours for that destination from DEMOFEED
- Hub navigation — not encyclopedia dump

---

## 6. Data Boundary Rules

| Boundary | Rule |
|----------|------|
| Place ≠ HotelBooking | Demo hotels are **catalog Places**. No fake room rates/availability/reservations. |
| Tour ≠ Pricing/Booking | Demo tours are **TourProducts**. No fake Price rows / bookings. |
| Tour ≠ TourDeparture | Departures optional future; do not invent calendars as real inventory. |
| DemoFeed ≠ Production Domain | Feeder stays in `tools/demofeed`; not `ITravelCoreModule` in Api. |
| Pricing / Booking ownership | Unchanged — experience may show “price on request / unavailable” patterns only. |

### Forbidden

- Scraping competitor websites
- Copying competitor content
- Fake availability
- Fake prices presented as real
- Fake reviews presented as real

---

## 7. Priority order (content work)

1. **Media pack upgrade** — replace synthetic 1×1 covers with real demo imagery + galleries  
2. **Copy enrichment** — FA/EN overviews & highlights for existing `demofeed-*` hotels/tours/destinations  
3. **Volume uplift** — grow to ≥4 hotels and ≥4 tours for marketplace density  
4. **Destination hub readiness** — ensure city destinations compose linked catalogs  
5. **Trust content** — experience-level trust bands (non-fake) coordinated with T003  

Executable enrichment requires future Architect-authorized DEMOFEED/content `.task.md` files (may be bundled into T003 prep or separate feeder tasks).

---

## 8. Relationship with DEMOFEED

| DEMOFEED today | P31 content strategy |
|----------------|----------------------|
| T002–T005 foundation seeds | Base inventory exists but commercially thin |
| Synthetic PNG covers | Must be upgraded under Media owner path |
| Purge fail-closed | Remains until authorized cleanup |
| Prefix `demofeed-*` | Remains the identity contract |

**Strategy does not auto-authorize purge or new seeds.** Future tasks may authorize:

- `seed` enrichment (media/copy/volume)
- optional labeled “demo mode” UX copy
- still-removable feeder changes

---

## 9. Acceptance criteria (for this strategy document)

- [x] Principles defined
- [x] Content categories + minimum pack proposed
- [x] Image/text/locale/SEO/consistency rules defined
- [x] Home/Hotel/Tour/Destination content needs mapped
- [x] Architecture boundaries + forbidden list explicit
- [x] Priority order for enrichment stated
- [x] DEMOFEED relationship clear
- [ ] Architect ACCEPT of `TC-P31-T002` (pending)

---

## 10. Recommended next authorized task

**`TC-P31-T003` — Home Commercial Upgrade** (after Architect ACCEPT of T002),  
optionally preceded by an Architect-authorized DEMOFEED media/copy enrichment task if imagery must land before Home composition.

Do **not** auto-start without `.task.md` / `.gate.md`.

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Initial strategy from `TC-P31-T002` (docs only) |
