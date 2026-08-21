# P31 — Commercial Demo Experience — Foundation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P31-PLAN` (proposed by `TC-P31-T001`) |
| Track | Commercial Demo Experience (Post-DEMOFEED) |
| Status | **ACCEPTED** (`TC-P31-T001`) — next authorized unit via Architect file |
| Authority | Derived from P30 Constitution · Public Spec · P30-GATE · DEMOFEED-GATE |
| Product code in this task | **NO** (`TC-P31-T001` = planning / gap analysis only) |

---

## 1. P31 goal

Answer the customer-demo question:

> When a tourism agency opens TravelCore, do they feel a **professional travel commerce platform** worth buying — not a technical foundation skeleton?

**Success intent (phase):** Public marketplace surfaces (Home · Hotel · Tour · Destination composition) look and behave like a **sellable commercial demo**, while remaining honest about ownership boundaries (no fake rates/availability/bookings unless domain-backed).

**Non-goals:**

- Backend / domain redesign
- Turning DemoFeed into a permanent production module
- Fake production commercial claims
- Scraping / competitor copy
- Full Pricing/Booking enablement as a substitute for experience polish

---

## 2. Current state (accepted foundations)

| Layer | Status | Evidence |
|-------|--------|----------|
| Experience foundation (P30) | **FOUNDATION ACCEPTED** (known limitations) | `docs/product-experience/evidence/P30-GATE/GATE-REVIEW.md` |
| Data enablement (DEMOFEED) | **GATE ACCEPTED** | `docs/plans/DEMOFEED-GATE-acceptance-evidence.md` |
| Removable feeder | `tools/demofeed` · prefix `demofeed-*` | T002–T005 |
| Design system | Design System 2.0 + North Star | `docs/product-experience/` |

**Diagnosis (one line):** Architecture ✅ · Demo catalog foundation ✅ · **Sellable product experience ❌ incomplete**.

---

## 3. Gap analysis (Public)

### 3.1 Home experience

| Gap | Why it hurts a sales demo | Constraint |
|-----|---------------------------|------------|
| Hero still leans gradient/intent more than destination photography | First viewport fails “premium marketplace” test vs North Star | Prefer real Media assets / curated composition — no stock scraping |
| Search entry is discovery-intent UI, not a full booking engine | Agencies expect a strong trip-search feeling | Do not fake unimplemented Search/Booking capabilities |
| Destinations / Tours / Hotels bands historically thin or empty | Empty premium states were honest for P30; commercially weak for sales | DEMOFEED can feed catalog; Home must **compose** live/demo catalog |
| Trust band = capability honesty copy | Correct for architecture; weak for conversion trust (social proof, partners, guarantees) | Trust elements must not invent reviews/ratings |

### 3.2 Hotel discovery

| Gap | Why it hurts | Constraint |
|-----|--------------|------------|
| Live success grids historically blocked by API/connectivity / empty catalog | Listing looks like foundation + error/empty, not commerce | Place = catalog owner; use DEMOFEED hotels when API up |
| Cards lack sales-quality gallery density | Feels like CMS list, not hotel marketplace | Place ≠ HotelBooking — no fake rates/availability |
| Detail surface not yet “sales-quality product page” consistently | Agencies compare to booking.com-class density | Amenities/gallery/related hotels only when authoritative |
| Synthetic 1×1 DEMOFEED covers | Tiny/blank imagery destroys commercial feel | Needs **licensed / original demo media strategy** (not scrape) |

### 3.3 Tour discovery

| Gap | Why it hurts | Constraint |
|-----|--------------|------------|
| Destination-scoped listing contract | Hard to show a global “browse tours” marketplace | Tour ownership preserved; may need experience composition / authorized API surfacing — no domain redesign without ADR |
| Package-only DEMOFEED · no departures | Detail cannot show departure calendar / scarcity | Tour ≠ TourDeparture ≠ Pricing ≠ Booking |
| No price presentation when Pricing absent | Cards feel incomplete vs competitors | Prefer “price unavailable / request quote / from owner” honesty over fake Price rows |
| Itinerary / included / excluded thin | Product page feels empty | Content enrichment via owner write paths / DEMOFEED content strategy |

### 3.4 Content richness · trust · conversion

| Area | Gap | Direction |
|------|-----|-----------|
| Content richness | DEMOFEED labels are clearly sample; media synthetic | Professional demo content pack (copy + images) under `demofeed-*` identity |
| Trust | Weak partner/UGC/rating signals | Only compose when domain-backed; otherwise premium honest trust patterns |
| Conversion paths | CTAs exist but catalog→detail→book path often dead-ends honestly | Polish path to **supported** next actions; label unsupported Booking steps |

---

## 4. Relationship with DEMOFEED

DEMOFEED remains a **temporary removable feeder**, not a product module.

| Role | P31 expectation |
|------|-----------------|
| Feeder | Supply identifiable `demofeed-*` Destination / Place / Tour / Media rows |
| Experience | Consume owner public reads; compose commercial UI |
| Media upgrade | Prefer richer demo assets stored via Media owner path — still deletable |
| Purge | Still fail-closed until Architect-authorized cleanup |
| Forbidden | Register DemoFeed in Api; invent demofeed schema; scrape competitors |

