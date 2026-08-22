# P40 — Marketplace Merchandising & Experience Depth Plan

| Field | Value |
|-------|--------|
| Task-ID | `TC-P40-T001` (this document) |
| Phase | P40 — Marketplace Merchandising & Experience Depth |
| Date | 2026-08-22 |
| Type | Implementation plan (**docs only**) |
| Audit evidence | [`docs/product-experience/evidence/P40-T001/EXPERIENCE-AUDIT.md`](../product-experience/evidence/P40-T001/EXPERIENCE-AUDIT.md) |
| Prior gates | P38 commerce · P39 finance foundation |

## 1. Purpose

Define the **smallest ranked set of P40 tasks** that increase commercial credibility and sellability while preserving domain truth — **no fake KPIs, no commission UI, no hardcoded banners**.

---

## 2. Locked boundaries (carry forward)

```text
AgencyOffer ≠ Price ≠ Quote · Quote ≠ Booking · Booking ≠ Payment
Commission ≠ Pricing · Settlement ≠ Payment · FE ≠ SoT
Search ≠ SEO · Media owns assets · consuming domains own relations
No fake ratings · commissions · revenues · conversion rates
Campaign ≠ hardcoded homepage banner
```

---

## 3. Ranked P40 implementation backlog

### MUST — commercial credibility

| Rank | Proposed Task-ID | Outcome | Surface | Data dependency | Acceptance (summary) |
|------|------------------|---------|---------|-----------------|----------------------|
| M1 | **TC-P40-T002** | Truthful multi-agency **offer comparison** on tour detail | Public | `RelatedAgencyOfferPublicContracts` fields only | 0/1/N offers; mobile-usable; selection → book unchanged; no price/rating invention |
| M2 | **TC-P40-T003** | Marketplace **card + listing** depth (tours/hotels/home) | Public | Existing public APIs + Media | Improved hierarchy, density, trust signals; DEMOFEED chip hygiene; empty states |
| M3 | **TC-P40-T004** | **Technical label leakage** cleanup (public + agency) | Public · Agency | Copy/i18n only | No policy codes on traveler UI; localized status; reduced UUID exposure where safe |
| M4 | **TC-P40-T005** | Agency Portal **operational UX** depth (offer ops polish) | Agency | Offer ops APIs (existing) | Filters, lifecycle clarity, mobile; no fake commission/settlement |

### SHOULD — experience quality

| Rank | Proposed Task-ID | Outcome | Surface | Data dependency | Acceptance (summary) |
|------|------------------|---------|---------|-----------------|----------------------|
| S1 | **TC-P40-T006** | Admin **data-grid + governance UX** refinement | Admin | Governance + catalog APIs | Filters/sort preserved; workflow cards; no fake reporting |
| S2 | **TC-P40-T007** | Customer dashboard **live bookings read** (honest list) | Customer | Booking read API (existing public status) | `/me/bookings` shows real Pending/Confirmed; no fake payments |
| S3 | **TC-P40-T008** | Design system **comparison + commerce primitives** | DS / shared | N/A | Reusable comparison panel, status chips, sticky action patterns |
| S4 | **TC-P40-T009** | **DEMOFEED / catalog density** experience hygiene | Public · docs | DEMOFEED + media | Disclosure copy; slug humanization; media gap documentation |

### LATER / blocked

| Item | Blocker |
|------|---------|
| TC-P40-T010 Campaign/Placement architecture | ADR + domain ownership decision |
| Agency commission/settlement dashboards | P39 business/legal/provider blockers |
| Admin revenue/reporting charts | No financial SoT in FE |
| Hotel multi-agency offers | No product envelope |
| Production payment UX polish | P35 provider blockers |
| Finance engine UI | Explicit finance execution envelopes |

---

## 4. Proposed phase task sequence (post-T001)

```text
TC-P40-T001  Experience Audit + Plan          ← this task
TC-P40-T002  Public Multi-Agency Comparison   MUST
TC-P40-T003  Marketplace Card/Listing Depth   MUST
TC-P40-T004  Label Leakage Cleanup            MUST
TC-P40-T005  Agency Offer Ops UX Depth        MUST
TC-P40-T006  Admin Grid/Governance UX          SHOULD
TC-P40-T007  Customer Live Bookings List      SHOULD
TC-P40-T008  DS Comparison/Commerce Primitives SHOULD
TC-P40-T009  DEMOFEED/Catalog Hygiene          SHOULD
TC-P40-T010  Campaign/Placement Architecture   LATER (plan/ADR)
TC-P40-GATE  Experience Depth Gate             (after MUST slice)
```

Architect may reorder after T001 ACCEPT. **Do not execute T002+ without envelope.**

---

## 5. Merchandising architecture direction (planning only)

Future capability model (not implemented in P40 without ADR):

```text
Campaign
  └── Promotion (rules, window, channel)
        └── Placement (surface slot: home hero, listing boost, detail ribbon)
              └── Audience (locale, market, optional segment)
```

**Editorial content** remains Content module SoT. **Catalog ranking** remains Search/index policy. **Promoted visibility** must be auditable and non-deceptive (no fake scarcity).

---

## 6. Design system priorities

1. **Offer comparison panel** — accessible radio/table hybrid; RTL-safe
2. **Localized status chips** — map domain enums to i18n keys
3. **Admin data-grid patterns** — sort/filter/saved-view hooks (no fake aggregates)
4. **Empty/loading/skeleton** — consistent across public shells
5. **Sticky commerce actions** — extend pattern from tour to comparable surfaces

Charts: **only when wired to real read APIs** (not in initial P40 MUST slice).

---

## 7. SEO / accessibility guardrails (all P40 UI tasks)

- Preserve Server Component First where applicable
- Semantic heading order on comparison UI
- Focus management on offer selection change
- `prefers-reduced-motion` for animations
- No indexation regressions on transaction pages
- Image `sizes` / aspect ratio via MediaImage

---

## 8. Explicit out of scope (entire P40 until enveloped)

- Commission / settlement / payout engines or fake financial UI
- Fake agency ratings, reviews, or conversion metrics
- Hardcoded promotional banners without placement model
- Architecture redesign of backend domains
- Production payment provider integration

---

## 9. Recommended next authorized task

**`TC-P40-T002`** — Public Multi-Agency Offer Comparison Depth (truthful dimensions only)

---

## 10. P40 success criteria (phase-level)

P40 MUST slice complete when:

1. Traveler can **compare** multiple agency offers on tour detail using **contract-backed** fields only
2. Public listing/card quality measurably improved (visual review evidence)
3. Agency offer ops usable on mobile without UUID-first UX where avoidable
4. No fake commerce/finance metrics introduced
5. DEMOFEED debt reduced or clearly disclosed
6. P40-GATE passes with known LATER items documented
