# TC-UIVAL-T006 — Hotel Detail Validation Evidence

**Task:** `TC-UIVAL-T006` — Hotel Detail validation (ROADMAP #6)  
**Baseline:** `70c87ea`  
**Routes:** `/[locale]/dev/hotel-detail` · `/[locale]/places/[slug]` (kind=Hotel)  
**Quality:** `npm run quality` → **PASS**  
**Date:** 2026-08-20

Related: P07-T007 · [`docs/pages/05-place-details.md`](../pages/05-place-details.md) § Hotel Detail

---

## 1. Deliverables

| Artifact | Purpose |
|----------|---------|
| `lib/fixtures/hotel-detail/{fa,en,index}.ts` | Hotel Place catalog PVM fixture |
| `app/[locale]/dev/hotel-detail/page.tsx` | Dev noindex route → `PlaceDetailView` |
| `scripts/uival-hotel-detail-checks.mjs` | Archetype structure checks |

## 2. Matrix

| Criterion | Result |
|-----------|--------|
| HotelDetailPage = Place kind Hotel | **PASS** |
| Star rating display | **PASS** |
| Destination link | **PASS** |
| Location / coordinates LTR | **PASS** |
| Facilities list | **PASS** |
| Book CTA → `/places/[slug]/book` (catalog ≠ booking engine) | **PASS** |
| Server Component | **PASS** |
| FA / EN | **PASS** |

**TC-UIVAL-T006 COMPLETE** · **TC-UIVAL-T007** (Home / Discovery) next.
