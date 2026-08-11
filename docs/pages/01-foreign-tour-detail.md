# Foreign Package Tour Detail — Page Archetype

**Archetype:** `ForeignTourDetailPage`  
**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)  
**Philosophy:** [`../architecture/13-reference-page-archetypes.md`](../architecture/13-reference-page-archetypes.md)

Cross-state rules: [`09-page-state-and-composition-rules.md`](09-page-state-and-composition-rules.md)

---

## Purpose

ارائهٔ پایدار یک **TourProduct** پکیج خارجی تا کاربر بتواند مناسب‌بودن تجاری را بفهمد و اقدام کند.

## Primary User Intent

Understand whether this travel package is suitable and commercially actionable.

## Secondary User Intents

- Compare hotel options  
- Understand flight schedule  
- Understand price (components / occupancy)  
- Inclusions / exclusions  
- Visa / passport / policy requirements  
- Agency / provider trust  
- Choose departure / rate  
- Begin booking / contact flow  

## Primary CTA

Select / Book / Contact (validated by backend; UI CTA is not authority).

## Secondary CTA

View related tours · Open destination · Open hotel catalog · Contact agency (when distinct).

## Target Resources / Modules

| Role | Module |
|------|--------|
| Composition root | Tour (`TourProduct`) |
| Hotel facts | Place (`Hotel`) via reference |
| Destinations | Destination |
| Price/rate presentation | Pricing contracts |
| Media | Media |
| Agency/seller | Identity / Agency context (as owned) |
| SEO | SEO module route/meta |
| Optional UGC | UGC |

Tour **does not own** Hotel catalog.

## Required Data

- Localized TourProduct identity + title  
- Destination context  
- Duration  
- At least one media or explicit placeholder policy  
- Departure / availability summary (may be empty set)  
- Commercial status (active / no departure / expired / unavailable)  
- SEO route resolution for locale  

## Optional Data

- Flight segments  
- Hotel options list  
- Multi-component prices  
- Itinerary / stay plan  
- Policies, requirements  
- Related tours  
- Maps / widgets  
- Reviews  

## Content Priority

| Priority | Sections |
|----------|----------|
| Decision Critical | Header/commercial summary · Departure/availability · Price context · Primary CTA · Hotel options (when package hinge) · Flight summary |
| Important Supporting | Services in/out · Requirements · Policies · Agency |
| Secondary Discovery | Related tours · Destination discovery · UGC |

## Page Anatomy (conceptual order)

A. Breadcrumb  
B. Product Header / Hero — title, destination, duration, primary media, agency context, commercial summary  
C. Departure / Availability Summary  
D. Flight / Transport Summary  
E. Hotel Options  
F. Pricing / Occupancy  
G. Services (included / excluded)  
H. Travel Requirements  
I. Cancellation / Commercial Policies  
J. Itinerary / Stay Plan (where applicable)  
K. Agency / Seller Information  
L. Related Tours / Destination Discovery  
M. Booking / Lead CTA  

Exact visual order may adapt; decision-critical before secondary.

## Above-the-Fold

Prioritize: title + destination + duration + commercial status + price context (or explicit unavailable) + primary media + path to CTA. Avoid huge decorative banner before product facts.

## TourProduct vs TourDeparture

صفحه عمدتاً **TourProduct** را نمایندگی می‌کند. Departures/rates نمونه‌های تجاری قابل انتخاب‌اند. هر Departure صفحهٔ indexable جدا ندارد مگر SEO route صریح (ADR 0010).

## Flight Segment Presentation

May include: origin/destination airport codes · carrier · flight number · local dep/arr date-time · time zones · class · baggage.

Values like `IKA`, `IST`, `EK978` / `TK875` are **bidi-sensitive**. Travel direction must **not** be blindly mirrored in RTL.

## Hotel Options

`TourHotelOption` references Place `Hotel`. Page may show: hotel identity · star/category · room/occupancy context · meal plan · nights · relative commercial price · media/summary.

Hotel catalog facts ← Place. Package-specific facts ← Tour.

## Pricing

Support: `PassengerCategory` · `Occupancy` · `PriceComponents[]`.

Do **not** visually imply automatic sum of mixed currencies. Toman only as explicit display/input per Money policy. Display price ≠ Quote.

If price unavailable: do not fabricate `0` or fake “starting at”.

## Desktop Behavior

Main content + commercial/booking summary area. Sticky panel allowed when useful; page readable without it; sticky must not own unique inaccessible content.

## Tablet Behavior

May keep split or collapse summary under header; filters/options remain touch-friendly; no hover-only critical actions.

## Mobile Behavior

Single-column. Persistent primary booking/action CTA where useful. Commercial details may open Bottom Sheet / Sheet / dedicated step later. Do **not** squeeze desktop pricing/hotel tables. Adapt to semantic cards/rows.

## RTL / LTR Notes

