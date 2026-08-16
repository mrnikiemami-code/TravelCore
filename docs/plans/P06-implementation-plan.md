# P06 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P06-PLAN` |
| Phase | P06 — Media |
| Status | AWAITING_ARCHITECT_REVIEW |
| Baseline | `02e06d3` (`docs: mark P05 COMPLETE after TC-P05-GATE accept`; envelope reissued after correct STOP on `37637bf`→`02e06d3` drift) |
| Authoritative sources | `docs/ROADMAP.md` § P06 · `docs/architecture/15-future-architecture-transition-map.md` § F · `04-module-boundaries.md` § Media · `00-constitution.md` · `02-technology-baseline.md` · `05-dependency-rules.md` · `docs/domain/module-ownership-matrix.md` · `docs/data/01-identifiers-and-references.md` · P02 image foundation (`TC-P02-T011`) · P05 SEO locks (R1/R2) · ADR 0001 · ADR 0007–0008 · ADR 0011–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P06** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

---

## 1. Phase Purpose

P06 باید ماژول **Media** را به‌عنوان SoR دارایی‌های باینری و چرخهٔ عمر آن‌ها با مالکیت schema-per-module پیاده‌سازی کند تا:

1. **MediaAsset** (هویت · MIME · اندازه · ابعاد · وضعیت پردازش · کلید/مسیر ذخیره‌سازی) متمرکز شود.
2. بایت‌ها در **S3-compatible object storage** قرار گیرند — نه به‌عنوان payload پیش‌فرض داخل جداول دامنهٔ کسب‌وکار.
3. **Upload · validation · variants · dimensions · focal point · alt/caption translations** طبق مرز مالکیت Media ساخته شوند.
4. قرارداد بهینه‌سازی تصویر برای مصرف‌کننده‌ها تعریف شود؛ **WebP/AVIF pipeline فقط در صورت تأیید معمار** (R1).
5. Frontend P02 (`MediaImage` / `MediaImagePresentation`) با سیاست remote/allowlist تأییدشده گسترش یابد — بدون جایگزینی بی‌مورد.
6. Invariant قفل‌شده حفظ شود: **معنای رابطهٔ رسانه متعلق به ماژول مصرف‌کننده است**؛ Media مالک ترتیب/نقش گالری Tour/Place/Content نیست.

P06 **Place catalog (P07)** · **Content CMS (P08)** · **Tour product (P09+)** · **UGC photo product (P16)** · **Search engine** · **commerce** نیست.

---

## 2. Starting Baseline

Accepted P05 final baseline + post-accept docs sync:

| Item | Value |
|------|--------|
| P05 Gate | `TC-P05-GATE` COMPLETE / ACCEPTED (`7f234e8`) |
| P05 Gate R1 | `TC-P05-GATE-R1` COMPLETE / ACCEPTED (`bde6661`) |
| Docs sync HEAD | `02e06d3` |
| P00–P05 | COMPLETE |
| Backend | Modular Monolith + Identity/Access/Party + ReferenceData + Destination + SEO |
| Frontend | Locale Admin Destination/ReferenceData · Public Destination · P02 `MediaImage` presentation foundation (no Media module) |
| Media module / object-storage abstraction | **Not implemented** (architecture/docs only; P02 presentation-only) |

USER phase token received: `TRAVELCORE_PHASE_CONFIRM: P06`.

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P06 · `15-future-architecture-transition-map.md` § F |
| Media ownership | `04-module-boundaries.md` · module-ownership-matrix · constitution storage rules |
| Technology | `02-technology-baseline.md` (S3-compatible binaries) |
| Identifiers | `docs/data/01-identifiers-and-references.md` (`MediaAssetId`) |
| UI / a11y / LCP | `docs/ui/01` · `10-ui-constitution.md` · P02 IMAGE-FOUNDATION |
| Workflow UX | `docs/ui/06-cross-domain-workflow-and-navigation.md` |
| Localization | ADR 0007–0008 (alt/caption publication/fallback) |
| SEO relationship | P05 R1/R2 · `docs/seo/03` (social preview reuses authoritative media; SEO ≠ Media SoR) |
| Authz | P03 Access + cookie Identity (no Bearer migration) |
| Governance | ADR 0011–0014 · pipeline protocol |

