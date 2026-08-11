# Destination Landing — Page Archetype

**Archetype:** `DestinationLandingPage`  
**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)

Destination is a primary discovery + SEO surface. Knowledge/discovery identity is independent of temporary commerce.

---

## Purpose

Understand a destination and discover actionable travel options.

## Primary User Intent

Understand a destination and discover actionable travel options.

## Secondary User Intents

Browse tours/hotels/places · Read guides · Explore hierarchy · Seasonal/requirements · Related destinations.

## Primary CTA

Explore primary commerce/discovery path (e.g. Tours for that destination) — one clear primary, not many equal CTAs.

## Secondary CTA

Hotels · Attractions · Guides · Related destinations.

## Target Resources / Modules

**Composition root:** Destination. **Composed (not owned):** Tour · Place · Content · UGC · SEO · Media.

## Required Data

Destination identity · localized published content for locale · hierarchy context where available · SEO route.

## Optional Data

Tours · Hotels · Attractions · Restaurants · Articles · Travelogues · Seasonal · Travel requirements · Sub-destinations.

## Content Priority

Decision Critical: hero identity · essential facts · primary discovery entry (tours/places). Supporting: sub-destinations · requirements. Secondary: long editorial · UGC.

## Page Anatomy

Breadcrumb · Destination Hero · Summary / essential facts · Sub-destinations/neighborhoods · Tours · Hotels · Attractions · Restaurants · Guides/articles · Travelogues/UGC · Seasonal · Travel requirements · Related destinations · Internal nav · SEO/editorial integrated (not text wall first).

## Destination Hierarchy

Hierarchy drives breadcrumbs / sub-nav / related — **not** derived merely from URL string segments.

### Illustrative hierarchy (conceptual)

```text
Asia → Turkey → Istanbul → Beyoglu → Taksim
```

Composition example: Istanbul landing may show Tours + Hotels + Attractions + Articles **without owning** those modules.

## Empty Commerce

Page remains valuable with **no active Tour** and **no live HotelBooking inventory**. Do not 404 for empty commerce alone.

## Above-the-Fold

Destination identity + essential context + entry to discovery — not SEO essay first.

## Desktop / Tablet / Mobile

Desktop: sectioned composition with side nav/anchors optional. Tablet/Mobile: stacked sections; decision discovery before long secondary lists.

## RTL / LTR / Bidi

Logical. Names may be mixed-script. Maps not mirrored.

## Loading / Empty / Error

Section skeletons. Empty Tours = empty section, page continues. Core Destination missing = not found. UGC fail = degrade.

## Accessibility

Landmarks per major section · heading hierarchy · skip links · map alternative text list.

## SEO Role

Primary Indexable Resource (direction).

## Indexability / Canonical

Per SEO module + locale publication. Localization example:

| Locale | Published? | Switch behavior |
|--------|------------|-----------------|
| fa | Yes | Show FA route |
| en | Yes | Show EN route |
| ar | No | Do **not** fabricate Arabic page |

## Internal Linking

→ Tours · Hotels · Attractions · Articles · Parent/child destinations · Controlled landings.

## Structured Data Candidates

`TouristDestination` · `BreadcrumbList` · `ItemList` sections.

## Performance Risks

Many modules → huge payload if serialized together. Use section-level projections. LCP: hero. Avoid loading all related entities in one shot.

## Analytics Intent

`TourOpened` · `HotelOpened` · `GuideOpened` · `SubDestinationOpened`.

## Explicit Non-Goals

Owning Tour/Place data · inventing commerce · SEO text-wall-first layout.

## Responsive Behavior Matrix

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Hero | Wide | Stack | Stack | Logical | H1 |
| Section nav | Side/anchors | Anchors | Anchors / disclosure | — | Skip |
| Tour rails | Horizontal/grid | Grid | Stack cards | — | Links labeled |
| Editorial | Readable measure | Same | Same | — | Not ahead of discovery |

## Reference Sites

REF-LS-003: destination-centric discovery + internal linking — useful IA signal. Must NOT copy homepage visual/brand.
