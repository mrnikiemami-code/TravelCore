# Tour Listing / Discovery — Page Archetype

**Archetype:** `TourListingPage`  
**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)

Listing state ≠ SEO Landing. See [`08-search-and-seo-landings.md`](08-search-and-seo-landings.md).

---

## Purpose

Discover and narrow relevant Tours.

## Primary User Intent

Discover and narrow relevant Tours.

## Secondary User Intents

Compare cards · Adjust filters · Sort · Open detail · Recover from empty.

## Primary CTA

Open Tour detail (or apply/refine search).

## Secondary CTA

Clear filters · Open Destination · Controlled SEO landings when linked editorially.

## Target Resources / Modules

Tour · Destination (filter context) · Search (if shared) · SEO only when page is also a controlled landing (then prefer SeoLanding archetype composition).

## Required Data

Result set (possibly empty) · query/filter context · sort · count.

## Optional Data

Editorial SEO intro (only if intentional landing) · badges · agency · nearest departure · price context.

## Content Priority

Decision Critical: context heading · filters · results · count. Supporting: sort · chips. Secondary: editorial blocks.

## Page Anatomy

Heading/context · Search/refinement summary · Filters · Sort · Result count · Tour cards · Active filter chips · Pagination/load · Empty · SEO/editorial only where applicable.

## Filters (potential dimensions)

Destination · Origin · Date · Duration · Tour type · Hotel category · Meal plan · Transport/carrier · Agency · Price range · Visa requirement.

Exact product scope later. **Do not** imply every filter combination is indexable (ADR 0010).

## Tour Card

Enough for comparison, not a mini-detail page: title · destination · duration · primary media · nearest/relevant departure · commercial price context · agency · important badge/status. Do not overload.

## Desktop / Tablet / Mobile

Desktop: filter sidebar/panel OK. Mobile: filter Sheet / full-screen. Active filters understandable. Critical filtering must not depend on hover.

## RTL / LTR / Bidi

Logical filter layout. Currency/date codes LTR-safe. Chips readable.

## Loading / Empty / Error

Skeleton cards. Empty: help recover (clear filters, suggestions) — not blank page. Core listing API fail: page error. Optional enrichment fail: degrade cards.

## Unavailable

N/A as page type; individual cards may show sold-out/expired badges without removing listing.

## Accessibility

Filter controls labeled · results count announced · keyboard · focus return from sheet · card headings.

## SEO Role

**Normally NoIndex Utility** or **Potential Indexable** when deliberately configured. Not automatic SEO landing.

## Indexability Direction

Filter query URLs generally noindex utility. Controlled landings use SeoLanding archetype.

## Canonical / Locale

Localized listing shells; no fake locale content.

## Internal Linking

→ Tour detail · → Destination · → Controlled landings (curated).

## Structured Data Candidates

`ItemList` (candidate) · `BreadcrumbList`. Avoid false Product markup on every card.

## Performance Risks

Large result pages · heavy card images · client filter thrash. Prefer SSR first page.

## Analytics Intent

`FilterApplied` · `SortChanged` · `TourCardOpened` · `EmptyResultsShown`.

## Explicit Non-Goals

Making every combination crawlable · HotelBooking live search · exact filter set.

## Responsive Behavior Matrix

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Filters | Sidebar | Collapsible | Sheet | Logical | Labels + focus trap |
| Cards | Grid | Grid 2 | Stack | — | Heading + link |
| Chips | Inline | Wrap | Wrap | Start→end | Removable via keyboard |
| Pagination | Bottom | Bottom | Bottom | — | Clear current page |

## Reference Sites

REF-LS-003 mentions discovery broadly; **Reference evidence incomplete** for exact listing filter UX — do not invent LastSecond filter lists as requirements.