---

## 4. Scope (In)

1. Physical **Media** module scaffolding under `src/backend/Modules/Media/` (Contracts/Domain/Infrastructure) with dedicated DbContext + PostgreSQL schema `media`.
2. **MediaAsset** aggregate/contracts: identity, MIME, byte size, dimensions, storage key/URI, processing status, timestamps.
3. **Object-storage abstraction** (S3-compatible) + development wiring; binaries bound to MediaAsset (not default domain-table blobs).
4. **Upload + validation lifecycle** (Access-backed Admin mutations): content-type/size enforcement, status transitions, ProblemDetails.
5. **Variants** baseline (constitution direction: original/large/medium/thumbnail) + per-variant dimensions.
6. **Focal point** persistence for crop/responsive framing.
7. **Alt/caption translations** (locale rows — forbid `AltFa`/`AltEn` columns); ADR 0008-compatible publication/fallback.
8. **Image optimization contract** for consumers; WebP/AVIF only if architect confirms (else explicit defer with evidence).
9. **Public/read presentation contract** mapping MediaAsset → safe URLs for P02 `MediaImagePresentation` + narrow `remotePatterns` allowlist.
10. **Consumer reference proof** without gallery engines: prove ID-reference pattern (contract/architecture and/or minimal Destination optional `MediaAssetId` smoke — relationship semantics remain Destination-owned).
11. Minimal **Admin Media operational baseline** (job-based; not silo CRUD for every Media table; no raw-ID primary UX).
12. Architecture/integration tests proving Media ↛ business modules; no cross-schema writes; no peer Infrastructure coupling.
13. Phase hardening evidence + `TC-P06-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| Place / Hotel / Restaurant / Attraction catalog + gallery meaning | **P07** |
| Content CMS image/gallery/video blocks | **P08** |
| TourMedia ordering/roles / Tour product media | **P09+** |
| UGC UserPhoto / review media product | **P16** |
| Full CDN product / multi-region CDN ops | later / ops |
| SEO owning image binaries or inventing OG image SoR without Media | **Forbidden** |
| Thin programmatic image SEO / spam | **Forbidden** |
| Video/transcoding product pipeline | Deferred unless architect expands |
| Malware scanning product (if not in-scope for P06) | Record as deferred security requirement |
| Bearer/JWT auth transport change | Forbidden (P03 cookie) |
| Treating Media as owner of Destination/Tour/Place business associations | **Forbidden** |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — schema-per-module; no cross-module DbContext.
2. Media owns asset/binary lifecycle; consumers own relationship semantics (`{EntityId, MediaAssetId, SortOrder, Role}` pattern).
3. Media depends on **no** business module.
4. Bytes in S3-compatible storage; metadata/derivatives separate from business tables.
5. Localization: no `NameFa`/`NameEn`/`NameAr` (and no `AltFa`/`AltEn`) columns.
6. Server Component First for public presentation; Client islands allowlisted.
7. Access is authorization authority for Admin upload/mutate.
8. SEO authority from P05 is not duplicated (canonical/IndexPolicy/hreflang/sitemap remain SEO).
9. Asset exists ≠ asset publicly deliverable ≠ asset attached to public resource ≠ SEO indexable.
10. One Task → One Writer; evidence-based acceptance.
11. Do not continue across NEW baseline drift (STOP `BLOCKED_BASELINE_DRIFT`).

---

## 7. Domain / Ownership Impact

| Module | Owns | Must not own | References |
|--------|------|--------------|------------|
| **Media** | MediaAsset · storage identity · MIME/size/dims · variants · focal point · alt/caption translations · upload/processing status | Tour/Place gallery order/role · Article body · Review logic · Destination content | none (business) |
| **Destination** (optional P06 smoke) | optional hero/reference MediaAssetId meaning | Media processing engine | MediaAssetId |
| **Place / Tour / Content** | future association tables | MediaAsset aggregate | MediaAssetId (later phases) |
| **SEO** | route/index/metadata composition mechanics | Media binaries · inventing image SoR | may compose OG from Media URLs later |
| **Presentation** | MediaImage compose | Persistence SoR | Media read contracts |
| **Access** | `media.*` permissions | Media domain rules | — |
| **Platform** | may host shared storage abstraction if justified | Media business rules | — |

---

## 8. Task Map

### TC-P06-T001 — Media module scaffolding

- **Purpose:** Physical Media module (Contracts/Domain/Infrastructure), DbContext, schema `media`, host registration, ArchitectureTests hooks.
- **Prerequisites:** `TC-P06-PLAN` architect accept.
- **Allowed:** empty/minimal persistence proof · module registration.
- **Forbidden:** upload product · Place/Tour engines · P07+.
- **Validation:** build · ArchitectureTests.
- **Done-when:** Media module exists; Media ↛ business modules; no cross-schema writes.

### TC-P06-T002 — MediaAsset domain + persistence baseline

- **Purpose:** MediaAsset aggregate + create/get/list contracts; strong `MediaAssetId`.
- **Prerequisites:** T001.
- **Allowed:** metadata-only SoR in PostgreSQL.
- **Forbidden:** consumer galleries · default binary blobs in domain tables.
- **Validation:** unit + persistence tests.
- **Done-when:** MediaAsset persistable with metadata fields.

### TC-P06-T003 — Object storage abstraction + wiring

- **Purpose:** S3-compatible put/get/delete (presign if needed); dev provider; config/options; bind keys to MediaAsset.
- **Prerequisites:** T002.
- **Allowed:** Platform and/or Media.Infrastructure ownership as architecture dictates.
- **Forbidden:** hardcoding one cloud vendor into Domain.
- **Validation:** integration smoke against configured backend.
- **Done-when:** binaries round-trip via abstraction.

### TC-P06-T004 — Upload + validation lifecycle

- **Purpose:** Access-backed upload; MIME/size validation; status transitions (Uploading→Ready/Failed).
- **Prerequisites:** T003 · Access patterns from P03.
- **Allowed:** Admin/API upload endpoints · ProblemDetails.
- **Forbidden:** unauthenticated public upload product · UGC free-for-all.
- **Validation:** host tests · authz.
- **Done-when:** authenticated upload yields Ready asset or explicit failure.

### TC-P06-T005 — Variants + dimensions

- **Purpose:** Variant model (original/large/medium/thumbnail direction) + dimensions per variant.
- **Prerequisites:** T004.
- **Allowed:** sync generation baseline; async/job only if justified and narrow.
- **Forbidden:** requiring full CDN product.
- **Validation:** unit/integration.
- **Done-when:** at least one derived size path works end-to-end for a test image.

### TC-P06-T006 — Focal point

- **Purpose:** Persist focal point for crop/responsive framing.
- **Prerequisites:** T002 (may parallel after T005 if independent).
- **Allowed:** domain fields + Admin set/get.
- **Forbidden:** undocumented frontend-only crop as SoR.
- **Validation:** unit + contract tests.
- **Done-when:** focal point stored and returned on reads.

### TC-P06-T007 — Alt/caption translations

- **Purpose:** Localized alt/caption with publication rules; forbid `AltFa`/`AltEn` columns.
- **Prerequisites:** T002.
- **Allowed:** Media-owned translation rows · Admin edit · ADR 0008 fallback.
- **Forbidden:** silent cross-locale publish inventing missing locale public content.
- **Validation:** unit + FA/EN cases.
- **Done-when:** published locale alt available to presentation contracts.

### TC-P06-T008 — Image optimization contract (+ WebP/AVIF R1 gate)

- **Purpose:** Documented/implemented optimization contract; WebP/AVIF **only if R1 confirmed**, else explicit defer with evidence.
- **Prerequisites:** T005 · R1 decision.
- **Allowed:** format selection policy · variant naming · content-type.
- **Forbidden:** shipping unapproved pipeline as done · breaking P02 MediaImage without migration path.
- **Validation:** tests for approved formats; architecture note if deferred.
- **Done-when:** contract accepted; pipeline shipped **or** deferred with architect sign-off.

### TC-P06-T009 — Public presentation URL contract + frontend remote allowlist

- **Purpose:** Resolve MediaAsset → safe URLs for `MediaImagePresentation`; configure narrow `remotePatterns`.
- **Prerequisites:** T003 · T005 · T007.
- **Allowed:** map to P02 MediaImage · smoke render.
- **Forbidden:** wildcard remote hosts · SEO image SoR.
- **Validation:** `npm run quality` · build · alt a11y.
- **Done-when:** approved remote/local presentation path works without CLS regression.

### TC-P06-T010 — Consumer reference proof (no gallery engines)

- **Purpose:** Prove ID-reference pattern without owning gallery semantics.
- **Prerequisites:** T009.
- **Allowed:** ArchitectureTests + contracts **and/or** minimal Destination optional MediaAssetId smoke (Destination keeps relationship meaning).
- **Forbidden:** Place/Tour gallery engines · EF nav Media↔business.
- **Validation:** architecture (+ optional public Destination hero).
- **Done-when:** reference pattern proven; relationship semantics remain with consumer.

### TC-P06-T011 — Admin Media operational baseline

- **Purpose:** Job-based Admin: upload, inspect metadata/variants, edit alt/caption, set focal point.
- **Prerequisites:** T004 · T007 · T009 · Access.
- **Allowed:** guided workflows · Access-backed writes · FA/EN copy.
- **Forbidden:** module-silo CRUD for every Media table · raw-ID primary UX · business rules in UI.
- **Validation:** `npm run quality` · authz.
- **Done-when:** operator can manage assets without Place/Tour CMS.

### TC-P06-T012 — Phase hardening tests & evidence pack

- **Purpose:** Consolidate architecture/integration/frontend evidence for gate readiness.
- **Prerequisites:** T001–T011 accepted (or architect-trimmed subset).
- **Allowed:** tests · docs evidence · tiny unambiguous P06 fixes.
- **Forbidden:** starting P07+ · new modules outside plan.
- **Validation:** full battery green.
- **Done-when:** evidence pack ready for `TC-P06-GATE`.

### TC-P06-GATE — P06 Acceptance Gate

- **Purpose:** Verify P06 exit criteria; mark phase COMPLETE only on PASS.
- **Prerequisites:** T001–T012 accepted (as issued/accepted) · USER `TRAVELCORE_TASK_CONFIRM: TC-P06-GATE`.
- **Allowed:** validation + state hygiene · tiny unambiguous P06 fixes.
- **Forbidden:** starting P07 · Place/Tour/CMS engines · history rewrite.
- **Validation:** checklist §10.
- **Done-when:** architect-accepted COMPLETE or explicit FAIL/BLOCKED.

---

## 9. Dependency Graph

```text
TC-P06-PLAN (architect accept)
 └─> T001 scaffolding
      └─> T002 MediaAsset persistence
           ├─> T003 object storage
           │     └─> T004 upload/validation
           │           └─> T005 variants/dimensions
           ├─> T006 focal point
           └─> T007 alt/caption translations
      T005+T007 + R1 ──> T008 optimization contract
      T003+T005+T007 ──> T009 presentation/remote allowlist
      T009 ──> T010 consumer reference proof
      T004+T007+T009 ──> T011 Admin Media baseline
      T001–T011 ──> T012 evidence ──> TC-P06-GATE
