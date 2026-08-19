# P26 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P26-PLAN` |
| Phase | P26 — Advanced SEO + Content Graph |
| Status | PLAN AUTHORED · **P26 NOT_STARTED** · awaiting architect review |
| Baseline | `ed5c95f` (`docs(p25): add GATE acceptance evidence and close phase`) |
| Authoritative sources | `docs/ROADMAP.md` § P26 · `docs/PROJECT-STATE.md` · `docs/architecture/04-module-boundaries.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/06-cross-module-communication.md` · `docs/architecture/07-data-architecture.md` · `docs/domain/module-ownership-matrix.md` · `docs/architecture/12-seo-constitution.md` · `docs/architecture/15-future-architecture-transition-map.md` § W · `docs/seo/01`–`05` · ADR 0007–0010 · P05 SEO engine · P08 Content · P14 enrichment · P15 Search |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Advanced SEO + Content Graph phase.

> **Envelope note:** `TC-P26-PLAN` authored only · **do not execute `TC-P26-T001` until architect accepts PLAN**.

---

## 0. Next-phase resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior phase status | **P25 COMPLETE / ACCEPTED** (`TC-P25-GATE` `ed5c95f`) |
| Authoritative next phase | **P26 — Advanced SEO + Content Graph** |
| Declared status before this plan | **PLANNED / NOT_STARTED** |
| Dedicated SEO module/schema in SoT today? | **YES** — P05 delivered SEO module + schema `seo` (SeoRoute, IndexPolicy, sitemap/robots frameworks) |
| Content graph implemented? | **NO** — transition map § W targets P26 after meaningful inventory/content |
| Search engine implemented? | **YES (boundary/index posture)** — P15; **Search != SEO Landing** preserved |

---

## 1. Phase purpose

P26 advances discoverability **after** meaningful Destination/Content/Tour/Place inventory exists, without reversing SEO/Content ownership or introducing thin programmatic URL spam.

Business purpose:

- Destination hubs and content clusters that improve internal discovery
- Internal link graph mechanics owned by SEO (not editorial duplication by Content)
- Controlled programmatic landing posture with quality gates
- Route quality, orphan detection, and indexation quality observability
- Sitemap scaling and structured-data completeness on real published surfaces

Architecture objective:

- Extend **SEO** as the platform owner of graph/route/indexation mechanics
- Preserve **Content** as editorial SoR and **Destination** as hierarchy/discovery-node SoR
- Preserve **Search URL != SEO Landing** and **Public != Indexable**
- No new cross-module ownership leaks from HotelBooking/Flight/DynamicPackage/B2B/Notification phases

---

## 2. Preserved locked architecture

P26 must preserve:

1. Modular Monolith — schema-per-module; no peer-schema FK; no shared DbContext.
2. **SEO owns route/graph/indexation mechanics**; business modules own content and commerce facts.
3. **Content owns editorial bodies**; SEO must not become CMS/content SoR.
4. **Destination owns hierarchy/slug hooks**; SEO coordinates publication/route namespace only.
5. **Search is derived**; Search URL != SEO Landing URL.
6. **Public != Indexable**; IndexPolicy remains explicit; no auto-index thin URLs.
7. Programmatic landings require inventory/value/unique purpose/content quality/internal linking/search intent — no bulk thin URL factory.
8. P21 HotelBooking · P22 Flight · P23 DynamicPackage · P24 B2B · P25 Notification ownership boundaries remain unchanged.

---

## 3. Current SoT baseline snapshot

- P05 SEO engine COMPLETE — SeoRoute, redirects, hreflang, IndexPolicy, metadata/structured-data/sitemap frameworks.
- P08 Content CMS baseline exists; P14 public enrichment composition binds Destination-based content without merging SoT.
- P15 Search AI-readiness boundary exists; ranking/index engine deferred.
- P04 Destination public pages and slug hooks exist.
- Transition map § W: Advanced SEO/content graph is **P26**; thin programmatic URL remains forbidden.

---

## 4. Decision inventory for P26 (open for architect locks)

