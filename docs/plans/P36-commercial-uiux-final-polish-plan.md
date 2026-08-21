# P36 — Commercial UI/UX Final Polish Plan

| Field | Value |
|-------|--------|
| Task-ID | `TC-P36-T001` |
| Date | 2026-08-21 |
| Evidence | [`docs/product-experience/evidence/P36-T001/`](../product-experience/evidence/P36-T001/) |
| Current verdict | **`NOT_SELLABLE_VISUALLY`** |

## Sales-demo gate

> Would a prospective travel-business customer pay for this product based on what they see?

**Today: No.** Target for P36-GATE: **Yes** (honest catalog, premium perception).

---

## Implementation sequence

| Task | Focus | Acceptance (summary) |
|------|-------|----------------------|
| **TC-P36-T002** | Home final commercial redesign/polish | Photographic/editorial hero; destination merchandising; DEMOFEED demotion; first impression premium without fake inventory |
| **TC-P36-T003** | Hotel listing/detail polish | Fix `undefined` search; card/gallery consistency; empty facilities/reviews as premium empty states; DEMOFEED badge treatment |
| **TC-P36-T004** | Tour listing/detail polish | Marketplace-first browse (or guided destination picker that looks commercial); hide UUID leaks; itinerary/price hierarchy polish |
| **TC-P36-T005** | Commerce/booking visual polish | Productized pending booking UI (summary card, imagery, progress); not admin form; keep Payment honesty labels |
| **TC-P36-T006** | Global design-system consistency | Radius/type/elevation/button/chip pass vs DESIGN-SYSTEM-2.0; Iran Sans path if licensed |
| **TC-P36-T007** | Mobile-first hardening | 360–430px density, sticky CTAs, no overflow, tap targets |
| **TC-P36-T008** | Final visual evidence pack | Desktop+mobile screenshots for Home/Hotel/Tour/Commerce vs North Star gaps closed |
| **P36-GATE** | Sellability gate | Architect answers the sales-demo question honestly |

Adjust only with Architect authorization.

---

## Per-task acceptance sketches

### T002 Home
- Hero is photo-led or rich editorial (not flat blue only)
- Destinations/hotels/tours merchandising feels commercial
- Sample/demo labeling not the primary visual story
- No fake prices/availability

### T003 Hotels
- Search never shows `undefined`
- Cards have consistent aspect, typography, CTA
- Detail gallery + summary feel bookable without inventing rates

### T004 Tours
- Entry path does not feel like a developer tool requiring slug knowledge
- No raw UUID as primary destination label
- Departure/price blocks scannable and premium

### T005 Commerce
- Booking prep shows product context (hotel/tour summary)
- Visual language matches marketplace chrome
- Honesty about Pending / non-payment preserved

### T006–T008 / GATE
- DS consistency + mobile + evidence + gate verdict

---

## Explicit do-not-regress

- no fake prices  
- no fake availability  
- no fake reviews  
- no fake payment success  
- preserve RTL / mobile / accessibility  
- preserve Booking/Payment architecture boundaries  
- do not clone competitor trade dress  

---

## Separate governance (not P36 scope)

Recommend Architect task: **cleanup accidentally tracked `docs/pipeline/inbox/` transport stubs** (46 + housekeeping).

---

## Recommended next authorized task

**`TC-P36-T002` — Home final commercial redesign/polish**