```

---

## 10. Acceptance Strategy (Gate must verify)

1. Media module exists with separate schema; Media ↛ business modules; no cross-schema writes.
2. MediaAsset upload → storage → metadata Ready path works (Access-backed).
3. Variants + dimensions persisted (constitution direction).
4. Focal point persisted and readable.
5. Alt/caption localization without `AltFa`-style columns; ADR 0008-compatible.
6. Bytes in S3-compatible storage; not default domain-table blobs.
7. Optimization contract accepted; WebP/AVIF shipped **or** explicitly deferred with evidence.
8. Frontend presentation extends P02 foundation with approved remote policy.
9. Relationship semantics remain with consumers (no Place/Tour gallery engines).
10. Admin Media baseline is job-based + Access-backed.
11. P07+ engines absent.
12. Evidence pack complete; tests green; clean tree after gate hygiene.

---

## 11. Risks / Open Decisions

| ID | Item | Classification |
|----|------|----------------|
| R1 | Whether WebP/AVIF generation pipeline ships in P06 | **OPEN** — ROADMAP: «در صورت تأیید»; decide by T008 (ship or explicit defer) |
| R2 | Object-storage ownership (Platform abstraction vs Media.Infrastructure-first) | **OPEN** — decide by T003; prefer provider abstraction + env portability |
| R3 | Sync vs async variant generation | **OPEN** — decide by T005; sync baseline acceptable if async not justified |
| R4 | Public URL strategy (direct object URL vs app proxy vs signed URL) | **OPEN** — decide by T009; drives `remotePatterns` |
| R5 | Whether Destination schema gets optional MediaAssetId in P06 or contract-only proof | **OPEN** — decide by T010; Destination relationship meaning stays Destination-owned either way |
| R6 | SVG acceptance policy | **OPEN** — decide by T004 (likely deny/restrict by default unless architect expands) |
| R7 | Malware/AV scanning | **DEFERRED** unless architect expands; record security requirement |
| R8 | Physical delete vs soft-delete + orphan cleanup | **OPEN** — decide by T002/T004; document lifecycle |
| R9 | Context-specific consumer alt override vs Media-owned alt only | **DEFERRED** to consumer phases; P06 Media owns default alt/caption translations |

If an OPEN decision blocks a task, Cursor must **STOP** with `BLOCKED_ARCHITECTURE_CONFLICT` rather than inventing policy.

---

## 12. Phase Exit Criteria (P06 COMPLETE)

1. Media engine baselines (asset/storage/upload/variants/focal/alt/presentation/admin) implemented.
2. Ownership invariant proven: Media owns assets; consumers own relationship meaning.
3. P02 MediaImage path extended safely; no Place/Tour/CMS leakage.
4. SEO authority not duplicated; Publish/Index rules unchanged.
5. Evidence + gate accepted.

---

## 13. Cursor Execution Rules for This Plan

1. Execute **only** the architect-issued Auto-Execute task.
2. When envelopes truncate in ChatGPT DOM, **this plan is source of truth** for scope after acceptance.
3. Do not start `TC-P06-T001` until `TC-P06-PLAN` is architect-accepted and T001 is issued.
4. Do not start P07.
5. Prefer exactly one commit per task unless architect allows evidence follow-up.
6. On baseline mismatch: **STOP** `BLOCKED_BASELINE_DRIFT` (do not self-authorize).
7. PowerShell: use `;` not `&&`.

---

## 14. Plan Delivery Checklist (this PLAN task)

- [x] Plan document created at `docs/plans/P06-implementation-plan.md`
- [x] `docs/PROJECT-STATE.md` updated (P06 IN_PROGRESS; PLAN awaiting review)
- [x] `docs/ROADMAP.md` updated
- [x] No product code / migrations
- [x] Single planning commit · push · RESULT
