# Content (Article / Travel Guide) and Travelogue

**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)

Editorial Article ≠ Travelogue (UGC). Do not merge ownership.

---

# A. Article / Travel Guide

**Archetype:** `ArticlePage`  
**Owner:** Content

## Purpose

Answer an informational travel question or guide planning.

## Primary User Intent

Read trustworthy long-form guidance.

## Secondary User Intents

Navigate TOC · Follow destination/place links · Related content · Contextually relevant products.

## Primary CTA

Continue reading / follow primary related destination (commerce CTA secondary and contextual).

## Secondary CTA

Related articles · Tours/hotels when contextually useful — not every paragraph as ads.

## Target Modules

Content · Destination/Place references · SEO · Media · optional Tour/Place commerce links.

## Required / Optional

Title · body · locale publication · metadata. Optional: hero · TOC · inline entities · author · dates · related commerce.

## Content Priority

Decision Critical: title · intro · body. Supporting: TOC · metadata. Secondary: commerce inserts · related.

## Page Anatomy

Breadcrumb · Title · Editorial metadata · Hero (optional) · Introduction · Body · TOC (long) · Inline destinations/places · Related content · Related products (contextual) · Author/source · Updated/published.

## Editorial Width

Long-form uses readable **narrow content measure**. Do not render articles at full desktop viewport width. Surrounding related rails may be wider.

## Above-the-Fold

Title + intro — not commerce rail first.

## Desktop / Tablet / Mobile

Desktop: narrow measure + optional side TOC/related. Mobile: single column; TOC disclosure OK; body remains accessible/SSR.

## RTL / LTR / Bidi

Logical prose. Inline LTR codes/URLs safe. Media not mirrored.

## Loading / Empty / Error

Body SSR preferred. Missing article = not found. Related commerce fail = omit inserts.

## Accessibility

Heading hierarchy · skip to content · TOC links · alt · focus.

## SEO Role

Primary Indexable Resource (direction).

## Internal Linking

→ Destination · Places · Related articles · Contextual products.

## Structured Data Candidates

`Article` · `BreadcrumbList` · `FAQPage` only when genuine FAQ.

## Performance Risks

Heavy inline media · third-party embeds. LCP: hero or title block.

## Analytics Intent

`ArticleReadProgress` · `InlineDestinationOpened` · `RelatedProductOpened`.

## Explicit Non-Goals

Turning Content into a shop · synthetic authorship.

---

# B. Travelogue

**Archetype:** `TraveloguePage`  
**Owner:** UGC / community semantics (not normal editorial Article ownership)

## Purpose

Present a traveler story with destination/place context.

## Primary User Intent

Consume a trip narrative / social travel story.

## Secondary User Intents

Author · Places visited · Related discovery · Moderation-aware status.

## Primary CTA

Explore related destination / places (or follow author if product supports).

## Secondary CTA

Related travelogues · Tours (contextual).

## Target Modules

UGC · Destination/Place refs · Media · SEO · Moderation status.

## Required / Optional

Author · narrative · publication/moderation status · locale rules. Optional: timeline · media · visited places · trip context.

## UGC Language

UGC language may differ from UI locale. Accepted i18n rules apply — do not silently rewrite UGC into UI locale as “translation” without product policy.

## Anatomy

Author · trip context · destination · timeline/story · media · visited places · status · related discovery.

## SEO Role

Potential Indexable (quality/moderation gated).

## Structured Data Candidates

`BlogPosting` / `SocialMediaPosting` candidates — only if truthful.

## Performance / States / A11y

Gallery risk · moderation unavailable states · author alt · headings.

## Explicit Non-Goals

Treating Travelogue as CMS Article · unverified ratings as facts.

## Responsive Matrices

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Article body | Narrow measure | Narrow | Full width readable | Logical prose | Headings |
| TOC | Side | Collapse | Disclosure | — | Linked |
| Travelogue media | Gallery | Gallery | Swipe | Not mirrored | Alt |
| Commerce inserts | Sparse side/below | Below | Below | — | Not primary |

## Reference Sites

REF-LS-003 lists travelogues conceptually. **Reference evidence incomplete** for travelogue page anatomy — TravelCore distinguishes UGC vs Content from domain.
