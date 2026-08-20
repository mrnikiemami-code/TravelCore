# P30 — TravelCore Product Experience Foundation

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P30-PLAN` |
| Phase | P30 — Product Experience Foundation |
| Status | **IN PROGRESS** — PLAN ACCEPTED · `TC-P30-T002` PASS · `TC-P30-T003` PASS / AWAITING_ARCHITECT_REVIEW |
| Baseline | `6b0e4af` (`docs: add DEMOFEED implementation plan`) |
| Authoritative sources | Architect lock 2026-08-20 · P02 UI constitution · ADR 0005/0006 · HOMFEED/MODOPS/HOTIDX retroactive ledger · Product Experience Constitution |
| Frontend root | `src/frontend/web` |
| Product code in this PLAN | **NO** |

Convert TravelCore from a **backend platform** into a **sellable travel commerce platform** — Experience first, then data, then commercial features.

> Envelope: `TC-P30-PLAN` ACCEPTED · `TC-P30-T003` docs only · **do not execute `TC-P30-T004` / Checkpoint A from T003 result**.

**Product Experience SoT:**

- [`docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md`](../product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md)
- [`docs/product-experience/DESIGN-SYSTEM-2.0.md`](../product-experience/DESIGN-SYSTEM-2.0.md)
- [`docs/product-experience/assets/travelcore-ui-ux-north-star.png`](../product-experience/assets/travelcore-ui-ux-north-star.png)
- Persistent T002 envelope: [`docs/plans/TC-P30-T002-task-envelope.md`](TC-P30-T002-task-envelope.md)
- Persistent T003 envelope: [`docs/plans/TC-P30-T003-task-envelope.md`](TC-P30-T003-task-envelope.md)

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior SoT next | `TC-DEMOFEED-PLAN` authored |
| Architect lock | **DEMOFEED / Feed is not P30 priority** |
| P30 vs copy | Benchmark LastSecond / Tahagasht / Booking / Airbnb / Tripadvisor — **output is TravelCore Design Language, not a clone** |
| Backend redesign | **FORBIDDEN** |
| New domain features | **FORBIDDEN** in P30 |
| Page-first UI | **FORBIDDEN** — Design System before pages |

---

## 1. Phase purpose

Lock **Product Experience Architecture** (not another domain module).

If this phase succeeds, the same language lands on:

- Public marketplace
- Admin console
- Agency (B2B) portal
- Internal workflows

Order of work (locked):

1. **Experience**
2. **Data** (DEMOFEED after P30, not before)
3. **Commercial features**

---

## 2. Locked decisions

### 2.1 Design is not copy

Benchmark (analysis only):

| Surface | References |
|---------|------------|
| Public | LastSecond · Tahagasht · Booking · Airbnb · Tripadvisor |
| Admin | Stripe Dashboard · Linear · Vercel Dashboard · Shopify Admin |
| B2B | Travel agency systems (pattern, not clone) |

Output: **TravelCore Design Language**.

### 2.2 Design System before pages

**No Page-First Development.**

First: Typography · Color · Spacing · Card · Form · Grid · Navigation · Feedback states. Then pages.

### 2.3 Real mobile-first

Every in-scope surface: Desktop PASS · Mobile PASS · Tablet PASS.

### 2.4 One design system, three experiences

| Experience | Audience | Jobs |
|------------|----------|------|
| A Public Marketplace | Traveler | Search · Discovery · Booking · Content |
| B Admin Console | Operator | Manage · Operate · Analyze |
| C Agency Portal | Agency | Sell · Quote · Manage customers · Track commission |

Not three unrelated UIs.

### 2.5 P30 OUT

- Feed / DEMOFEED execution
- New domain features
- Backend redesign
- Scraping
- Pixel-clone of competitor sites

---

## 3. Ordered task map (refined · checkpoints locked)

| Task / Checkpoint | Deliverable |
|-------------------|-------------|
| `TC-P30-PLAN` | This plan + SoT lock — **ACCEPTED** |
| `TC-P30-T002` | Product Experience Constitution + North Star lock + Public/Admin/Agency specs + Visual Acceptance Protocol + Recovery lock |
| `TC-P30-T003` | Design System 2.0 (brand, tokens, public + admin components) |
| **Visual Checkpoint A** | Design primitives / representative component board |
| `TC-P30-T004` | Application shells (public / admin / agency foundation) |
| **Visual Checkpoint B** | Shell review |
| `TC-P30-T005` | Public Home experience |
| **Visual Checkpoint C** | Home review |
| `TC-P30-T006` | Hotel commerce experience (listing + detail; not HotelBooking availability engine) |
| **Visual Checkpoint D** | Hotel review |
| `TC-P30-T007` | Tour commerce experience (listing + detail; Pricing/Booking SoR unchanged) |
| **Visual Checkpoint E** | Tour review |
| `TC-P30-T008` | Admin experience foundation (shell + professional data-grid + workflow patterns) |
| **Visual Checkpoint F** | Admin review |
| `TC-P30-T009` | Agency portal foundation only (dashboard overviews — not full B2B rewrite) |
| **Visual Checkpoint G** | Agency review |
| `TC-P30-GATE` | Customer-facing acceptance of Public / Admin / Agency feeling |
| After GATE | DEMOFEED may be reconsidered / authorized |

---

## 4. Task intent (not implementation)

### T002 — Visual Benchmark & Product Direction

Analyze references. Produce `TravelCore Product Design Constitution` (docs + token direction). No production pages.

### T003 — Design System 2.0

**Status:** PASS / AWAITING_ARCHITECT_REVIEW (`TC-P30-T003`)

Foundation docs: [`DESIGN-SYSTEM-2.0.md`](../product-experience/DESIGN-SYSTEM-2.0.md) + `design-system/*`.

Brand · semantic colors · typography · spacing/radius/elevation · component principles · responsive/a11y.

Public lean set (later code): Hero · Search · Product cards · Trust · Footer.
Admin lean set (later code): Shell · Data grid · Filter · Form · Modal/Drawer · Stepper.

Reuse P02 primitives; do not fork a second system. **No product pages in T003.**

### T004 — Application Shell

Public: header · footer · navigation.  
Admin: sidebar · topbar · breadcrumb · command menu.  
Agency: dashboard shell.

### T005 — Public Home

Hero search · popular destinations · featured tours · recommended hotels · travel stories · trust · footer. Curated composition, not personalized ML feed.

### T006 — Hotel Commerce

Listing: filter · sort · grid · map-ready.  
Detail: gallery · facilities · location · reviews · similar. Place catalog SoR; HotelBooking unchanged.

### T007 — Tour Commerce

Listing + detail: itinerary · hotel · flight · price display · booking CTA. Tour/Pricing/Booking ownership unchanged.

### T008 — Admin Experience Foundation

Admin shell + data grid standard: server pagination · filtering · sorting · column control · export · bulk action · responsive.

### T009 — Agency Portal Foundation

Dashboard · sales overview · booking overview · customer overview. Not full P24 rewrite.

### GATE

| Surface | Customer quote (acceptance intent) |
|---------|-----------------------------------|
| Public | «این سایت گردشگری حرفه‌ای است.» |
| Admin | «این سیستم قابل استفاده عملیاتی است.» |
| Agency | «این ابزار فروش است.» |

---

## 5. DEMOFEED relationship

`TC-DEMOFEED-PLAN` remains **authored / DEFERRED**. Architect: feed fills an empty site but does not create product feeling. Execute DEMOFEED only after P30 Experience (separate envelope).

---

## 6. Preserved architecture

Modular Monolith · Place ≠ HotelBooking · Search ≠ discovery UI · UGC ≠ Content · Pricing SoR · Booking SoR · Server Components first · Locale ≠ Currency.

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial PLAN from architect P30 lock · docs only |
| 2026-08-20 | `TC-P30-T002` — Constitution · specs · visual protocol · checkpoint map |
| 2026-08-20 | `TC-P30-T003` — Design System 2.0 foundation docs (lean · commercial · no pages) |
