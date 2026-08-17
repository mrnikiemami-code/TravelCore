# P16 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P16-PLAN` |
| Phase | P16 — UGC |
| Status | PLAN ACCEPTED; P16-R1–R5 RESOLVED; T001–T005 delivered (R6–R8 UNRESOLVED) |
| Baseline | `4e2098d` (`docs(search): P15 acceptance gate evidence [TC-P15-GATE]` — **TC-P15-GATE** ACCEPTED; P15 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P16 · `docs/architecture/15-future-architecture-transition-map.md` § P · `04-module-boundaries.md` § UGC · `05-dependency-rules.md` Knowledge/UGC · `docs/domain/module-ownership-matrix.md` · `docs/domain/glossary.md` (Review · Travelogue) · P08 Content (UGC ≠ Content) · P06 Media (consumer owns relationship meaning) · P05 SEO (IndexPolicy) · P14 PublicExperience (composition only) · P15 Search (retrieval ≠ UGC SoT) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P16** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P15-GATE` ACCEPT + `TC-NEXT-PHASE-RESOLVE`. Next phase is **explicitly** `P16 — UGC` in `docs/ROADMAP.md` (not guessed). Under PIPELINE continuity, ceremonial confirms and ceremonial Gate waits are **not required**. **No product code in PLAN task.** Open R# must stay OPEN until architect lock.

---

## 0. Next-phase resolve (TC-NEXT-PHASE-RESOLVE)

| Question | Answer from SoT |
|----------|-----------------|
| P15 completion | **COMPLETE / ACCEPTED** — Gate `4e2098d` |
| Authoritative next phase ID | **P16** |
| Title / purpose | **UGC** — Review · Rating · Rating dimensions · Travelogue · User Photo · Comment · Like (if later confirmed) · Report · Moderation · Publication state |
| PLAN already existed? | **NO** — this document is the first P16 PLAN |
| SoT conflict? | **NO** — ROADMAP, module-boundaries, dependency-rules, ownership matrix, transition map, page-archetype registry all name P16 = UGC |
| Missing business fact blocking next phase? | **NO** |
| Invented phase? | **NO** — P16 is already listed after P15 in ROADMAP |

---

## 1. Phase Purpose

P16 باید قابلیت **UGC (محتوای کاربرساخت)** را معرفی کند بدون دزدیدن مالکیت Content، Destination، Place، Tour، Media، SEO، Search، Booking، یا Payment.

هدف (از Roadmap + architecture):

1. **UGC = User-generated content owner** — Review · Rating · RatingDimension · Travelogue · UserPhoto *relationship* · Comment · Moderation · Report/abuse · publication state (`04-module-boundaries.md`).
2. **Target ≠ UGC owner** — Destination/Place/Tour (و Content اگر بعداً تأیید شود) هدف ارجاع‌اند؛ موجودیت هدف مالک Aggregate UGC نیست و نباید navigation EF به `UGC.Review` داشته باشد.
3. **UGC ≠ Content** — Article/Guide/LandingPage editorial باقی می‌ماند در Content (P08). Travelogue UGC با Article editorial یکی نیست.
4. **Media owns technical asset truth** — UserPhoto رابطه/معنا را مالک می‌شود؛ بایت/variant متعلق به Media است (P06).
5. **SEO owns IndexPolicy** — انتشار UGC ≠ ایندکس SEO. آستانهٔ indexability UGC در SEO است نه در UGC.
6. **Search may later retrieve published UGC** — Search مالک Review/Travelogue نیست (P15).
7. **PublicExperience = composition only** — صفحهٔ Place/Destination می‌تواند UGC projection را ترکیب کند؛ مالک lifecycle نیست (P14).

P15 تحویل داد: Search Discovery owner + hybrid read-model + outbox projection + faceting/ranking/AI-readiness contracts + engine-neutral `GET /api/search` stub.

P16 اضافه می‌کند: **UGC module** برای lifecycle کاربرساخت — **بدون** Booking، بدون Payment، بدون Recommendation، بدون تبدیل Review به Catalog SoT.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P15 Gate | `TC-P15-GATE` COMPLETE / ACCEPTED (`4e2098d`) |
| P15 evidence | [`P15-GATE-acceptance-evidence.md`](P15-GATE-acceptance-evidence.md) · [`P15-T009-hardening-and-evidence-pack.md`](P15-T009-hardening-and-evidence-pack.md) |
| P15 Plan | ACCEPTED · R1–R7 RESOLVED · T008 VACANT |
| Baseline HEAD | `4e2098d` |
| P00–P15 | COMPLETE |
| UGC module | **Not implemented** (architecture/docs only) |
| Booking / Payment | Modules do not exist |

---

## 3. Non-goals (explicit)

1. Booking / Payment / Quote / checkout.
2. Turning Review into Tour/Place catalog SoT.
3. Absorbing Article/Guide into UGC or Travelogue into Content.
4. Media storage engine / object-store vendor invent.
5. SEO IndexPolicy / canonical / sitemap ownership.
6. Search ranking/faceting/FTS engine (already P15 contracts; do not re-own).
7. Recommendation / personalization / embeddings / LLM moderation invent.
8. Confirming **Like** (ROADMAP: «در صورت تأیید») — stays OPEN until architect lock.
9. Inventing unlocked R# closures.

---

## 4. Task sequence (proposed)

### TC-P16-PLAN — this document

### TC-P16-T001 — UGC module scaffolding / ownership boundary
- Purpose: Independent UGC module + ownership contracts (**P16-R1 RESOLVED**).
- Delivered: Contracts/Domain/Infrastructure scaffolding; schema `ugc`; `UgcOwnershipBoundary`; opaque `UgcActorReference`; host registration; no peer FKs; no product aggregates.
- Forbidden kept: Review/Rating/Travelogue/Comment/Like/Report tables · target-attachment model · SEO IndexPolicy · Search · Booking · Payment · inventing R2–R8.

### TC-P16-T002 — Review / Rating baseline
- Purpose: Authoritative Review + structured ratings (**P16-R2 RESOLVED**).
- Delivered: `Review` aggregate owns `ReviewId`, opaque actor id, optional title/body, `OverallRating` (1..5), child `ReviewDimensionRating` rows (`DimensionCode` + `Value` 1..5, unique/normalized per Review), audit timestamps. Persistence: `ugc.reviews` + `ugc.review_dimension_ratings`. Rating is **not** an independent aggregate.
- Forbidden kept: independent `Rating` table/aggregate · hardcoded HotelRating/GuideRating/FoodRating/ServiceRating columns · target attachment (P16-R3) · RatingSummary / averages / ranking · Travelogue · UserPhoto · Comment · Like · Report · moderation · SEO/Search ownership · peer-module FK.

### TC-P16-T003 — Target attachment boundary
- Purpose: How UGC attaches to Destination / Place / Tour (and Content only if locked).
- Delivered: Each Review owns exactly one logical `TargetType` + `TargetId` (controlled: TourProduct · Place · Agency). `IReviewTargetValidator` structural port. No peer-schema FK. Target entity is not UGC aggregate owner.
- Forbidden kept: peer FK · cloning Tour/Place/Agency · multiple targets · ReviewForTour duplicates · arbitrary target strings · RatingSummary · Travelogue · inventing R4–R8.

### TC-P16-T004 — Travelogue baseline
- Purpose: Travelogue as UGC narrative, not editorial Article (**P16-R4 RESOLVED**).
- Delivered: Independent `Travelogue` aggregate in schema `ugc` (`TravelogueId`, opaque ActorId, Locale, Title, Body, timestamps). Travelogue != ContentItem. No Content schema change. No publication/moderation (R7 open). No peer FK.
- Forbidden kept: ContentItem subtype · IsUserGenerated on Content · UserPhoto · Comment · Like · Report · inventing R5–R8.

### TC-P16-T005 — UserPhoto relationship baseline
- Purpose: UserPhoto *relationship* over MediaAssetId; Media remains asset SoT (**P16-R5 RESOLVED**).
- Delivered: Independent `UserPhoto` relationship in schema `ugc` (`UserPhotoId`, opaque ActorId, logical MediaAssetId, timestamps). UserPhoto != MediaAsset. No StorageKey/MimeType/dimensions/renditions. Structural `IUserPhotoMediaAssetValidator` only. No peer FK. No Media schema change.
- Forbidden kept: media clone · Comment · Like · Report · inventing R6–R8.

### TC-P16-T006 — Comment + Report/abuse baseline
- Purpose: Comment and Report/abuse without inventing Like unless R6 locks it.

### TC-P16-T007 — Moderation + publication state
- Purpose: Draft/Pending/Published/Rejected/Archived (exact set may be locked by R7).
- Invariant: Published ≠ SEO Indexed.

### TC-P16-T008 — Public composition / read contracts
- Purpose: Replaceable public-read so PE/Place/Destination pages can compose UGC without owning it.
- Vacant allowed if architect finds no independent scope after R7.

### TC-P16-T009 — Hardening + evidence
- Purpose: Harden P16 boundaries and produce gate evidence (**no new capability**).

### TC-P16-GATE — Acceptance Gate
- Evidence only. Ceremonial Gate wait is **not** a pipeline stop.

---

## 5. Open decisions (must not invent)

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P16-R1** | UGC ownership / module / schema | **RESOLVED** | Independent UGC module. Schema `ugc`. Owns user-generated content lifecycle. Does **not** own Identity/Party, Content CMS, MediaAsset technical truth, Tour/Place/Destination facts, SEO IndexPolicy, Search, Booking, or Payment. Actor = opaque logical id only. T001: no Review/Rating/Travelogue/Comment/Like/Report product types, no target-attachment model, no peer FKs. |
| **P16-R2** | Review vs Rating vs RatingDimension | **RESOLVED** | Review is the aggregate. OverallRating is part of Review (range 1..5, domain-validated). Dimension ratings are children (`ReviewDimensionRating`: normalized `DimensionCode` + `Value` 1..5, unique per Review, no independent lifecycle). Do **not** hardcode Hotel/Guide/Food/Service columns. Rating is **not** an independent aggregate. No target attachment in T002 (P16-R3 remains open). UGC owns review/rating facts; not Tour/Search/Agency ranking, SEO, or commercial policy. |
| **P16-R3** | Target attachment | **RESOLVED** | Each Review owns exactly one polymorphic logical target (`TargetType` + `TargetId`). Controlled types: TourProduct · Place · Agency. Logical reference only — no FK to tour/place/agency_marketplace/party. UGC owns the targeting relationship; peer modules own target facts. Structural `IReviewTargetValidator` port; no peer-query orchestration in T003. |
| **P16-R4** | Travelogue vs Content | **RESOLVED** | Travelogue is an independent UGC aggregate (user-authored travel narrative). Article/Guide/LandingPage remain Content CMS. Travelogue != ContentItem. Do not store Travelogue as a ContentItem flag. No peer FK. Publication/moderation remains P16-R7. |
| **P16-R5** | UserPhoto vs Media | **RESOLVED** | UGC owns the UserPhoto relationship (Actor + logical MediaAssetId). Media owns technical MediaAsset truth (bytes/variants/StorageKey/mime/dimensions/renditions/focal). UserPhoto relationship != MediaAsset. No peer FK. No second media store. Publication/moderation remains P16-R7. |
| **P16-R6** | Like / Comment scope | **UNRESOLVED** | ROADMAP: Like only «در صورت تأیید». Comment listed; do not invent social graph. |
| **P16-R7** | Moderation / publication states | **UNRESOLVED** | ROADMAP lists Draft/Pending/Published/Rejected/Archived. Exact workflow, who moderates, and Published ≠ IndexPolicy remain locks. |
| **P16-R8** | Public composition vs SEO/Search | **UNRESOLVED** | PE may compose published UGC. SEO owns indexability. Search may retrieve later. Do not invent UGC SEO factory or ranking-from-reviews. |

---

## 6. Architecture invariants (carry forward)

1. UGC = user-generated owner · Content = editorial owner · Media = asset owner · SEO = IndexPolicy owner · Search = retrieval owner · PublicExperience = presentation/composition only.
2. Target entity (Destination/Place/Tour) is **not** UGC aggregate owner.
3. UGC ≠ Content · Travelogue ≠ Article · UGC != Content · UGC != Media · UGC != target domain owner · UGC != SEO · UGC != Search.
4. Published ≠ SEO Indexed · Published ≠ Bookable.
5. UserPhoto relationship ≠ MediaAsset SoT.
6. Review is not a catalog/pricing/ranking-policy authority.
7. No Booking/Payment modules in P16 unless a later lock says otherwise.
8. Do not invent unlocked R# closures.

---

## 7. Repository safety

- Branch `main` · fast-forward push only · no force · CLEAN working tree before RESULT.
- One docs commit for PLAN (no product code).
- After PLAN ACCEPT, Auto-Execute first locked product task only when architect envelope names it.
