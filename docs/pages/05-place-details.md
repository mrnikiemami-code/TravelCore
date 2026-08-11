# Place Details — Hotel · Restaurant · Attraction

**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)

Place subtypes share Place ownership but **must not** share one identical domain meaning or one universal detail page. Three contracts below.

Cross-state: [`09-page-state-and-composition-rules.md`](09-page-state-and-composition-rules.md)

---

# A. Hotel Detail (Catalog)

**Archetype:** `HotelDetailPage`

## Purpose

Canonical Hotel catalog owned by **Place**. **Not** live HotelBooking search/offer page.

## Primary User Intent

Understand the hotel as a travel place and discover relevant travel/booking options.

## Secondary User Intents

Amenities · Location · Related tours · Nearby attractions · Content/UGC · Live availability CTA when integrated.

## Primary CTA

View availability / Book **only when** HotelBooking integration exists and healthy; otherwise explore-related / contact / destination — do not fake live bookability.

## Secondary CTA

Related Tours · Destination · Nearby places.

## Target Modules

Place.Hotel · Destination · Tour (related) · Media · SEO · optional HotelBooking · optional UGC/Content.

## Required / Optional Data

Required: hotel identity · destination/location · locale publication · status. Optional: gallery · amenities · map · room catalog facts · policies · related tours · UGC · live offers.

## Content Priority

Decision Critical: identity · classification · location · summary · CTA state honesty. Supporting: amenities · map. Secondary: UGC · related content.

## Page Anatomy

Breadcrumb · Hero/Gallery · Name · Destination/location · Star/category (when authoritative) · Summary · Amenities · Map · Room/catalog info · Policies · Related Tours · Nearby Attractions · Content/UGC · Live-booking area **only when integrated**.

## Provider Outage (critical)

Hotel catalog page **must not disappear** because live booking provider is unavailable.

| Concern | Effect of HotelBooking outage |
|---------|-------------------------------|
| Canonical Hotel identity | Unaffected — page remains |
| Live availability / booking CTA | Degraded / unavailable messaging |
| Place facts | Still from Place |

**Illustrative:** Hotel Catalog continues to exist when HotelBooking provider availability is temporarily unavailable.

## Above-the-Fold

Name · location · classification · primary media · honest CTA state.

## Desktop / Tablet / Mobile

Gallery + facts; mobile single column; map enhancement not required for basic info.

## RTL / LTR / Bidi

Logical; addresses/phones LTR-safe; maps not mirrored.

## Loading / Empty / Error / Unavailable

Gallery skeleton. Empty related tours OK. Core hotel missing = not found. Booking fail = partial. Hotel closed/unavailable: status, not silent delete.

## Accessibility

Alt gallery · headings · map alternative · CTA disabled state announced.

## SEO Role

Potential / Primary Indexable (direction).

## Internal Linking

→ Destination · Related Tours · Nearby Attractions · Articles.

## Structured Data Candidates

`Hotel` · `BreadcrumbList` · `AggregateRating` **only with real eligible data**.

## Performance Risks

Large gallery · map · live offer widgets. LCP: hero.

## Analytics Intent

`AvailabilityViewed` · `RelatedTourOpened` · `BookingCtaClicked`.

## Explicit Non-Goals

Full live search UX (later) · inventing reservation.

## Responsive Matrix (Hotel)

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Gallery | Multi | Compact | Swipe | Not mirrored | Alt |
| Amenities | Grid | Grid | List | — | Lists |
| Booking CTA | Side | Side/below | Sticky when live | Logical | Disabled clarity |

---

# B. Restaurant Detail

**Archetype:** `RestaurantDetailPage`

## Purpose

Place-owned restaurant understanding for travel relevance.

## Primary User Intent

Understand whether the restaurant is relevant during travel.

## Primary CTA

Open map / Save-or-related destination (no invented reservation commerce).

## Secondary CTA

Nearby places · Destination · UGC.

## Target Modules

Place.Restaurant · Destination · Media · SEO · optional UGC.

## Required / Optional

Identity · location · locale. Optional: cuisine · hours · contact · map · reviews · nearby.

## Anatomy

Identity · destination/location · media · cuisine/category · description · hours (authoritative) · contact · map · reviews/UGC · nearby · related destination.

## Do not invent reservation commerce if not implemented.

## SEO Role

Potential Indexable.

## Structured Data Candidates

`Restaurant` · `BreadcrumbList` · `Review` (real data only).

## Performance / A11y / States

Same pattern as Place: core fail = not found; optional UGC degrade; map enhancement.

## Responsive Matrix (Restaurant)

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Identity | Header | Stack | Stack | Logical | H1 |
| Hours/contact | Side facts | Stack | Stack | Phone LTR | Definitions |
| Map | Enhancement | Same | Same | Not mirrored | Text address fallback |

---

# C. Attraction Detail

**Archetype:** `AttractionDetailPage`

## Purpose

Understand and plan visiting a point of interest (Place-owned).

## Primary User Intent

Understand and plan visiting a point of interest.

## Primary CTA

Plan visit / Open related tours / Destination.

## Secondary CTA

Nearby attractions · Content · UGC.

## Target Modules

Place.Attraction · Destination · Tour (related) · Content · UGC · Media · SEO.

## Anatomy

Identity · destination · media · description · category · location · visit guidance · hours/pricing (authoritative) · related tours · nearby · content · UGC.

## SEO Role

Potential Indexable.

## Structured Data Candidates

`TouristAttraction` · `BreadcrumbList`.

## Responsive Matrix (Attraction)

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Visit guidance | Main | Main | Main early | Logical | Clear lists |
| Related tours | Rail | Grid | Stack | — | Links |
| Map | Enhancement | Same | Same | Not mirrored | Address fallback |

## Shared Place Notes

- RTL/LTR/bidi: shared Place rules  
- Loading/empty/error: core vs secondary  
- Social proof: real eligible data only  
- UGC enhances; not authoritative for business facts  
- Anti-pattern: one component representing every Place subtype identically  

## Reference Sites

**Reference evidence incomplete** for dedicated Hotel/Restaurant/Attraction wireframes in registry — archetypes defined from Place domain + architecture.
