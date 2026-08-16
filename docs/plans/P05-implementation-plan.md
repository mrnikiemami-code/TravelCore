# P05 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P05-PLAN` |
| Phase | P05 — SEO Engine |
| Status | COMPLETE / ACCEPTED (architect) |
| Baseline | `1d3c224` at plan write (docs hygiene after `TC-P04-GATE` ACCEPTED `f70991f`); governance: original envelope expected `f70991f` — see [`P05-PLAN-R1-baseline-reconciliation.md`](P05-PLAN-R1-baseline-reconciliation.md) |
| Authoritative sources | `docs/ROADMAP.md` § P05 · `docs/architecture/12-seo-constitution.md` · `15-future-architecture-transition-map.md` § E · `docs/seo/01`–`05` · ADR 0007–0010 · ADR 0001 · `03/04/05` domain · `docs/domain/module-ownership-matrix.md` · P04 Destination slug hooks (`TC-P04-T006`) · P04 R3 (`noindex,follow` until SEO IndexPolicy) · ADR 0011–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P05** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

---

## 1. Phase Purpose

P05 باید **موتور مکانیک discoverability عمومی** TravelCore را به‌صورت ماژول **SEO** با مالکیت schema-per-module پیاده‌سازی کند تا:

1. **SeoRoute** · LocalizedSlug/history · **Canonical** · **Redirect** · **hreflang** · **IndexPolicy** · Crawl/robots · metadata overrides (محدود) · breadcrumb/structured-data **framework** · sitemap/robots **framework** · route publication rules متمرکز شوند.
2. Invariant قفل‌شده حفظ شود: **SEO مالک مکانیک مسیر است، نه محتوای کسب‌وکار** (Destination/Tour/Place/Content title/body).
3. مسیر عمومی به هویت معنایی `ResourceType + ResourceId` نگاشت شود — مسیر ≠ هویت کسب‌وکار.
4. **Public ≠ Indexable** · **Search URL ≠ SEO Landing** · controlled indexation (نه ایندکس انبوه thin).
5. اعتبارسنجی روی **صفحات واقعی Destination** موجود از P04 (اولویت یکپارچه‌سازی)؛ بدون کشیدن Place/Tour/Content engines.

P05 **Media (P06)** · **Place catalog (P07)** · **Content CMS (P08)** · **Search engine** · **commerce** نیست.

---

## 2. Starting Baseline

Accepted P04 final baseline (product gate) + docs hygiene:

| Item | Value |
|------|--------|
| Gate commit | `f70991f` (`TC-P04-GATE` ACCEPTED) |
| Docs sync HEAD | `1d3c224` |
| P00–P04 | COMPLETE |
| Backend | Modular Monolith + Identity/Access/Party + ReferenceData + Destination (R1 kinds; slug hooks; Access-backed mutations) |
| Frontend | Locale Admin Destination/ReferenceData workflows · Public Destination `/[locale]/destinations/[slug]` with P04 R3 `noindex,follow` |
| SEO engine code | **Not implemented** (constitution/ADR/docs only) |

USER phase token already received: `TRAVELCORE_PHASE_CONFIRM: P05`.

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P05 · `15-future-architecture-transition-map.md` § E |
| SEO constitution | `12-seo-constitution.md` · `docs/seo/01`–`05` |
| Ownership | ADR 0009 · module-ownership-matrix (SEO Platform) |
| Indexation | ADR 0010 · `seo/02` · `seo/04` |
| Locale routing | ADR 0007–0008 · `11-internationalization-architecture.md` |
| Persistence | ADR 0001 · `07`/`08`/`18` |
| Destination hooks | P04 T006 slug · T009 public page · R3 policy |
| Governance | ADR 0011–0014 · pipeline protocol |

---

## 4. Scope (In)