**P31 may authorize DEMOFEED enrichment tasks** (richer media/copy/volume) via future `.task.md` files — not by inventing them here as executable work.

---

## 5. Required improvement themes

1. **Realistic demo content strategy** — destinations, hotels, tours, SEO-ready titles/descriptions, DEMOFEED labeling honesty
2. **Image / media strategy** — non-synthetic demo photography set; cover + gallery; local/S3 via Media contracts
3. **Homepage commercial composition** — hero + search prominence + live demo catalog bands + trust
4. **Hotel commerce polish** — listing/detail density without HotelBooking fakes
5. **Tour commerce polish** — listing/detail density without Pricing/Booking fakes
6. **Customer-facing priorities** — Public first; Admin/Agency only if needed for the sales demo story

---

## 6. Proposed P31 structure (task breakdown)

> Proposed only. **Do not execute** until Architect issues authorized `.task.md` / `.gate.md` files.

| Task | Type | Deliverable | Priority |
|------|------|-------------|----------|
| `TC-P31-T001` | Analysis | This plan + gap analysis | **DONE (this task)** |
| `TC-P31-T002` | Content | Professional Demo Content Strategy + DEMOFEED enrichment plan (volume, copy, media pack rules) | P0 |
| `TC-P31-T003` | Experience | Home Commercial Upgrade (compose DEMOFEED catalog · hero/search · trust) | P0 |
| `TC-P31-T004` | Experience | Hotel Commerce Polish (listing + detail sales-quality composition) | P0 |
| `TC-P31-T005` | Experience | Tour Commerce Polish (listing + detail sales-quality composition) | P0 |
| `TC-P31-T006` | Optional | Destination hub polish (compose hotels/tours for demo cities) | P1 |
| `TC-P31-GATE` | Gate | Customer-demo question: **yes / no with known limitations** + visual evidence vs North Star | Gate |

Suggested execution order: **T002 → T003 → T004 → T005 → (T006) → GATE**.

Rationale: richer content/media first, then Home (first impression), then Hotel/Tour depth.

---

## 7. Acceptance criteria (phase-level)

### Product / visual

- [ ] A tourism-business viewer can open Public Home and recognize a **travel marketplace**, not a foundation landing page
- [ ] Hotel listing/detail show **image-forward catalog commerce** using real demo Place/Media rows
- [ ] Tour listing/detail show **package product commerce** using real demo Tour/Media rows
- [ ] Visual evidence captured vs North Star for Home · Hotel · Tour (desktop + mobile)
- [ ] Empty/error states remain premium when APIs fail — never fake success inventory

### Architecture

- [ ] Modular Monolith + ownership unchanged
- [ ] Place ≠ HotelBooking · Tour ≠ Pricing/Booking · DemoFeed ≠ production module
- [ ] No scraping · no competitor copy · no fake production claims
- [ ] DEMOFEED rows remain identifiable (`demofeed-*`) and removable by strategy

### Honesty rules

- [ ] No invented rates / availability / ratings / review counts
- [ ] Unsupported booking steps clearly non-authoritative or omitted
- [ ] DEMOFEED clearly demo/non-production where labeling is required

---

## 8. Visual / product quality criteria

Aligned with `P30-VISUAL-ACCEPTANCE-CHECKLIST.md` + North Star, raised for **sales demo**:

| Dimension | P31 bar |
|-----------|---------|
| First impression | Marketplace density, not sparse skeleton |
| Imagery | Real demo photography (not 1×1 PNG placeholders) |
| Card composition | Image · identity · location/meta · CTA — scan-friendly |
| Detail pages | Gallery-forward product surfaces |
| Trust | Credible capability / policy / content trust — not fake social proof |
| Mobile / RTL | Same commercial feeling as desktop FA RTL |

---

## 9. Architecture constraints (locked)

Preserve:

- Modular Monolith
- Existing domain ownership (Destination · Place · Tour · Media · …)
- Place ≠ HotelBooking
- Tour ≠ Pricing / Booking / Departure ownership
- DemoFeed as development/demo data layer only

Forbidden in P31:

- Backend redesign / domain redesign without ADR
- Fake production claims
- Competitor copying / scraping
- Api registration of DemoFeed as `ITravelCoreModule`

---

## 10. Out of scope (unless future Architect envelope)

- Full HotelBooking availability/rate integration as “fake-filled” demo
- Full Pricing engine population for tours
- Payment / Booking production flows
- Admin/Agency deep feature builds (unless required for the sales story)
- DEMOFEED purge execution (separate authorized cleanup)

---

## 11. Recommended next (Architect)

Authorize **`TC-P31-T002`** (Professional Demo Content Strategy) as the first implementation-planning/content unit — or adjust task IDs via new `.task.md` files.

Pipeline must **not** auto-start T002+.

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Initial plan from `TC-P31-T001` envelope (docs only) |
