# Experience Tour Detail — Page Archetype

**Archetype:** `ExperienceTourDetailPage`  
**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)

Do **not** mechanically reuse Foreign Package Tour layout. Structure differs: experience understanding + itinerary timeline dominate over hotel-option packages.

Cross-state: [`09-page-state-and-composition-rules.md`](09-page-state-and-composition-rules.md)

---

## Purpose

ارائهٔ یک تجربهٔ سفر ساخت‌یافته تا کاربر بفهمد آیا این تجربه با او سازگار است.

## Primary User Intent

Understand the actual travel experience and determine whether it fits the traveler.

## Secondary User Intents

Assess difficulty/eligibility · Inspect day-by-day plan · Equipment · Meals/accommodation · Safety · Policies · Related experiences · Start book/contact.

## Primary CTA

Book / Request / Contact (backend-validated).

## Secondary CTA

Related experiences · Destination · Attraction links.

## Target Resources / Modules

Tour (Experience) · Destination/Place references · Media · SEO · optional Pricing · optional UGC. Stops may **reference** Attraction/Destination without ownership transfer.

## Required Data

Localized title · experience summary · duration · commercial/status · SEO locale publication.

## Optional Data

Difficulty · eligibility · meeting/origin · day itinerary · stops · meals · accommodation · local transport · equipment (required/recommended/optional) · guide · safety · policies · media gallery · related.

## Content Priority

| Priority | Content |
|----------|---------|
| Decision Critical | Summary · duration · difficulty/eligibility · commercial status · CTA · itinerary overview |
| Important Supporting | Day structure · equipment · inclusions · policies · guide/safety |
| Secondary Discovery | Related experiences · UGC |

## Page Anatomy

Breadcrumb · Hero · Experience summary · Duration · Difficulty · Eligibility · Meeting/origin · Day-by-day itinerary · Stops · Destination/Attraction refs · Meals · Accommodation · Local transport · Equipment · Included/Excluded · Guide · Safety · Policies · Media · Related · Primary CTA.

## Itinerary Timeline

Structured, not one untyped blob. UI may represent: Day · Stop · Destination · Attraction · Meal · Accommodation · Activity · Transport — without implying module ownership transfer.

## Above-the-Fold

Identity + summary + duration + difficulty/eligibility + status + CTA path. Avoid burying overview under long day-1 prose.

## Desktop / Tablet / Mobile

Desktop: long-form sections + optional side summary. Tablet: stacked sections. Mobile: readable long-form; avoid ultra-dense timelines; progressive disclosure OK while keeping SEO-important content accessible (not client-only hidden forever).

## RTL / LTR / Bidi

Logical layout. Codes, times, map coords LTR-safe. Timeline reading order follows document locale; travel arrows semantic.

## Loading / Empty / Error / Unavailable

Loading: stable itinerary placeholders. Empty day content = subsection. Core missing = not found. Expired/unavailable experience: clear status, no false bookability. Secondary UGC fail = degrade.

## Accessibility

Heading per day/section · skip to itinerary · keyboard · focus · alt · status for CTA. Risk: endless timeline without landmarks.

## SEO Role

Primary Indexable Resource (direction). IndexPolicy route-specific.

## Internal Linking

→ Destination · → Attraction · → Related experiences · → Guides.

## Structured Data Candidates

`BreadcrumbList` · `TouristTrip` · `ItemList` (itinerary candidate) · `Offer`.

## Performance Risks

Long itinerary HTML · many images · map. LCP: hero. Prefer SSR of critical summary + day headings.

## Analytics Intent

`ItineraryDayExpanded` · `AttractionOpened` · `BookingStarted`.

## Explicit Non-Goals

Foreign-package hotel comparison UI · live flight search · checkout.

## Responsive Behavior Matrix

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Summary | Beside hero | Stack | Stack | Logical | H1 |
| Day timeline | Full sections | Full | Progressive disclosure | Logical order | Headings per day |
| Equipment lists | Columns | Columns | Stacked lists | — | Clear required vs optional |
| CTA | Sticky optional | Sticky optional | Persistent bar | Start edge | Focusable |

## Reference Sites

| Source | Useful | TravelCore | Must NOT copy |
|--------|--------|------------|---------------|
| REF-LS-002 | Structured itinerary value | Timeline model | Brand/text/assets/UI |

**Reference evidence incomplete** for difficulty/equipment wireframes beyond registry concepts — TravelCore still specifies from domain.
