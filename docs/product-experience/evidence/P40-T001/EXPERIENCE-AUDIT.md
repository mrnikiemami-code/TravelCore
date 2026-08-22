# P40-T001 — Marketplace & Experience Depth Audit

| Field | Value |
|-------|--------|
| Task-ID | `TC-P40-T001` |
| Phase | P40 — Marketplace Merchandising & Experience Depth |
| Date | 2026-08-22 |
| Type | Experience audit (**evidence only · no UI implementation**) |
| HEAD baseline | `e989538` |
| Prior gates | P38 `READY_COMMERCE_VERTICAL_WITH_GOVERNANCE` · P39 `READY_FOUNDATION` |

## Overall experience verdict

**`FOUNDATION_WITH_PARTIAL_SELLABLE_PUBLIC_SLICE`**

Public tour commerce (discovery → detail → multi-offer **selection** → prepare → payment sandbox) is the strongest vertical. Customer, Agency (except offer ops), and Admin reporting remain **foundation-first with honest empties**. **No offer comparison UI** exists. Merchandising/campaign architecture is **not established**. DEMOFEED naming debt persists.

---

## 1. Public marketplace assessment

| Surface | Maturity | Key paths |
|---------|----------|-----------|
| Home | PARTIAL | `app/[locale]/page.tsx` · `HomeDiscoveryView` |
| Tour listing | PARTIAL | `tours/page.tsx` · destination-scoped |
| Tour detail | PARTIAL+ | `tours/[slug]/page.tsx` · commerce panel + offers |
| Hotel listing/detail | PARTIAL | `hotels/*` · no live rates |
| Booking prepare | WORKING | `tours/[slug]/book` · `agencyOfferId` param |
| Payment/status | WORKING | sandbox path (P34) |

**Strengths:** Real API-backed sections; honest empties; departure + pricing gating; sticky actions; design-system primitives.

**Gaps:** Sparse DEMOFEED catalog; policy/requirement **code** leakage on tour detail; demo chip/slug visibility; no search engine (by design); hotel path lacks multi-agency model.

**Gate lineage:** P36 PARTIALLY_SELLABLE_VISUALLY · P33 tour-first commerce PASS WITH LIMITATIONS.

---

## 2. Multi-agency comparison assessment (primary P40 concern)

| Scenario | Current behavior |
|----------|------------------|
| Zero offers | Block hidden / empty state |
| One offer | Auto-selected |
| Multiple offers | Radio list (`agency-offers-list.tsx`); URL `?agencyOfferId=` |
| Comparison | **None** — no side-by-side, no dimension matrix |

**Truthful dimensions available today (public DTO):**

| Dimension | In contract | In UI |
|-----------|-------------|-------|
| Agency display name | YES | YES |
| Title override / highlight / description | YES | YES |
| Contact fields | YES | YES |
| Manual confirmation flag | YES | YES |
| Price / commission / ranking | NO (AgencyOffer ≠ Price) | NO |
| Departure scope / commercial notes | Panel-only | NO on public |

**Booking CTA:** Requires selection when ≥2 offers (`tour-commerce-panel.tsx`).

**Verdict:** **Selection works; comparison depth is the #1 P40 gap** — must use only contract-backed dimensions.

---

## 3. Customer dashboard assessment

| Section | Status |
|---------|--------|
| Overview, Bookings, Payments, Documents, Passengers, Notifications, Profile | **Empty foundation** |

**Shell:** `CustomerShell` — correct IA separation from agency/admin.

**Gap:** Live booking exists at `/bookings/[bookingId]` but **not aggregated** in `/me/bookings`.

**Verdict:** **READY_FOUNDATION** (P37-T002) — needs live read-model wiring before depth polish.

---

## 4. Agency portal assessment

| Section | Status |
|---------|--------|
| Dashboard | Empty foundation |
| Catalog / Offers | **Wired** (P38-T007) — list/create/lifecycle |
| Bookings, Customers | Empty foundation |
| Commission, Settlement | Empty foundation (finance skeleton exists backend; no fake KPIs) |
| Users, Profile | Empty foundation |

**Strengths:** Offer ops workflow; acting-agency isolation; lifecycle actions.

**Gaps:** TourProduct UUID input (technical); EN/FA label mix; no booking/customer roster; commission/settlement deferred correctly.

**Verdict:** **Offer ops ready; commercial ops depth needed** without fake financial dashboards.

---