1. Physical **SEO** module scaffolding under `src/backend/Modules/SEO/` (Contracts/Domain/Infrastructure) with dedicated DbContext + PostgreSQL schema.
2. **SeoRoute** aggregate/contracts mapping `ResourceType + ResourceId` ↔ public locale paths.
3. Localized slug **history** / reservation hooks coordinated with Destination-owned slug fields (Destination remains content/slug SoR for destination names; SEO owns route namespace conflict + publication mechanics).
4. **Canonical** selection rules per locale route.
5. **Redirect** history (at least 301/410 posture as constitution requires) for slug/path changes.
6. **hreflang** / alternate-locale binding only for truly published equivalents (ADR 0008).
7. **IndexPolicy** / crawl posture (default conservative; can lift Destination pages from blanket P04 noindex under explicit policy).
8. Metadata composition framework (title/description overrides only where constitution allows — business content still from Destination).
9. Breadcrumb + JSON-LD **framework** (truthful; Destination path validation first).
10. Sitemap + robots **framework** (generation hooks; not thin URL factory).
11. Route **publication** rules / conflict detection in public namespace.
12. Wire **Destination** public page to SEO contracts (replace hard-coded P04 R3 blanket with IndexPolicy-driven robots).
13. Minimal Admin SEO operational surfaces (job-based; not module-silo CRUD for every table).
14. Architecture/integration tests proving SEO ≠ Destination content ownership; no cross-schema writes.
15. Phase hardening evidence + `TC-P05-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| MediaAsset upload/variants | **P06** |
| Place / Hotel / Restaurant / Attraction catalog SEO binding at scale | **P07** (+ later) |
| Content CMS / Article landings | **P08** |
| Full Search engine / facets SoR | Search phase |
| Advanced content graph / programmatic landing factory | **P26** (foundations may start earlier only if constitution allows; no thin URL spam) |
| Tour/Hotel commerce SEO surfaces | later commerce phases |
| Treating SEO as owner of Destination EnglishName/translations | **Forbidden** |
| Reopening R1 DestinationKind | Forbidden |
| Bearer/JWT auth transport change | Forbidden (P03 R1 cookie) |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — schema-per-module; no cross-module DbContext.
2. SEO owns route mechanics; business modules own content.
3. Public path → SeoRoute → ResourceType+ResourceId → read model.
4. Public ≠ Indexable; IndexPolicy is explicit.
5. Search URL ≠ SEO Landing URL.
6. Locale-prefixed public routes (ADR 0007); no silent cross-locale content (ADR 0008).
7. Destination slug fields remain Destination-owned hooks; SEO must not become Destination content SoR.
8. Server Component First for public pages; metadata from server.
9. No inventing parallel URL registries inside Destination/Tour/Place.
10. One Task → One Writer; evidence-based acceptance.

---

## 7. Domain / Ownership Impact

| Module | Owns | Must not own | References |
|--------|------|--------------|------------|
| **SEO** | SeoRoute · canonical · redirects · hreflang bindings · IndexPolicy · sitemap/robots frameworks · route conflict namespace | Destination/Tour/Place/Content bodies · pricing · booking | Publishable resource IDs/contracts/events |
| **Destination** | Hierarchy · translations · geo · entity slug hooks · destination content | SeoRoute registry · sitemap engine · indexation authority | SEO publication contracts |
| **Presentation** | Compose metadata/robots from SEO + Destination contracts | Invent route SoR | SEO + Destination reads |

---

## 8. Task Breakdown

### TC-P05-T001 — SEO module scaffolding

- **Purpose:** Create SEO module physical structure (Contracts/Domain/Infrastructure), DbContext, schema, host registration.
- **Prerequisites:** `TC-P05-PLAN` accepted.
- **Allowed:** empty/minimal persistence proof · architecture test hooks.
- **Forbidden:** full SeoRoute product behavior · frontend SEO UI · P06 Media.
- **Validation:** build · ArchitectureTests.
- **Done-when:** SEO module exists with separate schema; no cross-schema writes.

### TC-P05-T002 — SeoRoute + localized path binding baseline

- **Purpose:** Domain/persistence for SeoRoute bound to ResourceType+ResourceId and locale path.
- **Prerequisites:** T001.
- **Allowed:** create/get/list-by-resource · conflict detection baseline.
- **Forbidden:** inventing Destination content fields inside SEO.
- **Validation:** unit + persistence.
- **Done-when:** SeoRoute can represent Destination public paths without owning Destination text.

### TC-P05-T003 — Slug history / reservation coordination

- **Purpose:** Route/slug history and reservation mechanics that coordinate with Destination-owned slugs.
- **Prerequisites:** T002 · P04 T006.
- **Allowed:** history records · reservation checks · Destination slug change → redirect candidate hooks.
- **Forbidden:** moving Destination.Translation.Slug ownership into SEO tables as content SoR.
- **Validation:** unit + persistence.
- **Done-when:** slug change can be tracked without duplicate business identity.

### TC-P05-T004 — Canonical + Redirect engine baseline

- **Purpose:** Canonical selection and Redirect history for public routes.
- **Prerequisites:** T002 · T003.
- **Allowed:** 301/410 postures per constitution · host endpoints/contracts as needed.
- **Forbidden:** arbitrary soft redirects that hide missing publication.
- **Validation:** unit + host.
- **Done-when:** canonical/redirect baseline works for Destination route changes.

### TC-P05-T005 — IndexPolicy + robots posture

- **Purpose:** Explicit IndexPolicy/CrawlPolicy replacing P04 blanket Destination noindex when policy allows.
- **Prerequisites:** T002.
- **Allowed:** policy model · default conservative · Destination policy application.
- **Forbidden:** auto-index all URLs · programmatic thin indexation.
- **Validation:** unit + host + frontend metadata integration contract.
- **Done-when:** robots/indexation is policy-driven, not hardcoded forever.

### TC-P05-T006 — hreflang / alternate locale bindings

- **Purpose:** Alternate-locale annotations only for published equivalents.
- **Prerequisites:** T002 · T005 · ADR 0008.
- **Allowed:** bindings API/contracts · metadata consumers.
- **Forbidden:** fabricating unpublished locale alternates.
- **Validation:** unit + frontend metadata checks.
- **Done-when:** Destination FA/EN published pair can emit correct alternates; missing locale omitted.

### TC-P05-T007 — Metadata composition framework

- **Purpose:** Server-side metadata composition from SEO overrides + Destination content contracts.
- **Prerequisites:** T002 · T005.
- **Allowed:** title/description composition rules · noindex/index honor.
- **Forbidden:** SEO copying Destination CMS; Client-side SEO authority.
- **Validation:** frontend quality + Destination page metadata.
- **Done-when:** Destination public page metadata comes through SEO-aware composition.

### TC-P05-T008 — Breadcrumb + structured data framework

- **Purpose:** Truthful breadcrumb/JSON-LD framework validated on Destination path.
- **Prerequisites:** T002 · P04 T005 hierarchy.
- **Allowed:** framework primitives · Destination breadcrumb JSON-LD baseline.
- **Forbidden:** fake ratings/prices · Tour/Hotel schema without ownership.
- **Validation:** unit/frontend assertions · truthful fields only.
- **Done-when:** Destination page can emit non-lying breadcrumb structured data when enabled.

### TC-P05-T009 — Sitemap + robots.txt framework

- **Purpose:** Sitemap/robots generation framework including only policy-approved routes.
- **Prerequisites:** T002 · T005.
- **Allowed:** framework endpoints/jobs hooks · Destination inclusion rules.
- **Forbidden:** dumping all DB rows · thin URL factories.
- **Validation:** host tests · architecture constraints.
- **Done-when:** sitemap/robots framework exists and respects IndexPolicy.

### TC-P05-T010 — Destination public integration + publication rules

- **Purpose:** Wire existing Destination public routes to SEO publication; enforce namespace conflict rules.
- **Prerequisites:** T004 · T005 · T006 · T007 · P04 T009.
- **Allowed:** replace blanket P04 R3 hardcode with IndexPolicy; keep conservative default if unpublished.
- **Forbidden:** Place/Tour SEO binding as primary scope · starting P06.
- **Validation:** FA/EN Destination render · robots policy · quality.
- **Done-when:** Destination public pages are SEO-integrated without content ownership leak.

### TC-P05-T011 — Admin SEO operational baseline

- **Purpose:** Job-based Admin surfaces to inspect/manage route publication/index posture for Destination (minimal).
- **Prerequisites:** T005 · T010 · Access patterns from P03/P04.
- **Allowed:** guided/read+limited mutate workflows · Access-backed writes.
- **Forbidden:** module-silo CRUD for every SEO table · raw-ID primary UX.
- **Validation:** `npm run quality` · authz.
- **Done-when:** Admin can operate Destination SEO posture without silo menus.

### TC-P05-T012 — Phase hardening tests & evidence pack

- **Purpose:** Consolidate architecture/integration/frontend evidence for gate readiness.
- **Prerequisites:** T001–T011 accepted (or architect-trimmed subset).
- **Allowed:** tests · docs evidence · tiny unambiguous P05 fixes.
- **Forbidden:** starting P06 · new modules outside plan.
- **Validation:** full battery green.
- **Done-when:** evidence pack ready for `TC-P05-GATE`.

### TC-P05-GATE — P05 Acceptance Gate

- **Purpose:** Verify P05 exit criteria; mark phase COMPLETE only on PASS.
- **Prerequisites:** T001–T012 accepted (as issued/accepted) · USER `TRAVELCORE_TASK_CONFIRM: TC-P05-GATE`.
- **Allowed:** validation + state hygiene · tiny unambiguous P05 fixes.
- **Forbidden:** starting P06 · Place/Tour engines · history rewrite.
- **Validation:** checklist §16.
- **Done-when:** architect-accepted COMPLETE or explicit FAIL/BLOCKED.

---

## 9. Dependency Graph

```text
TC-P05-PLAN (architect accept)
 └─> T001 scaffolding
      └─> T002 SeoRoute
           ├─> T003 slug history coordination
           │     └─> T004 canonical/redirect
           ├─> T005 IndexPolicy
           │     ├─> T006 hreflang
           │     ├─> T007 metadata composition
           │     └─> T009 sitemap/robots
           └─> T008 breadcrumb/JSON-LD framework
      T004+T005+T006+T007 ──> T010 Destination integration
      T005+T010 ──> T011 Admin SEO baseline
      T001–T011 ──> T012 evidence ──> TC-P05-GATE