Layout uses start/end. Semantic LTR values: airport codes, flight numbers, currency codes, emails, phones, booking refs. Maps/media not auto-mirrored.

## Bidi-Sensitive Values

Persian title + English airline + `IKA`/`IST`/`TK875` + `USD` + numeric price + date/time in one realistic context (see illustrative example).

## Loading State

Stable skeleton for hero, summary, hotel list, pricing. Prefer server-rendered critical identity. Avoid full-page collapse/re-expand. Secondary sections may stream independently.

## Empty State

Empty hotel list / empty related tours = subsection empty, not page empty.

## Error State

Core TourProduct unresolved → page-level not-found/error per SEO route rules. Secondary projection failure (e.g. related articles) → degrade section.

## Unavailable / Expired State

Expired Tour ≠ automatic 404. Support: Expired · No active departure · Temporarily unavailable. UX: clear status · disable misleading CTA · related/newer options · retain useful product/destination info when policy permits.

## Accessibility Notes

Heading hierarchy · landmarks · keyboard to CTA and hotel/rate selection · focus management for sheets · alt for primary media · status announcements for availability/price changes. See UI a11y constitution; risks: dense tables, sticky overlay focus trap.

## SEO Role

**Primary Indexable Resource** (default direction). Final IndexPolicy route-specific. Component type alone does not decide index/noindex.

## Indexability Direction

Stable TourProduct routes may be indexable when published. Departures/rates not automatically indexable pages.

## Canonical / Localized Route Relationship

One archetype; localized routes per published locale. Missing AR publication → no fabricated AR page (ADR 0008).

## Internal Linking

→ Destination · → Hotel catalog · → Attractions places if referenced · → Related tours · → Visa/content when relevant. No crawler-only link clouds.

## Structured Data Candidates

`BreadcrumbList` · `Product` · `Offer` · `TouristTrip` (candidate) · Organization/seller. Label: **Candidate** — truthfulness rules apply.

## Performance Risks

Large hero/gallery · many hotel options · many rates/departures · mixed structured data · booking Client Island · map/flight viz. Keep initial SSR useful without hydrating whole page. LCP risk: hero image.

## Analytics Intent (conceptual)

`HotelOptionViewed` · `RateSelected` · `BookingStarted` · `RelatedTourOpened`. Not vendor-specific schemas.

## Future Implementation Notes

Purpose-built page VM; Server Components + booking island; compose via module read contracts.

## Explicit Non-Goals

Checkout wizard · payment · live flight search · Admin · exact sticky px · visual brand.

---

## Responsive Behavior Matrix

| Major Element | Desktop | Tablet | Mobile | RTL/LTR | Accessibility |
|---------------|---------|--------|--------|---------|---------------|
| Hero / header | Wide media + summary | Stacking begins | Single column; media first or beside title per design later | Layout logical | H1 once; alt on primary image |
| Commercial panel | Side / sticky | Compact side or below | Persistent CTA + sheet | CTA at start-edge | Focus trap in sheet |
| Flight row | Table/row | Compact rows | Cards; codes remain LTR | Do not mirror IKA→IST | Announce times with timezone |
| Hotel options | Compare list/table | Cards | Cards | Logical | Keyboard select |
| Pricing | Multi-component visible | Same | Sheet/expand | Currency codes LTR | Live region on rate change |
| Related | Below fold | Below | Far below decision content | — | Skip link optional |

---

## Illustrative Example (NOT production data)

**Label: Illustrative example.**

- Destination: Istanbul  
- Origin: Tehran  
- Flight: `IKA` → `IST` · `TK875`  
- Departure: local Tehran time · Arrival: local Istanbul time  
- Pricing: `1290 USD` + `119,900,000 IRR` (not auto-summed)  
- Occupancy examples: Double · Single · ChildWithBed  
- Hotel options (≥2 conceptual): e.g. Hotel A 4★ BB · Hotel B 5★ HB  

### Bidi illustration (Persian RTL page)

On a `fa` RTL layout, the following remain readable LTR islands: `IKA`, `IST`, `TK875`, `USD`. Direction of travel `IKA → IST` is semantic, not mirrored to `IST → IKA`. Do not inject unsafe Unicode bidi controls in docs; implementation uses accepted bidi utilities later.

### Page-state examples

| Situation | Classification |
|-----------|----------------|
| Tour exists + active departure | Normal |
| Tour exists + no active departure | Unavailable (commerce) — page may remain |
| Tour exists + expired | Unavailable/Expired — not auto-404 |
| Tour exists + secondary hotel projection failure | Partial degradation |
| Tour route nonexistent | Not found (core failure) |

---

## Reference Sites

| Source | Useful pattern | TravelCore | Must NOT copy |
|--------|----------------|------------|---------------|
| REF-LS-001 | Package anatomy: flight + hotels + multi-currency | Anatomy + pricing honesty | Brand/UI/text/assets |

Reference behavior alone ≠ requirement.