## 5. Admin console assessment

| Section | Status |
|---------|--------|
| Dashboard | Workflow direction cards |
| Catalog ops | **Functional islands** (tours, places, departures, destinations) |
| Agency offer governance | **Wired** (P38-T010/T014) — queue, filter, approve, history |
| Content/Media | Islands linked |
| Reporting | Empty foundation (no fake charts) |
| Audit | Direction-only |

**Verdict:** **Governance + catalog ops strong**; reporting/data-grid depth and cross-domain workflows remain foundation-level.

---

## 6. Merchandising architecture assessment

**Not implemented:**

- Campaign / Promotion / Placement domain
- Audience/locale/channel targeting
- Sponsored/promoted visibility (truthful)
- Homepage editorial beyond API section composition

**Recommendation:** Plan for future model:

```text
Campaign → Promotion → Placement → Audience/Targeting
```

**Do not:** Hardcoded homepage banner system.

**Ownership questions:** Likely new bounded context or extension of Content/Search — **ADR may be required** before implementation (flag for Architect).

---

## 7. Design system gap assessment

**Exists:** `components/ui/*` — Container, Stack, Surface, Text, MoneyText, MixedCurrencyPrice, BidiText, MediaImage, RouteStatePanel, shells.

**Gaps for P40:**

| Component need | Gap |
|----------------|-----|
| Comparison table/cards | Missing |
| Filter chips / advanced filters | Partial |
| Data grid (admin) | Foundation only |
| Status chips (localized) | Enum strings leak English |
| Empty/loading patterns | Present but inconsistent |
| Sticky commerce actions | Tour detail only |
| Chart primitives | Correctly absent (no fake data) |

Reference: `docs/product-experience/DESIGN-SYSTEM-2.0.md` — FOUNDATION LOCKED; token maturity incomplete (P36).

---

## 8. Content / media debt assessment

| Issue | Evidence |
|-------|----------|
| DEMOFEED slugs/titles on cards | P36 gate |
| Demo chip on public surfaces | `tour-card.tsx`, `hotel-card.tsx` |
| Sparse catalog density | P36/P38 gates |
| Destination cover media gaps | demo-media README |
| Description noise filters | demofeed sample data strings |
| Sample-data disclosure | Partial — demo chip only |

**Verdict:** Hygiene task needed; **no fabricated claims** to fill gaps.

---

## 9. SEO / accessibility / performance risk assessment

| Risk area | P40 guardrail |
|-----------|---------------|
| Server Components First | Preserve on new islands |
| Semantic headings | Audit comparison UI when built |
| Canonical/indexation | SEO module owns; don't duplicate |
| Locale routing / RTL/LTR | BidiText + shell direction |
| Focus / reduced motion | DS checklist on new components |
| Image performance | MediaImage aspect ratios |
| Core Web Vitals | Avoid client-heavy comparison tables |

---

## 10. Visual review evidence

Runtime screenshots not captured in this session (docs-only audit from code + prior gate evidence).

**Prior evidence referenced:**

- `docs/product-experience/evidence/P36-GATE/`
- `docs/product-experience/evidence/P37-T002/` – `P37-T004/`
- `docs/product-experience/evidence/P38-T004/` (offer selection API)
- `docs/product-experience/assets/travelcore-ui-ux-north-star.png`

**Recommendation:** P40-T002+ implementation tasks should include viewport screenshots per visual acceptance checklist.

---

## 11. Technical label leakage (observed)

- Policy/requirement **codes** on tour detail
- English: `Pending`, `Agency Offers`, breadcrumbs
- DEMOFEED slug prefixes (partially humanized)
- TourProduct UUID in agency create form
- URL `agencyOfferId` GUID (acceptable for deep links)

---

## 12. Audit summary matrix

| Surface | Verdict | Top gap |
|---------|---------|---------|
| Public marketplace | PARTIAL_SELLABLE | Comparison + merchandising depth |
| Multi-agency comparison | **NOT STARTED** | Side-by-side truthful dimensions |
| Customer dashboard | FOUNDATION | Live booking aggregation |
| Agency portal | PARTIAL (offer ops) | Bookings/customers workflow |
| Admin console | PARTIAL (gov/catalog) | Reporting + grid depth |
| Merchandising architecture | **NOT STARTED** | Campaign/placement model |
| Design system | FOUNDATION | Comparison + grid + status i18n |
| Content/media | DEBT | DEMOFEED hygiene |
