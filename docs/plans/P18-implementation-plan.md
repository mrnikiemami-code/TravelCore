# P18 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P18-PLAN` |
| Phase | P18 — Trip Planner / Lead Experience |
| Status | PLAN ACCEPTED; **P18-R1–R6 RESOLVED**; T001–T006 delivered; **P18-R7–R8 OPEN** |
| Baseline | `1826013` (`docs(tripplanner): add P18 implementation plan [TC-P18-PLAN]`) · T001 on top |
| Authoritative sources | `docs/ROADMAP.md` § P18 · `docs/PROJECT-STATE.md` · `04-module-boundaries.md` · `05-dependency-rules.md` · `07-data-architecture.md` · `docs/domain/module-ownership-matrix.md` · `13-reference-page-archetypes.md` · `docs/pages/00-page-archetype-registry.md` · `docs/pages/09-page-state-and-composition-rules.md` · P04 Destination/ReferenceData · P05 SEO · P08 Content · P09 Tour · P12 Pricing · P13 AgencyMarketplace · P14 PublicExperience · P15 Search · P16 UGC · P17 Visa · P19 Booking · P20 Payment · `15-future-architecture-transition-map.md` § V Notification |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P18** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P17-GATE` ACCEPT. Next phase is **explicitly** `P18 — Trip Planner / Lead Experience` in `docs/ROADMAP.md` (not guessed). Under PIPELINE continuity, ceremonial confirms and ceremonial Gate waits are **not required**. **No product code in PLAN task.** Open R# must stay OPEN until architect lock. **Do not implement T001 until this PLAN is ACCEPTED and P18-R1 is architecturally locked.**

---

## 0. Next-phase resolve (from SoT; no extra discovery task)

| Question | Answer from SoT |
|----------|-----------------|
| P17 completion | **COMPLETE / ACCEPTED** — Gate `f439924` |
| Authoritative next phase ID | **P18** |
| Title / purpose | **Trip Planner / Lead Experience** — help a visitor express travel intent and request assistance without prematurely creating Booking, CRM, Payment, or a generic workflow engine |
| PLAN already existed? | **NO** — this document is the first P18 PLAN |
| SoT conflict? | **NO** — ROADMAP names P18 after P17; no competing phase ordering |
| Dedicated module/schema in SoT today? | **NO** — Trip Planner / Lead is ROADMAP-listed only; no `trip_planner` / `lead` schema yet; Notification module not implemented (transition map § V only) |
| Missing business fact blocking PLAN authorship? | **NO** — PLAN may enumerate R# and IN/OUT/DEFER without locking product semantics |
| Invented phase? | **NO** — P18 is already listed in ROADMAP |

---

## 1. Phase Purpose

P18 باید تجربهٔ **Trip Planner / Lead** را معرفی کند: کمک به بیان نیاز سفر و درخواست پیگیری/مشاوره — **بدون** دزدیدن مالکیت Tour، Destination، Place، Pricing، AgencyMarketplace، Party، Search، SEO، Booking، Payment، Notification delivery، یا CRM کامل.

هدف (از Roadmap + accepted prior phases):

1. **Trip Intent ≠ Lead ≠ Booking** — Intent = آنچه مسافر می‌خواهد؛ Lead = درخواست ارسال‌شده برای پیگیری انسانی/تجاری؛ Booking = فرآیند رزرو/تراکنش (P19).
2. **TripPlanner ≠ Search** — Planner ممکن است Search/Destination/Tour **read contracts** را مصرف کند؛ موتور index/ranking/faceting مالکیت P15 است.
3. **Lead Experience ≠ CRM by default** — P18 نباید بدون قفل صریح، sales pipeline / tasks / campaigns / call-center را مالک شود.
4. **Lead contact ≠ Party master identity** — ارسال فرم planner نباید به‌تنهایی Party/Identity بسازد مگر R3 صریحاً قفل کند.
5. **BudgetPreference ≠ Price/Quote** — بودجه/ترجیح مالی preference است؛ Pricing مالک Price/Quote (P12).
6. **PlannerTravelerPreference ≠ Booking Passenger** — ترکیب مسافران preference است؛ رکورد مسافر رزرو P19 است.
7. **PublicExperience = composition only** — entry pointها و UX عمومی؛ Lead/TripIntent SoT در ماژول P18 (پس از R1).
8. **Notification = delivery owner** — تأییدیه/هشدار از طریق قرارداد Notification؛ Lead مالک SMTP/SMS/provider نیست (module هنوز پیاده نشده — transition map § V).
9. **Visa assistance preference ≠ VisaApplication** — P17-R8: Visa policy complete; applicant case deferred. Planner فقط preference/flag informational.
10. **AI-readiness = structured intent facts** — destination flexibility, traveler composition, budget band, interests — **بدون** LLM/embeddings/vector/RAG/itinerary generator.

P17 تحویل داد: Visa structured facts + public read + application boundary deferred.

P14 تحویل داد: Sticky Action ≠ Booking؛ **Contact / Request Information** as presentation affordance only (`PublicDetailStickyActions`) — P18 may eventually back this with real Lead submission without stealing PE ownership.

P18 اضافه می‌کند: **Trip Planner / Lead capability** — **بدون** Booking، بدون Payment، بدون full CRM، بدون agency ranking engine، بدون Search engine، بدون recommendation/AI itinerary.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P17 Gate | `TC-P17-GATE` COMPLETE / ACCEPTED (`f439924`) |
| P17 evidence | [`P17-GATE-acceptance-evidence.md`](P17-GATE-acceptance-evidence.md) · [`P17-T009-hardening-and-evidence-pack.md`](P17-T009-hardening-and-evidence-pack.md) |
| P17 Plan | ACCEPTED · R1–R8 RESOLVED · T001–T009 ACCEPTED |
| Baseline HEAD | `f439924` |
| P00–P17 | COMPLETE |
| Trip Planner / Lead module | **Scaffolding delivered** (`TC-P18-T001` / schema `trip_planner`) |
| Notification module | **Not implemented** (architecture docs only) |
| Booking / Payment | Modules do not exist (P19 / P20) |
| Existing public contact affordance | P14 `Contact / Request Information` anchor — presentation only, no Lead backend |

---

## 3. Scope classification (IN / OUT / DEFER)

| Concept | Classification | Notes |
|---------|----------------|-------|
| Trip intent expression (structured preferences) | **IN** | Core P18 purpose |
| Destination preferences (logical Destination/country refs) | **IN** | No geo clone |
| Flexible / approximate travel dates | **IN** | exact · range · season · undecided |
| Traveler counts/categories (adult/child/infant preference) | **IN** | **≠ Booking Passenger** |
| Budget preference / band | **IN** | **≠ Price/Quote** |
| Trip style / interests / pace | **IN** | structured codes/rows |
| Accommodation preference | **IN** | preference only, not Hotel catalog |
| Transportation preference | **IN** | preference only, not Flight inventory |
| Visa assistance preference | **IN** | flag/preference · **≠ VisaApplication** |
| Free-form notes | **IN** | bounded text; no document upload |
| Contact fields for follow-up (name/email/phone) | **IN** | data minimization required |
| Consent / contact permission facts | **IN** | privacy boundary (R7) |
| Lead submission / acknowledgment UX | **IN** | honest conversion, not fake booking |
| Minimal submitted-lead persistence | **IN candidate** | subject to R1/R2 lock; not assumed in PLAN |
| Client/session-only draft before submit | **IN candidate** | subject to R3 |
| Public planner route + progressive UX | **IN** | mobile-first · skip optional steps |
| PublicExperience entry-point composition | **IN** | PE composes; does not own Lead SoT |
| Search/Destination/Tour read consumption | **IN** | read contracts only |
| Basic lead status (e.g. Submitted) | **DEFER** | full lifecycle R5 |
| Lead qualification pipeline | **DEFER** | not full CRM |
| Agency routing / assignment / ranking | **DEFER** | AgencyMarketplace commercial boundary |
| Notification provider / email-SMS infra | **DEFER** | Notification module not built; plan contracts only |
| Account-owned saved drafts | **DEFER** | Identity optional; R3 |
| Authenticated planner requirement | **DEFER** | anonymous-first likely; R3 |
| Booking / Quote / reservation | **OUT** | P19 |
| Payment / checkout | **OUT** | P20 |
| Passport / identity document collection | **OUT** | P17 applicant boundary |
| Full CRM (pipeline, tasks, campaigns) | **OUT** | unless future explicit phase |
| Sales automation / call-center | **OUT** | |
| Generic workflow / BPM engine | **OUT** | |
| Agency ranking / commission / allocation | **OUT** | P13 boundaries |
| Search engine / ranking / faceting | **OUT** | P15 |
| Recommendation / personalization engine | **OUT** | |
| AI itinerary generation / LLM / RAG | **OUT** | |
| SEO IndexPolicy ownership transfer | **OUT** | P05 |
| Content CMS ownership of lead facts | **OUT** | P08 |

---

## 4. Non-goals (explicit)

1. Booking engine / reservation / Quote acceptance (P19).
2. Payment / checkout (P20).
3. Applicant/passport/document collection (P17 deferred application).
4. Full CRM / sales pipeline / marketing automation.
5. Call-center or generic workflow/BPM engine.
6. Agency ranking / seller selection algorithm / commission allocation.
7. Search engine / FTS / Elasticsearch / ranking implementation (P15).
8. Recommendation / personalization / AI itinerary generator.
9. Notification provider implementation (plan contracts only until Notification phase).
10. Cloning Destination / TourProduct / Place / Agency aggregates.
11. Budget preference becoming Price/Quote authority.
12. Lead submission auto-creating Party master records without R3 lock.
13. Fake urgency / fake availability / misleading Book Now language.
14. LLM / embeddings / vector DB / RAG infrastructure.
15. Inventing unlocked R# closures.
16. Next-phase product (P19 Booking implementation).

---

## 5. Task sequence (proposed)

### TC-P18-PLAN — this document

### TC-P18-T001 — Trip Planner / Lead module scaffolding / ownership boundary
- Purpose: Independent module + schema + ownership contracts (**P18-R1 RESOLVED**).
- Delivered: Contracts/Domain/Infrastructure scaffolding; schema `trip_planner`; `TripPlannerOwnershipBoundary`; opaque `TripPlannerLogicalReference`; host registration; architecture guardrails; persistence lifecycle smoke; no peer FKs; no product aggregates.
- Forbidden kept: TripIntent/Lead/preferences/lifecycle/routing/notification provider/identity tables · Booking/Payment tables · CRM pipeline · Search engine · peer FK · Party/Identity clone · inventing R2–R8.

### TC-P18-T002 — TripIntent vs Lead aggregate boundary
- Purpose: Separate mutable planning intent from submitted follow-up request (**P18-R2 RESOLVED**).
- Delivered: `TripIntent` + `Lead` aggregates; `LeadSubmissionSnapshot`; `TripIntentLeadSubmissionBoundary`; schema tables `trip_intents` + `leads`; snapshot invariant enforced.
- Preserve: **TripIntent != Lead** · **Lead != Booking** · **Lead != Quote** · **Lead != CRM Opportunity**.
- Forbidden kept: full preferences (R4) · identity/contact (R3) · lifecycle pipeline (R5) · routing (R6) · consent/notification (R7) · public UI/API (R8) · inventing R3–R8.

### TC-P18-T003 — Anonymous vs authenticated planner / contact identity
- Purpose: Anonymous-first TripIntent + optional actor + submission-time contact (**P18-R3 RESOLVED**).
- Delivered: `PlannerActorReference`; `LeadContactSnapshot`; `TripIntentDraftAccessToken`; optional actor on TripIntent/Lead; contact snapshot on Lead; no Identity/Party clone.
- Preserve: **Lead contact != Party master identity** · **LeadContactSnapshot != Party** · **LeadContactSnapshot != Identity Account**.
- Forbidden kept: consent (R7) · preferences (R4) · lifecycle (R5) · routing (R6) · public UI/API (R8) · inventing R4–R8.

### TC-P18-T004 — Travel preference model
- Purpose: Structured destination/date/travelers/interests/budget/accommodation/transport/visa-assistance preferences (**P18-R4 RESOLVED**).
- Delivered: `TravelPreferences` on TripIntent; `TravelPreferenceSnapshot` on Lead submission; timing/travelers/budget/destination/interest controlled types; logical destination refs only; migration `20260818040000_AddTravelPreferencesBaseline`.
- Preserve: **BudgetPreference != Price/Quote** · **PlannerTravelerComposition != Booking Passenger** · date flexibility semantics · no false precision.
- Forbidden kept: lifecycle (R5) · routing (R6) · consent/notification (R7) · public UI/API (R8) · inventing R5–R8.

### TC-P18-T005 — Lead lifecycle / qualification boundary
- Purpose: Minimal controlled lifecycle vs deferred qualification/CRM (**P18-R5 RESOLVED**).
- Delivered: `LeadStatus` = Submitted · Contacted · Closed · Cancelled; `LeadLifecycleBoundary` deterministic transitions; `StatusChangedAt`/`UpdatedAt`; migration `20260818050000_AddLeadLifecycleBaseline`.
- Preserve: **LeadStatus != CRM Pipeline Stage** · **Lead != CRM Opportunity** · **Contacted != Qualification** · **Closed != Booking conversion**.
- Forbidden kept: Qualified/Won/Lost/Converted · agency routing (R6) · consent/notification (R7) · public UI/API (R8).

### TC-P18-T006 — Agency routing / assignment boundary
- Purpose: Close P18-R6 by explicitly **DEFERRING** agency routing product implementation (**P18-R6 RESOLVED**).
- Delivered: `TripPlannerAgencyRoutingBoundary`; guardrails proving no AgencyAssignment/AgencyId/routing tables; SoT records **P18 Agency Routing = DEFERRED**.
- Preserve: **Lead != AgencyAssignment** · **TripPlanner != AgencyMarketplace** · **LeadStatus != AgencyAssignmentStatus** · **Lead Routing != Search Ranking**.
- Forbidden kept: routing engine · agency ranking · commercial allocation · agency acceptance workflow · notification (R7) · public UI (R8).

### TC-P18-T007 — Notification / consent / privacy boundary
- Purpose: Delivery handoff + consent/contact permission + data minimization (**P18-R7**).
- Preserve: **Lead ≠ Notification infrastructure** · no passport/document PII · public vs admin visibility separation.
- Forbidden kept: SMTP/SMS provider implementation · inventing R8.

### TC-P18-T008 — PublicExperience composition + Search/Booking/CRM boundary
- Purpose: Public entry points, dedicated planner route composition, honest CTA semantics (**P18-R8**).
- Preserve: **PublicExperience ≠ Lead Source of Truth** · **TripPlanner ≠ Search** · no fake booking/checkout.
- Forbidden kept: PE-owned persistence · Search engine · Booking CTA · inventing beyond R8.

### TC-P18-T009 — Hardening + evidence
- Purpose: Harden P18 boundaries and produce gate evidence (**no new capability**).
- Forbidden kept: new planner product · next-phase work.

### TC-P18-GATE — Acceptance Gate
- Evidence only. Ceremonial Gate wait is **not** a pipeline stop.
- No new Trip Planner product capability. Do not start P19 inside GATE.

Do not manufacture empty capabilities merely to fill numbering. T006 may remain boundary/docs-only if agency routing stays DEFER.

---

## 6. Open decisions (must not invent)

| ID | Topic | Status | SoT notes (not a lock) |
|----|-------|--------|------------------------|
| **P18-R1** | Trip Planner / Lead module ownership and schema | **RESOLVED** | Independent TripPlanner module. Schema `trip_planner`. Owns future trip-intent/lead facts and lifecycle — not Tour, Destination, Place, Pricing, AgencyMarketplace, Search, SEO, Booking, Payment, Notification delivery, CRM, or Party/Identity master data. No peer-schema FK. T001: no TripIntent/Lead/preferences/lifecycle/routing/notification provider/identity product types. |
| **P18-R2** | TripIntent vs Lead aggregate boundary | **RESOLVED** | **TripIntent** = mutable planning intent. **Lead** = submitted request for follow-up. **Lead ≠ Booking**. Do not collapse. Submission copies snapshot; later TripIntent mutation must not change existing Lead. T002: no full preferences/identity/lifecycle/routing. |
| **P18-R3** | Anonymous vs authenticated planner / contact identity | **RESOLVED** | Anonymous-first TripIntent without Account requirement. Optional `PlannerActorReference` (opaque logical id). `LeadContactSnapshot` at submission — **Lead contact != Party master identity**; **LeadContactSnapshot != Party/Identity**. Minimal `TripIntentDraftAccessToken` for anonymous draft retrieval. No Party/Identity clone, no AnonymousUser platform, no consent finalization (R7). |
| **P18-R4** | Travel preference model | **RESOLVED** | Destination/date/travelers/interests/budget/accommodation/transport preferences. **BudgetPreference != Price/Quote**. **PlannerTravelerComposition != Booking Passenger**. Date semantics: exact · flexible range · season · undecided. Logical refs to Destination/Tour — no clone. T004 delivered. |
| **P18-R5** | Lead lifecycle / qualification boundary | **RESOLVED** | Minimal baseline: **Submitted · Contacted · Closed · Cancelled**. Full qualification/CRM pipeline **DEFERRED**. No generic workflow engine. T005 delivered. |
| **P18-R6** | Agency routing / assignment boundary | **RESOLVED (DEFERRED)** | **P18 Agency Routing = DEFERRED**. No routing engine, assignment persistence, agency ranking, commercial allocation, or agency acceptance workflow in P18. Future capability may orchestrate without merging SoT. T006 delivered. |
| **P18-R7** | Notification / contact / privacy-consent boundary | **OPEN** | Notification owns delivery channels (module not built). Lead may emit semantic events/contracts for acknowledgment/internal alert. Plan consent, retention posture, access control, data minimization. No passport/document collection. |
| **P18-R8** | PublicExperience composition and Booking/Search/CRM boundary | **OPEN** | PE composes entry points (nav · tour detail · destination · visa · dedicated route). **PublicExperience ≠ Lead SoT**. **TripPlanner ≠ Search**. Honest CTA — no fake Book Now. P14 contact affordance may connect later without PE owning persistence. |

---

## 7. Architecture invariants (carry forward)

1. TripPlanner != Booking · TripPlanner != Search · Lead != Quote · Lead != Payment.
2. TripIntent != Lead · Lead != Booking.
3. Lead Experience != CRM by default.
4. Lead contact != Party master identity (until R3 lock says otherwise).
5. LeadContactSnapshot != Party · LeadContactSnapshot != Identity Account.
6. BudgetPreference != Price · BudgetPreference != Quote.
6. **PlannerTravelerComposition != Booking Passenger**.
7. **Visa assistance preference ≠ VisaApplication** (P17-R8 deferred).
8. **PublicExperience ≠ Lead Source of Truth**.
9. Destination/ReferenceData own geography; Tour owns catalog; Planner holds preferences with logical refs only.
10. Search owns retrieval; Planner consumes read contracts only.
11. AgencyMarketplace owns commercial relationship; P18 does not own offer ranking/allocation by default.
12. Notification owns delivery; Lead does not own email/SMS infrastructure.
13. SEO owns IndexPolicy; planner route existence ≠ automatically indexed.
14. Structured attributable locale-aware facts first; no AI infrastructure in P18.
15. Mobile-first · progressive disclosure · optional fields · summary before submit · accessible · no dark patterns.
16. FA / EN / AR · RTL/LTR · bidi-safe numbers/dates/contact values.
17. Do not invent unlocked R# closures.

---

## 8. UX / conversion posture

Plan for:

- mobile-first, low-friction, short steps with clear progress
- progressive disclosure; skip nonessential questions
- summary/review before submission
- optional fields where architecturally valid
- accessible keyboard/touch targets
- honest language: request assistance / plan my trip — **not** Book Now / Pay Now / Checkout when only a lead is collected
- disable misleading CTAs when backend capability absent (`docs/pages/09-page-state-and-composition-rules.md`)

Potential entry points (composition only — not implemented in PLAN):

- global navigation · dedicated `/[locale]/plan` (or equivalent) route
- Tour detail `#request-information` / sticky contact affordance (P14)
- Destination landing · Visa detail "consultation" honesty (P17)

---

## 9. Privacy / consent posture

Lead data may contain personal contact information. P18 planning must preserve:

- data minimization (no passport, no identity documents, no unnecessary PII)
- explicit consent/contact permission where required
- retention/access-control posture (public submit vs admin/ops visibility)
- separation from Visa applicant PII boundary (P17-R8)

Do **not** collect sensitive documents in a trip-planning lead.

---

## 10. Repository safety

- Branch `main` · fast-forward push only · no force · CLEAN working tree before RESULT.
- One docs commit for PLAN (no product code).
- After PLAN ACCEPT, Auto-Execute first locked product task only when architect envelope names it **and** P18-R1 is locked.
- Do not start T001 from this PLAN document alone.