```

---

## 10. Acceptance Strategy (Gate must verify)

1. SEO module exists with separate schema; no cross-schema writes.
2. SeoRoute maps ResourceType+ResourceId ↔ locale paths.
3. Canonical + Redirect baseline works.
4. IndexPolicy drives robots/indexation (Destination not forever hardcoded noindex).
5. hreflang only for published equivalents.
6. Metadata/breadcrumb frameworks do not steal Destination content ownership.
7. Sitemap/robots frameworks respect IndexPolicy; no thin URL spam.
8. Destination public pages integrated and validated FA/EN.
9. Admin SEO baseline is job-based + Access-backed.
10. P06+ engines absent.
11. Evidence pack complete; tests green; clean tree after gate hygiene.

---

## 11. Risks / Open Decisions

| ID | Item | Classification |
|----|------|----------------|
| R1 | Exact persistence shape for LocalizedSlug history vs Destination.Translation.Slug | **RESOLVED** (architect at T003): `DestinationTranslation.Slug` = authoritative **current** localized Destination slug (Destination-owned/mutated). SEO does **not** own/write it. SEO owns SeoRoute binding, historical public path records, path reservation, redirect candidate/history mechanics (preferred redirect-chain: A→C, B→C — not A→B→C). Historical path ≠ current Destination slug. |
| R2 | Default IndexPolicy for existing Destination pages after integration | **REQUIRES_ARCHITECT_CONFIRMATION_AT_T005/T010** (proposal: remain noindex until explicit publish) |
| R3 | Whether sitemap generation is on-request endpoint vs background job in P05 | **RESOLVED_BY_PLAN_DEFAULT**: framework first; job runner optional if host already supports |
| R4 | Multi-resource SEO beyond Destination in P05 | **DEFERRED** unless architect expands; Destination-first validation |

If R2 blocks a task, Cursor must **STOP** with `BLOCKED_ARCHITECTURE_CONFLICT` rather than inventing policy. R1 is closed; do not reopen Destination.Translation.Slug ownership.

---

## 12. Phase Exit Criteria (P05 COMPLETE)

1. SEO engine baselines (route/canonical/redirect/index/hreflang/sitemap/robots/metadata frameworks) implemented.
2. Destination public integration accepted under IndexPolicy.
3. Invariants SEO ≠ business content ownership proven.
4. No P06 Media / P07 Place / P08 CMS leakage.
5. Evidence + gate accepted.

---

## 13. Cursor Execution Rules for This Plan

1. Execute **only** the architect-issued Auto-Execute task.
2. When envelopes truncate in ChatGPT DOM, **this plan is source of truth** for scope.
3. Do not start `TC-P05-T001` until `TC-P05-PLAN` is architect-accepted and T001 is issued.
4. Do not start P06.
5. Prefer exactly one commit per task unless architect allows evidence follow-up.
6. PowerShell: use `;` not `&&`.

---

## 14. Plan Delivery Checklist (this PLAN task)

- [x] Plan document created at `docs/plans/P05-implementation-plan.md`
- [x] `docs/PROJECT-STATE.md` updated (P05 IN_PROGRESS; PLAN awaiting review)
- [x] `docs/ROADMAP.md` updated
- [x] No product code / migrations
- [x] Single commit · push · RESULT