| ID | Topic | Status |
|----|-------|--------|
| `P26-R1` | Content graph ownership / schema posture vs Content/Destination/Search | **OPEN** — SEO owns graph mechanics in schema `seo` (or declared SEO-owned graph tables) · **SEO != Content editorial** · **SEO != Destination hierarchy SoR** · **SEO != Search ranking SoR** · semantic references by ResourceType+ResourceId only · no peer-schema FK |
| `P26-R2` | Hub / cluster taxonomy (Destination hubs · content clusters) | **OPEN** — hub/cluster taxonomy owned by SEO graph semantics · Destination/Content remain fact publishers · no hub content duplication |
| `P26-R3` | Internal link graph boundary | **OPEN** — directed semantic link edges · editorial links remain Content-owned facts · SEO owns graph orchestration/indexation implications only |
| `P26-R4` | Programmatic landing factory posture | **OPEN** — controlled landing generation with quality gates · inventory/value/uniqueness required · **thin URL spam forbidden** · factory automation deferred until explicit lock |
| `P26-R5` | Route quality / orphan detection / indexation quality | **OPEN** — observability and quality markers · orphan/unpublished route detection · no fake index success |
| `P26-R6` | Sitemap scaling + structured-data completeness | **OPEN** — extend existing P05 sitemap/JSON-LD frameworks for graph-aware surfaces · truthful structured data only |
| `P26-R7` | Public/admin operational boundary for graph tooling | **OPEN** — internal read/ops and admin graph posture only until explicit product lock · no public graph mutation API by default |
| `P26-R8` | Deferred/out-of-scope posture (external crawl, AI content gen, full factory automation) | **OPEN** — external link crawling · AI landing copy generation · bulk landing factory · search-ranking manipulation remain DEFERRED unless explicitly locked |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P26-PLAN` — P26 architecture implementation plan (**THIS TASK / AWAITING_ARCHITECT_REVIEW**)
2. `TC-P26-T002` — plan-driven SoT alignment (**NOT EXECUTED**)
3. `TC-P26-T003` — plan decision inventory + execution sequence authoring (**NOT EXECUTED**)
4. `TC-P26-T004` — content graph module/schema foundation (**NOT EXECUTED**)
5. `TC-P26-T005` — hub/cluster boundary (**NOT EXECUTED**)
6. `TC-P26-T006` — internal link graph boundary (**NOT EXECUTED**)
7. `TC-P26-T007` — programmatic landing + route quality boundary (**NOT EXECUTED**)
8. `TC-P26-T008` — hardening and guardrails (**NOT EXECUTED**)
9. `TC-P26-T009` — evidence pack (**NOT EXECUTED**)
10. `TC-P26-GATE` — acceptance gate (**NOT EXECUTED**)

> Note: `TC-P26-T001` is reserved in roadmap numbering for first product task after PLAN acceptance; this plan uses T002+ following established P25 progression where PLAN equals T001 authoring.

---

## 6. Scope (IN)

1. Authoritative P26 plan + SoT alignment (plan-driven tasks only until architect locks R1–R8).
2. SEO-owned content graph scaffolding (contracts/domain/infrastructure extensions within SEO module boundaries).
3. Hub/cluster taxonomy and internal link graph boundaries without Content editorial takeover.
4. Controlled programmatic landing posture (quality gates; no thin URL factory in early tasks).
5. Route quality / orphan / indexation quality markers and guardrails.
6. Graph-aware sitemap/structured-data completeness extensions building on P05 frameworks.
7. Architecture tests proving SEO/Content/Destination/Search separation.
8. Evidence pack + GATE.

---

## 7. Out of scope (explicitly NOT in P26 plan-driven early tasks)

- Product code beyond declared boundary scaffolding (until respective task envelopes)
- Real external web crawling / backlink acquisition
- AI-generated landing copy at scale
- Bulk programmatic URL factory without quality gates
- Search ranking engine / faceted search SoR
- Tour/Hotel/Flight/Booking/Payment workflow changes
- Notification delivery implementation
- Frontend landing factory UI (unless explicitly locked later)
- Next phase P27 Analytics

---

## 8. Deferred scope

- Full automated landing factory operations
- External link graph ingestion
- AI content generation for SEO landings
- Advanced personalization/recommendation SEO surfaces
- Microservice extraction

---

## 9. Blockers / conflicts

| Item | Status |
|------|--------|
| P25 GATE acceptance | **RESOLVED** — `TC-P25-GATE` ACCEPTED · baseline `ed5c95f` |
| SEO module existence | **RESOLVED** — P05 COMPLETE |
| Meaningful Content/Destination inventory | **PARTIAL** — sufficient for boundary phase; full programmatic scale deferred |
| Search vs SEO landing invariant | **LOCKED** — must preserve |
| Thin programmatic URL policy | **LOCKED** — forbidden by ADR 0010 / SEO constitution |

---

## 10. Architecture constraints (locked)

1. Extend existing **SEO** module; do not create parallel URL/content registries in Destination/Content/Tour.
2. Graph edges reference publishable resources by `SeoResourceType + ResourceId` (or equivalent contract), not peer-schema FK.
3. Content editorial text remains in Content schema; SEO stores graph mechanics only.
4. IndexPolicy governs indexability; graph existence != indexability.
5. One task → one writer; evidence-based acceptance; GATE adds no new capability.

---

## 11. Validation strategy (phase-level)

- Plan tasks: `git diff --check` + docs coherence only.
- Product tasks (future): `dotnet build TravelCore.sln` + SEO/Architecture/Integration tests relevant to task scope.
- GATE: full Notification-independent SEO/graph validation battery + clean working tree.

---

## 12. Done-when (PLAN task)

- `docs/plans/P26-implementation-plan.md` exists with sections 0–12, R1–R8 OPEN inventory, execution sequence, IN/OUT/DEFERRED, blockers.
- `docs/plans/P26-PLAN-task-envelope.md` captures architect envelope reference.
- `docs/PROJECT-STATE.md` and `docs/ROADMAP.md` declare P26 PLAN authored / NOT_STARTED product execution.
- No product code, migration, API, frontend, or package dependency changes in PLAN task.
