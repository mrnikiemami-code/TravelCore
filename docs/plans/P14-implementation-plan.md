# P14 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P14-PLAN` |
| Phase | P14 — Public Tour Experience |
| Status | IN PROGRESS — PLAN ACCEPTED · P14-R1–R5 RESOLVED; T005 related tours composition delivered |
| Baseline | `c0bcd78` (`docs: P13 acceptance gate evidence [TC-P13-GATE]` — **TC-P13-GATE** ACCEPTED; P13 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P14 · P09–P13 Gates · P05 SEO · P08 Content · P11-R8 Published ≠ Bookable · P12-R8 public price read · P13-R7 Published Offer ≠ SEO Indexed · architect P13 Gate ACCEPT (Public Experience ≠ Booking · SEO Page ≠ Commercial Transaction · Content ≠ Catalog Ownership) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P14** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** + architect P13 Gate ACCEPT continuity (auto-start P14 PLAN). Under PIPELINE continuity, ceremonial confirms and ceremonial Gate waits are **not required**. **No product code in PLAN task.**

---

## 1. Phase Purpose

P14 باید **تجربهٔ عمومی تور** را از walking-skeleton / Admin-first به سطح production-ready برساند، بدون دزدیدن مالکیت Catalog، SEO engine، Search engine، Pricing، Booking، یا Marketplace.

هدف (از Roadmap + Gate ACCEPT):

1. **Public Experience ≠ Booking** — صفحهٔ عمومی تور فروش/رزرو نیست؛ Published ≠ Bookable باقی می‌ماند.
2. **SEO Page ≠ Commercial Transaction** — landing/indexation با Checkout/Payment قاطی نمی‌شود.
3. **Content ≠ Catalog Ownership** — غنی‌سازی نمایش از Content/SEO استفاده می‌کند؛ TourProduct SoR کاتالوگ می‌ماند.
4. **Search URL ≠ SEO Landing URL** — listing/query جدا از canonical landing (P15 موتور جستجو است؛ P14 نباید Search engine بسازد مگر قفل معمار).
5. سطح عمومی: Tour Landing · Destination Tour Landing · Tour Listing · Foreign Tour Detail · Experience Tour Detail · Filters/Sorting/Pagination · mobile filters · sticky/mobile booking **actions as display** (نه Booking engine) · Related tours · تمایز SEO landing.
6. اعتبارسنجی: RTL · LTR · Mobile · Desktop · Accessibility · SEO · Performance.

P13 تحویل داد: Agency Marketplace مستقل + AgencyProfile + AgencyOffer + Panel + publication (≠ SEO).  
P14 اضافه می‌کند: **Public Tour Experience factory** — **بدون** Booking engine، بدون Payment، بدون Search indexing engine، بدون تکرار TourProduct.

P14 **Search (P15)** · **UGC (P16)** · **Visa (P17)** · **Booking/Payment** نیست.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P13 Gate | `TC-P13-GATE` COMPLETE / ACCEPTED (`c0bcd78`) |
| P13 evidence | [`P13-GATE-acceptance-evidence.md`](P13-GATE-acceptance-evidence.md) · [`P13-T009-hardening-and-evidence-pack.md`](P13-T009-hardening-and-evidence-pack.md) |
| P13 Plan | ACCEPTED · R1–R7 RESOLVED · T008 vacant |
| Baseline HEAD | `c0bcd78` |
| P00–P13 | COMPLETE |
| Public Tour detail | Live `features/tour-detail` + public price summary (P12-R8); **no Book Now** |
| P02 walking skeleton | `foreign-tour-detail` fixture + `BookingCtaIsland` — fixture only, not live Booking |
| SEO | Route/IndexPolicy SoR (P05); TourProduct SEO hooks exist |
| Content | Editorial CMS (P08) — links/enrichment ownership **UNRESOLVED for P14** |
| Marketplace public | AgencyOffer publication exists; public Offer display **UNRESOLVED** |
| Search | **Not implemented** (P15) |

---

## 3. Non-goals (explicit)

1. Booking engine / reservation / hold / inventory consumption / live Book Now checkout.
2. Payment capture / refund / settlement.
3. PostgreSQL FTS / `pg_trgm` Search engine / autocomplete / faceting engine (P15).
4. Duplicating `TourProduct` / `TourDeparture` as a second public catalog SoR.
5. Moving IndexPolicy ownership out of SEO.
6. Agency Marketplace ranking / commission / public seller portal expansion.
7. Inventing unlocked R# closures — open decisions stay OPEN until architect lock.

---

## 4. Task sequence (proposed)

### TC-P14-PLAN — this document

### TC-P14-T001 — Public experience ownership / surface inventory
- Purpose: Lock which public surfaces P14 owns vs Tour/SEO/Content/Search (**P14-R1 RESOLVED**).
- Architect lock: Public Experience Surface belongs to Public Experience Layer — not Search, not Catalog. Surfaces: Detail / Listing / Landing. P14 = Presentation + SEO composition. P15 owns Query/Ranking/FTS.
- Delivered: `PublicExperience.Contracts` · `PublicExperienceSurfaceKind` · ownership boundary · frontend `features/public-experience` · guardrails. No schema / no Booking / no Search engine.
- Forbidden: Booking · Search engine · new catalog SoR.

### TC-P14-T002 — Foreign / Experience public detail production baseline
- Purpose: Production public detail with mobile-first sticky experience actions (**P14-R2 RESOLVED**).
- Architect lock: Sticky Action ≠ Booking. Allowed: View Departure · View Price Summary · Contact / Request Information. Forbidden: Book Now · Pay Now · Reserve Seat · Checkout.
- Delivered: `PublicDetailStickyActions` on live `TourDetailView` · presentation anchors only.
- Forbidden: Booking CTA · Payment · Reservation · Search engine · Pricing ownership.

### TC-P14-T003 — Tour / Destination landing distinction
- Purpose: SEO landing vs listing URL (needs **P14-R3**). Search URL ≠ SEO Landing URL.
- Architect lock: Listing and SEO Landing are two surfaces. Listing = Discovery (`/tours` + query). Landing = Search Intent (`/tours/{topic}/{intent}`). Landing ≠ filtered listing. P15 owns Query/Ranking/FTS. SEO owns IndexPolicy.
- Delivered: listing/landing contracts · `/tours` listing route · `/tours/{topic}/{intent}` landing route · composition slots only.
- Forbidden: Search engine · Ranking · FTS · Faceting engine · SEO IndexPolicy ownership.

### TC-P14-T004 — Shared and specialized Tour Detail composition
- Purpose: Shared public Detail shell with kind-specific section composition (**P14-R4 RESOLVED**).
- Architect lock: Shared Shell + Common Sections + Kind-specific Sections. Not independent Experience/Package pages. Not a giant union ViewModel. Package specialty remains future contributor.
- Delivered: `PublicExperienceDetailComposition` · shared `TourDetailView` shell · Experience sections · Tour public `experience/presentation` read of existing facts · destinations/policies compose.
- Forbidden: Booking · Search engine · Pricing ownership · new Tour domain facts · Package domain implementation · IndexPolicy ownership.

### TC-P14-T005 — Related Tours composition baseline
- Purpose: Related tours as presentation/composition (**P14-R5 RESOLVED**). Sticky/mobile chrome already shipped in T002.
- Architect lock: Public Experience owns presentation only. Related ≠ Recommendation. Related ≠ Search ranking. Deterministic same-destination retrieval behind a Tour public-read boundary so P15 can replace it later. Published products only. Compact cards (max 6); do not push primary content down.
- Delivered: `PublicExperienceRelatedToursBoundary` · Tour `related-published` public read · compact cards on Detail + Landing.
- Forbidden: Recommendation engine · Ranking · FTS · `pg_trgm` · popularity/personalization · Booking.

### TC-P14-T006 — Content enrichment hooks
- Purpose: Content enrichment without stealing catalog (needs **P14-R6**).
- Forbidden: Content owning TourProduct · Search recommendations engine.

### TC-P14-T007 — Public AgencyOffer display (if locked)
- Purpose: Show published marketplace offers on public tour experience **only if P14-R7 locked**.
- Explicit DEFER allowed. Published Offer ≠ SEO Indexed · ≠ Bookable.

### TC-P14-T008 — Hardening + evidence

### TC-P14-GATE — Acceptance Gate
- Evidence only. Ceremonial Gate wait is **not** a pipeline stop. Continuity may auto-start **P15 PLAN** after ACCEPT unless a real Stop Condition applies.

---

## 5. Open decisions (must not invent)

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P14-R1** | Which public surfaces are in P14 vs deferred to P15 Search | **RESOLVED** | Public Experience Layer owns Detail/Listing/Landing presentation. Not Search. Not Catalog. P14 = Presentation + SEO composition. P15 owns Query/Ranking/FTS. |
| **P14-R2** | Sticky/mobile booking **actions** vs Published ≠ Bookable | **RESOLVED** | Sticky Action ≠ Booking. Allowed: View Departure · View Price Summary · Contact / Request Information. Forbidden: Book Now · Pay Now · Reserve Seat · Checkout. Published ≠ Bookable. |
| **P14-R3** | Listing URL vs SEO landing URL ownership | **RESOLVED** | Listing and SEO Landing are two surfaces. Listing = Discovery. Landing = Search Intent. Landing ≠ filtered listing. P15 owns Query/Ranking/FTS. SEO owns IndexPolicy. |
| **P14-R4** | Shared Tour public detail vs Foreign/Experience specialized pages | **RESOLVED** | Shared Shell + kind-specific sections. Not independent pages. Not a giant union ViewModel. Package specialty is future contributor only. |
| **P14-R5** | Related tours owner | **RESOLVED** | Public Experience owns presentation only. Deterministic shared-destination retrieval behind Tour public-read. Related ≠ Recommendation. P15 may replace retrieval. |
| **P14-R6** | Content enrichment vs Content CMS ownership | **UNRESOLVED** | Content ≠ Catalog. Editorial blocks may display; TourProduct remains SoR. |
| **P14-R7** | Public AgencyOffer on tour experience | **UNRESOLVED** | P13 publication exists. Public seller listing may be DEFER. Published Offer ≠ SEO Indexed. |
| **P14-R8** | Filters/facets implementation vs P15 | **UNRESOLVED** | Simple catalog filters in P14 vs faceting engine in P15. |

---

## 6. Architecture invariants (carry forward)

1. Public Experience ≠ Booking · SEO Page ≠ Commercial Transaction · Content ≠ Catalog Ownership.
2. TourProduct ≠ TourDeparture · Published Departure ≠ Bookable.
3. Price ≠ Quote ≠ Payment / Booking Amount.
4. Published Offer ≠ SEO Indexed · CatalogStatus ≠ PublicationStatus ≠ IndexPolicy.
5. Agency ≠ Party ≠ Pricing ≠ Booking ≠ TourProduct.
6. Search URL ≠ SEO Landing URL.
7. No Booking/Payment/Search-engine/FX engines in P14 unless a later lock says otherwise.
8. Do not duplicate TourProduct as a second catalog.

---

## 7. Continuity

After `TC-P14-GATE` ACCEPT, continuity may auto-start **P15 PLAN** (Search) unless a real Stop Condition applies.

**Pipeline rule (USER lock):** ceremonial Gate is **not** a stop. Cursor stays in PIPELINE and continues.

---

## 8. PLAN acceptance criteria

- [x] Phase purpose + non-goals explicit
- [x] Task sequence proposed without product code
- [x] Open decisions listed (R1–R8) — no invention
- [x] Baseline = P13 Gate ACCEPT commit `c0bcd78`
- [x] Architect ACCEPT plan + lock **P14-R1** then Auto-Execute `TC-P14-T001`
