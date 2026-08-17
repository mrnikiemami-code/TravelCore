# P17 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P17-PLAN` |
| Phase | P17 — Visa |
| Status | **PLAN — AWAITING ARCHITECT ACCEPT** · P17-R1–R8 **OPEN** |
| Baseline | `538f3fc` (`docs(ugc): add P16 acceptance gate evidence [TC-P16-GATE]` — **TC-P16-GATE** ACCEPTED; P16 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P17 · `docs/architecture/15-future-architecture-transition-map.md` § Q · `04-module-boundaries.md` § Visa · `05-dependency-rules.md` Commerce/Visa · `docs/architecture/07-data-architecture.md` schema `visa` · `docs/domain/module-ownership-matrix.md` · `docs/pages/07-visa.md` · `docs/pages/00-page-archetype-registry.md` · P04 Destination/ReferenceData · P05 SEO · P06 Media · P08 Content · P12 Pricing · P14 PublicExperience · P15 Search · P16 UGC · P19 Booking · P20 Payment |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P17** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P16-GATE` ACCEPT. Next phase is **explicitly** `P17 — Visa` in `docs/ROADMAP.md` (not guessed). Under PIPELINE continuity, ceremonial confirms and ceremonial Gate waits are **not required**. **No product code in PLAN task.** Open R# must stay OPEN until architect lock. **Do not implement T001 until this PLAN is ACCEPTED and P17-R1 is architecturally locked.**

---

## 0. Next-phase resolve (from SoT; no extra discovery task)

| Question | Answer from SoT |
|----------|-----------------|
| P16 completion | **COMPLETE / ACCEPTED** — Gate `538f3fc` |
| Authoritative next phase ID | **P17** |
| Title / purpose | **Visa** — catalog/offering · Destination/country applicability · VisaType · requirements · documents · processing · commercial reference if needed · public landing composition |
| PLAN already existed? | **NO** — this document is the first P17 PLAN |
| SoT conflict? | **NO** — ROADMAP, module-boundaries, dependency-rules, ownership matrix, data-architecture schema `visa`, transition map § Q, and `VisaDetailPage` all name P17 = Visa |
| ROADMAP wording vs ownership | ROADMAP P17 lists `pricing · content · forms/workflow · SEO landing` as **capability themes**. Those are **not** ownership transfers. Pricing / Content / SEO / Search / Booking remain their own modules. |
| Missing business fact blocking PLAN authorship? | **NO** |
| Invented phase? | **NO** — P17 is already listed after P16 in ROADMAP |

---

## 1. Phase Purpose

P17 باید قابلیت **Visa** را به‌عنوان دامنهٔ مستقل معرفی کند بدون دزدیدن مالکیت Tour، Destination، Content، Booking، Payment، SEO، Search، Pricing، Media، Party، یا UGC.

هدف (از Roadmap + architecture):

1. **Visa = visa-domain owner** — VisaType · visa offering · applicability مقصد/کشور · requirements · documents · processing info · later workflow · commercial service *reference* if needed (`04-module-boundaries.md`).
2. **Tour ≠ Visa product owner** — «این پکیج نیاز به ویزا دارد» مالکیت Visa را منتقل نمی‌کند. وابستگی سخت دوطرفه Tour ↔ Visa ممنوع است. UI عمومی می‌تواند هر دو را ترکیب کند.
3. **Destination / ReferenceData retain geography** — Country/Destination facts remain P04; Visa consumes applicability, does not clone the geo graph.
4. **Content retains editorial guides** — Article/Guide/FAQ blocks remain Content CMS. Visa owns structured requirement facts, not CMS.
5. **Media owns technical asset truth** — Visa may reference MediaAssetId; it does not store bytes/variants.
6. **SEO owns IndexPolicy** — a public Visa page existing ≠ indexed. Canonical / redirects / sitemap remain P05.
7. **Search may later retrieve published Visa** — Search is not Visa SoT (P15).
8. **PublicExperience = composition only** — `VisaDetailPage` is presentation; Visa remains fact owner (P14).
9. **Booking / Payment stay later** — P19 / P20. Public CTA must not imply a transaction that does not exist.
10. **AI-readiness = structured facts** — applicability, applicant category, requirement type, required document, processing duration, validity/stay. No LLM / embeddings / vector / RAG infrastructure.

P16 تحویل داد: UGC lifecycle (Review/Travelogue/UserPhoto/Comment + public composition). Like deferred.

P17 اضافه می‌کند: **Visa module** برای کاتالوگ/الزامات ویزا — **بدون** Booking، بدون Payment، بدون embassy/government integration، بدون OCR، بدون legal-advice engine.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P16 Gate | `TC-P16-GATE` COMPLETE / ACCEPTED (`538f3fc`) |
| P16 evidence | [`P16-GATE-acceptance-evidence.md`](P16-GATE-acceptance-evidence.md) · [`P16-T009-hardening-and-evidence-pack.md`](P16-T009-hardening-and-evidence-pack.md) |
| P16 Plan | ACCEPTED · R1–R8 RESOLVED · T001–T009 ACCEPTED |
| Baseline HEAD | `538f3fc` |
| P00–P16 | COMPLETE |
| Visa module | **Not implemented** (architecture/docs / page-archetype only) |
| Booking / Payment | Modules do not exist (P19 / P20) |

---

## 3. Non-goals (explicit)

1. Booking implementation / reservation / Quote acceptance (P19).
2. Payment implementation (P20).
3. Embassy / government API / external visa-provider integration.
4. OCR / document recognition / applicant CRM.
5. Generic workflow/rules engine / automated legal advice.
6. Search engine / FTS / Elasticsearch implementation (P15 contracts already exist; do not re-own).
7. SEO IndexPolicy / canonical / sitemap ownership transfer.
8. Content CMS ownership of VisaType/requirements.
9. Tour ownership of Visa product; hard bidirectional Tour ↔ Visa.
10. Pricing ownership theft (Visa fee vs Price/Quote is **P17-R6 OPEN**).
11. LLM / embeddings / vector DB / RAG / AI platform.
12. Hardcoding unstable regulatory facts into architecture docs.
13. Inventing unlocked R# closures.
14. Next-phase product (P18 Trip Planner).

---

## 4. Task sequence (proposed)

### TC-P17-PLAN — this document

### TC-P17-T001 — Visa module scaffolding / ownership boundary
- Purpose: Independent Visa module + ownership contracts (**requires P17-R1 lock**).
- Expected lean from SoT (not locked here): Contracts/Domain/Infrastructure scaffolding; schema `visa` (`07-data-architecture.md`); no peer FKs; Destination/ReferenceData/Media by logical id/contracts only (`05-dependency-rules.md`).
- Forbidden until R1 lock + later R#: VisaType/offering/requirement tables beyond scaffolding that R1 allows · Tour bidirectional package · Booking · Payment · SEO IndexPolicy · Search engine · embassy APIs · inventing R2–R8.

### TC-P17-T002 — VisaDefinition vs destination-specific requirement
- Purpose: Separate reusable visa definition from destination/country requirement facts (**P17-R2**).
- Forbidden kept: cloning Destination aggregate · treating ContentItem as VisaType · inventing R3–R8.

### TC-P17-T003 — Applicability model
- Purpose: Who/where a visa fact applies — country/destination/applicant context (**P17-R3**).
- Forbidden kept: peer-schema FK to destination · inventing applicant CRM · inventing R4–R8.

### TC-P17-T004 — Required documents and eligibility facts
- Purpose: Structured eligibility + required-document facts (**P17-R4**). Preserve future room for effective dates / source / last verification **without** a legal rules engine.
- Forbidden kept: inventing competitor checklists as requirements (`docs/pages/07-visa.md`) · OCR · legal-advice engine · inventing R5–R8.

### TC-P17-T005 — Processing time / validity / stay-duration
- Purpose: Structured processing and stay/validity semantics (**P17-R5**).
- Forbidden kept: hardcoded embassy SLAs as architecture truth · inventing R6–R8.

### TC-P17-T006 — Visa fee vs Pricing ownership
- Purpose: Commercial fee boundary (**P17-R6**). If commercialized, Pricing remains Price/Quote owner; Visa must not steal Money/Quote.
- Forbidden kept: second pricing engine · Booking · Payment · inventing R7–R8.

### TC-P17-T007 — Public Visa presentation / Content / SEO boundary
- Purpose: Public `VisaDetailPage` composition without stealing Content or SEO (**P17-R7**). PublicExperience composes; Visa remains fact owner; SEO remains IndexPolicy; Content remains editorial FAQ/guides.
- Public UX posture: mobile-first · locale-aware · RTL/LTR-safe · structured requirements · document checklists · explicit uncertainty/disclaimer · accessible warnings. Published Visa page ≠ SEO Indexed.
- Forbidden kept: Visa-owned IndexPolicy · treating Article as VisaType · Search engine · implying a live application/booking transaction · inventing R8.

### TC-P17-T008 — Application/service vs future Booking
- Purpose: Lock the application/lead/service boundary vs P19 Booking (**P17-R8**). ROADMAP says forms/workflow **if needed**; page archetype CTA is consultation/application **honesty**, and explicitly excludes a full wizard. Do **not** manufacture a workflow engine merely to fill T008. If R8 = DEFER/OUT, T008 may be vacant or docs-only.
- Forbidden kept: Booking module · Payment · embassy integration · applicant CRM.

### TC-P17-T009 — Hardening + evidence
- Purpose: Harden P17 boundaries and produce gate evidence (**no new capability**).
- Forbidden kept: new Visa product · next-phase work.

### TC-P17-GATE — Acceptance Gate
- Evidence only. Ceremonial Gate wait is **not** a pipeline stop.
- No new Visa product capability. Do not start P18 inside GATE.

Do not manufacture empty capabilities merely to fill numbering. T008 is intentionally allowed to become VACANT if R8 defers application/workflow.

---

## 5. Open decisions (must not invent)

| ID | Topic | Status | SoT notes (not a lock) |
|----|-------|--------|------------------------|
| **P17-R1** | Visa module ownership / schema / aggregate boundary | **OPEN** | SoT names an independent Visa module (Commerce). Data architecture lists schema `visa`. Allowed deps: Destination, ReferenceData, Media. Does not own Tour product, Content CMS, SEO IndexPolicy, Search, Booking, Payment. Exact aggregate root(s) are **not** locked. |
| **P17-R2** | VisaDefinition vs Destination-specific VisaRequirement | **OPEN** | Boundaries own both VisaType and requirements. Whether these are one aggregate, two, or definition+applicability children is **not** locked. Destination remains geo SoT. |
| **P17-R3** | Applicability model (country / destination / applicant context) | **OPEN** | Ownership matrix: Visa → Destination = B (applicability). Applicant-category dimensions are useful for AI-readiness but not yet locked. No peer FK. |
| **P17-R4** | Required documents and eligibility facts | **OPEN** | Visa owns requirements/documents. Page archetype lists documents as supporting, not invented checklists. Provenance / effective dates / last verification must remain possible without a legal engine. |
| **P17-R5** | Processing time / validity / stay-duration semantics | **OPEN** | Boundaries own “processing info”. Temporal model exists (NodaTime). Do not hardcode regulatory durations in PLAN. |
| **P17-R6** | Visa fee vs Pricing ownership | **OPEN** | ROADMAP mentions pricing as a P17 theme. Pricing module already owns Price/Quote (P12). Commercial Visa service reference is allowed “if needed”. Do **not** assume Visa stores Money. |
| **P17-R7** | Public Visa presentation / Content / SEO boundary | **OPEN** | `VisaDetailPage` exists as public intent only. SEO = IndexPolicy. Content = editorial FAQ/guides. PublicExperience = composition. Visa = structured fact owner. Published ≠ Indexed. |
| **P17-R8** | Visa application/service vs future Booking/transaction | **OPEN** | ROADMAP: forms/workflow **if needed**. Booking = P19. Payment = P20. Notification example `VisaApplicationUpdated` is **not** a locked event list. Full application wizard is an explicit page non-goal. Likely DEFER — **not resolved here**. |

---

## 6. Architecture invariants (carry forward)

1. Visa != Tour · Visa != Destination · Visa != Content · Visa != Booking · Visa != Payment · Visa != SEO authority · Visa != Search authority.
2. Tour may *reference* visa need; Tour does not own Visa product.
3. Destination/ReferenceData own geography; Visa owns visa meaning applied to that geography.
4. Content owns editorial Article/Guide; Visa owns structured visa facts.
5. Media owns MediaAsset technical truth; Visa may hold logical MediaAssetId only.
6. Pricing owns Price/Quote if a commercial visa fee is later locked; Visa must not steal that ownership.
7. SEO owns IndexPolicy / canonical / redirects / sitemap. Public Visa page ≠ automatically indexed.
8. Search owns retrieval/discovery projection later; Search != Visa SoT.
9. PublicExperience owns presentation/composition only.
10. Published Visa fact ≠ bookable transaction.
11. Time-sensitive visa facts need room for last updated / source / jurisdiction — not a legal rules engine.
12. Structured, attributable, locale-aware facts first. No AI infrastructure in P17.
13. No Booking/Payment modules in P17 unless a later lock says otherwise.
14. Do not invent unlocked R# closures.

---

## 7. Legal / content posture

Visa information is regulatory and time-sensitive (`docs/pages/07-visa.md`).

P17 must **preserve a future ability** to track:

- effective dates
- source / provenance
- last verification
- jurisdiction / context

without inventing a full legal rules engine, embassy connector, or “government truth” store.

Do **not** hardcode unstable regulatory facts into architecture documentation. Public copy must not imply static content is permanently correct. Warnings are assertive; missing pricing is explicit, not `0`.

---

## 8. Public UX posture

Plan for:

- mobile-first
- locale-aware
- RTL/LTR-safe (passport field names, currency, form codes remain LTR-safe)
- structured requirements and document checklists
- clear uncertainty / disclaimer boundaries
- accessible warnings (assertive status, headings, lists)

Primary public intent: understand visa requirements/product and move toward a **honest** next step. Public Visa pages must **not** imply an application/booking transaction exists unless that capability is explicitly introduced later (P17-R8 / P19).

---

## 9. Repository safety

- Branch `main` · fast-forward push only · no force · CLEAN working tree before RESULT.
- One docs commit for PLAN (no product code).
- After PLAN ACCEPT, Auto-Execute first locked product task only when the architect envelope names it **and** P17-R1 is locked.
- Do not start T001 from this PLAN document alone.
