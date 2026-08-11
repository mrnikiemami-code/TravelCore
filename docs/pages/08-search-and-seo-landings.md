# Search Results and Controlled SEO Landings

**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)  
**Authoritative SEO:** [`../architecture/12-seo-constitution.md`](../architecture/12-seo-constitution.md) · ADR 0010

These are **two different archetypes**. Do not conflate.

---

# A. Search Results

**Archetype:** `SearchResultsPage`

## Purpose

User-discovery across catalog types after a query.

## Primary User Intent

Find relevant Destinations / Tours / Hotels / Places / Content from a query.

## Secondary User Intents

Refine · Correct spelling · Open a result · Recover from empty.

## Primary CTA

Open a result item.

## Secondary CTA

Refine query · Clear filters · Popular discovery.

## Target Modules

Search · composed result owners (Destination, Tour, Place, Content, …) · SEO only for noindex/meta utility — Search is **not** a general SEO landing generator.

## Required / Optional

Query context · result groups or ranked list · count. Optional: filters · suggestions · autocomplete.

## Content Priority

Decision Critical: query · results · empty recovery. Supporting: filters. Secondary: promos (avoid).

## Page Anatomy

Query context · Autocomplete/refinement · Result groups or unified ranking · Filters where meaningful · Count · Empty · Suggestions/corrections · Pagination.

## Empty State

Help recovery: spelling suggestion · remove filters · related destinations · popular discovery. **Never** blank page.

## Desktop / Tablet / Mobile

Desktop: results + optional filter. Mobile: filters in sheet; results first.

## RTL / LTR / Bidi

Query may be mixed-script; preserve user input direction. Codes LTR-safe.

## Loading / Error

Skeleton groups. Search core fail = page error. One group fail = degrade that group.

## SEO Role

**Normally NoIndex Utility.** Do not create indexability assumptions from component type.

## Canonical / Locale

Localized UI shell; no fake translated result fabrication.

## Internal Linking

Results are the links; avoid irrelevant clouds.

## Structured Data Candidates

Usually none for utility search; avoid fake ItemList SEO spam.

## Performance Risks

Fan-out to many modules · autocomplete chatter. Prefer progressive group loading.

## Analytics Intent

`SearchSubmitted` · `ResultOpened` · `EmptySearchRecovered`.

## Explicit Non-Goals

Auto-generating indexable pages from every query · treating Search as SeoLanding.

## Responsive Matrix (Search)

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Query | Top | Top | Top | Input bidi | Label |
| Groups | Sections | Sections | Accordion OK | — | Headings |
| Empty | Help panel | Same | Same | — | Status |

---

# B. Controlled SEO Landing

**Archetype:** `SeoLandingPage`

## Purpose

Intentional search-demand landing with meaningful value — not arbitrary filter state.

## Primary User Intent

Land on a curated topic (e.g. Istanbul Tours) and discover relevant options + context.

## Secondary User Intents

Open products · Read intro/FAQ · Navigate related landings/destinations.

## Primary CTA

Primary product exploration (e.g. first tour results / destination deep link).

## Secondary CTA

Related landings · Guides.

## Target Modules

SEO landing definition · composed Destination/Tour/Place/Content as approved · FAQ only when genuine.

## Required / Optional

Intent heading · explicit index intent · meaningful composed value. Optional: editorial intro · products · FAQ · internal links.

## SEO Landing ≠ Filter Snapshot

Explicit:

```text
SEO Landing  ≠  FilterState serialized into URL
```

Requires explicit route/index intent and meaningful value (ADR 0010).

### Conceptual comparison (routes not permanently frozen)

| Kind | Conceptual example | Archetype |
|------|-------------------|-----------|
| Functional filter URL | `/fa/tours?destination=istanbul&hotel=5` | TourListing / Search utility — generally **not** controlled landing |
| Controlled SEO Landing | `/fa/tours/istanbul` | SeoLandingPage — **only if** explicitly approved index intent + value |

Exact final URL patterns may remain deferred; principles stand.

## Content Priority

Decision Critical: intent heading · primary product set · clear navigation. Supporting: short editorial. Secondary: FAQ. **Do not** mass-produce content-free landings. **Do not** put SEO text wall above user intent.

## Page Anatomy

Intent-specific heading · editorial/contextual introduction · relevant products · Destination data · Place data · Content · genuine FAQ · internal links.

## Desktop / Tablet / Mobile

Product grids + short intro; mobile products early; editorial integrated.

## RTL / LTR / Bidi

Same public rules as Destination/Listing.

## Loading / Empty / Error

Products may empty → still show landing identity + recovery; empty does not auto-delete approved landing unless policy says unpublish. Core landing identity missing = not found.

## SEO Role

**Controlled SEO Landing.** IndexPolicy from SEO module — not from React component type.

## Internal Linking

→ Destination · Tours · sibling landings · Guides.

## Structured Data Candidates

`BreadcrumbList` · `ItemList` · `FAQPage` (genuine only).

## Performance Risks

Over-composition; same as Destination — section projections.

## Analytics Intent

`LandingProductOpened` · `LandingFaqOpened`.

## Explicit Non-Goals

Programmatic junk landings · equating listing filters with SEO routes.

## Responsive Matrix (SEO Landing)

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Heading + intro | Top, concise | Same | Same | Logical | H1 |
| Products | Grid | Grid | Stack | — | Cards |
| FAQ | Below | Below | Disclosure | — | Accordion a11y |

## Reference Sites

REF-LS-003 signals SEO landing architecture conceptually. Must NOT copy proprietary landing copy/URL schemes as TravelCore requirements.
