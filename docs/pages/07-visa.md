# Visa Detail / Landing — Page Archetype

**Archetype:** `VisaDetailPage`  
**Registry:** [`00-page-archetype-registry.md`](00-page-archetype-registry.md)

## P17-T007 implementation note

Public `VisaDetailPage` is informational composition (`/[locale]/visas/[code]`). Visa owns structured facts; Content may enrich; SEO owns IndexPolicy. **Public Visa Page != Automatically SEO Indexed**. Application/consultation CTA remains **P17-R8 UNRESOLVED** — this page must not imply a live application or booking transaction.

---

## Purpose

Understand visa requirements/product and move toward application/lead flow.

## Primary User Intent

Understand visa requirements/product and move toward the relevant application/lead flow.

## Secondary User Intents

Eligibility · Documents · Processing time · Validity · Pricing · FAQ · Related tours/destination.

## Primary CTA

Start application / Request consultation (backend-validated).

## Secondary CTA

Related destination · Related tours · FAQ anchors.

## Target Resources / Modules

Visa · Destination/Country context · Content (FAQ/guides) · Pricing if authoritative · SEO · Media.

## Required Data

Visa product/requirement identity · locale publication · clear status · CTA honesty.

## Optional Data

Documents list · processing · validity/stay · pricing · steps · warnings · FAQ · related tours.

## Content Priority

Decision Critical: country/destination context · visa type · eligibility · warnings · CTA. Supporting: documents · timing · pricing. Secondary: related commerce.

## Page Anatomy

Country/destination context · Visa type · Eligibility · Required documents · Processing time · Validity/stay · Pricing (authoritative) · Application steps · Important warnings · FAQ/content · Related tours/destination · CTA.

## Legal / Factual Clarity

Visa content is time-sensitive. Provide room for: last updated · source/provenance where appropriate · policy notices. Do **not** imply static content is permanently correct.

## Above-the-Fold

Visa type + destination/country + eligibility snapshot + warning if any + CTA.

## Desktop / Tablet / Mobile

Document lists readable; steps as sections; mobile avoid dense legal walls without headings.

## RTL / LTR / Bidi

Logical. Form codes, passport field names, currency LTR-safe.

## Loading / Empty / Error / Unavailable

Skeleton for requirements. Core missing = not found. Pricing unavailable = explicit, not `0`. Policy outdated flag when product supports.

## Accessibility

Warnings as assertive status · headings · lists · focus to CTA · form later.

## SEO Role

Potential / Primary Indexable (direction). IndexPolicy route-specific.

## Internal Linking

→ Destination · Tours · Guides · Application entry.

## Structured Data Candidates

`FAQPage` (genuine) · `BreadcrumbList` · `Service`/`Product` candidates carefully.

## Performance Risks

Long FAQ · document downloads. LCP: header.

## Analytics Intent

`ApplicationStarted` · `DocumentListViewed` · `RelatedTourOpened`.

## Explicit Non-Goals

Full application wizard UX · legal advice engine · inventing government truth.

## Responsive Behavior Matrix

| Element | Desktop | Tablet | Mobile | RTL/LTR | A11y |
|---------|---------|--------|--------|---------|------|
| Warnings | Prominent | Prominent | Top sticky/banner | Logical | Assertive live region |
| Documents | Two-col | One | One | — | Lists |
| Steps | Numbered sections | Same | Same | — | Ordered list |
| CTA | Side/sticky | Below header | Persistent | Start edge | Clear disabled |

## Reference Sites

REF-TH-001 mentions visa in commerce breadth. **Reference evidence incomplete** for visa page anatomy — do not invent competitor document checklists as requirements.
