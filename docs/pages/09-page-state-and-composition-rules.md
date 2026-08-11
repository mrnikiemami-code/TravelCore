# Page State and Composition Rules

Cross-cutting rules for all public Page Archetypes.  
Philosophy: [`../architecture/13-reference-page-archetypes.md`](../architecture/13-reference-page-archetypes.md)

---

## 1. Route → Archetype → Page View Model

```text
SEO Route
  → Resource identity / landing identity
  → Page Archetype
  → purpose-built Page View Model
```

Do not bind architectural identity to arbitrary JSX filenames.  
Page VMs evolve independently from DB columns.  
Historical Booking snapshots ≠ current detail read models.

---

## 2. Composition Boundary

Pages compose **module application/read contracts**.

Forbidden as composition strategy:

- cross-module DbContext  
- cross-module EF Include  
- giant SQL joins across module schemas  

Ownership unchanged by composition.

---

## 3. Core vs Secondary Failure

| Failure | Behavior |
|---------|----------|
| **Core resource** unresolved (unknown id/route) | Page-level error / not-found per SEO route semantics |
| **Secondary section** fails (UGC, related articles, optional map) | Graceful degradation of that section |

Example: Hotel exists + RelatedArticles unavailable → Hotel page still renders.

Do not turn every optional failure into whole-page failure.

---

## 4. Loading

- Stable layout; avoid collapsing/re-expanding entire page  
- Independent section streaming allowed  
- Important initial semantic content server-renderable where possible (ADR 0005)  
- Skeletons match final structure  

---

## 5. Empty

**Empty subsection ≠ empty page.**

Destination with no active Tours may still show Hotels, Attractions, Content.  
Do not convert partial absence into 404.

---

## 6. Unavailable / Expired

Commercial unavailability ≠ automatic HTTP not-found.

Support patterns: Expired · No active departure · Temporarily unavailable · Provider outage (commerce only).

Preserve useful content when SEO/business policy permits.  
Disable misleading CTAs.  
Offer related/newer alternatives when available.

---

## 7. CTA Authority

UI CTA visibility ≠ proof of bookability.  
Backend/application use case validates business state.

One clear primary CTA on commercial archetypes; avoid many equal-weight primaries.

---

## 8. Price Honesty

- Display price ≠ Quote  
- Clarify From / Per person / Occupancy-specific / Multi-component  
- Never fabricate `0`, “Contact us = 0”, or fake starting prices  
- Mixed currencies not implicitly summed  
- Toman only as explicit display/input per Money ADR  

---

## 9. Responsive Behavior

Every archetype documents a matrix: Major Element × Desktop × Tablet × Mobile × RTL/LTR × Accessibility.

Rules:

- Mobile-first priority: Decision Critical not buried under secondary  
- Mobile ≠ compressed desktop tables  
- No hover-only primary actions  
- Sticky OK if content also available without it  

UI Constitution authoritative for Tabs vs Sections: do not hide all core detail behind client-only tabs; prefer sections/anchors for document-like content.

---

## 10. RTL / LTR / Bidi

- Layout: start/end logical  
- Semantic non-mirroring: travel direction (`IKA → IST`), maps, media  
- LTR-safe islands: airport codes, flight numbers, currency codes, email, phone, booking refs  
- One archetype for fa/en/ar — localized representation, not duplicated archetypes  
- Missing published translation → no fabricated locale page (ADR 0008)  

---

## 11. Accessibility (archetype risks)

Reference Accessibility Constitution; per page call out:

- heading hierarchy · landmarks · keyboard · focus (esp. sheets) · touch targets · alt · status/error announcements  

---

## 12. SEO Integration

| SEO Role direction | Meaning |
|--------------------|---------|
| Primary Indexable Resource | Stable entity pages (TourProduct, Destination, Article, …) |
| Potential Indexable Resource | Place details, Visa, quality UGC, … |
| Normally NoIndex Utility | Search results, many filter listings |
| Controlled SEO Landing | Explicit approved landings |

Indexability is **not** hard-wired by React component type. SEO route/IndexPolicy owns it. SEO does not own business content.

Public ≠ automatically indexable.

---

## 13. Internal Linking

Meaningful user+crawl paths only. No crawler-only link clouds.

---

## 14. Structured Data

Candidates only; truthfulness required; no synthetic ratings.

---

## 15. Performance

Identify LCP/hydration risks per archetype. Section-level projections for multi-module pages. Client islands limited (booking, maps). Maps enhance — basic info works if map JS fails.

---

## 16. Gallery

Primary media + gallery access; understanding product must not require gallery interaction. Accessible alt semantics.

---

## 17. Trust

Clear places for: seller/agency · policies · dates · price units · availability · inclusions/exclusions. Critical conditions not visually hidden.

---

## 18. UGC / Social Proof

UGC enhances; not authoritative for core business facts. Reviews/ratings only from real eligible data.

---

## 19. Analytics

Conceptual intent events only — not vendor taxonomy.

---

## 20. Anti-Patterns (enforce)

See list in [`../architecture/13-reference-page-archetypes.md`](../architecture/13-reference-page-archetypes.md) §10.

---

## 21. Page-State Examples (Foreign Tour)

| Situation | Classification |
|-----------|----------------|
| Tour exists + active departure | Normal |
| Tour exists + no active departure | Unavailable (commerce) |
| Tour exists + expired | Unavailable / Expired (not auto-404) |
| Tour exists + secondary hotel projection failure | Partial degradation |
| Tour route nonexistent | Not found (core) |

---

## 22. Intentionally Deferred

Exact visual design · brand · component library/names · final URLs where deferred · homepage archetype · checkout/payment · live Flight/HotelBooking search UX · Admin/Agency · full analytics taxonomy · exact JSON-LD · exact filter lists · sticky measurements · design tokens · map/gallery libraries.

---

## 23. Reference ≠ Requirement

A LastSecond/TahaGasht pattern becomes TravelCore requirement only if justified by user intent, domain semantics, accepted architecture, and product value.
