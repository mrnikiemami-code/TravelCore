# Post-P29 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-Post-P29-PLAN` |
| Phase | Post-P29 — Continuous Evolution |
| Status | **Post-P29 COMPLETE / ACCEPTED** · GATE executed |
| Baseline | `6bc824c` (`docs: sync P29 gate commit hashes in SoT`) |
| Authoritative sources | `docs/ROADMAP.md` § Post-P29 · `docs/PROJECT-STATE.md` · `docs/architecture/15-future-architecture-transition-map.md` · `docs/architecture/04-module-boundaries.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/14-engineering-quality-constitution.md` · P15 Search · P25 Notification · P27 Analytics · P28 Performance · P29 Hardening |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Continuous Evolution phase after production hardening.

> **Envelope note:** `TC-Post-P29-PLAN` ACCEPTED · `TC-Post-P29-T002`–`T009` ACCEPTED · `TC-Post-P29-GATE` COMPLETE · **Post-P29 COMPLETE**.

---

## 0. Next-phase resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior phase status | **P29 COMPLETE / ACCEPTED** (`TC-P29-GATE` `f866cb2`) |
| Authoritative next phase | **Post-P29 — Continuous Evolution** |
| Declared status before this plan | **PLANNED / NOT_STARTED** |
| Dedicated Evolution module in SoT today? | **NO** — evolution themes exist in ROADMAP only |
| Metrics-driven product evolution locked? | **NO** — real production metrics gate not productized yet |
| Microservice extraction pre-committed? | **NO** — explicitly forbidden without evidence · ADR required |
| P00–P29 platform/module boundaries complete? | **YES** — evolution must preserve all accepted boundaries |

---

## 1. Phase purpose

Post-P29 defines **continuous, metrics-driven evolution boundaries** after production readiness — without speculative roadmap delivery, premature microservice extraction, or scattering evolution concerns across Domain modules.

Business purpose (from SoT):

- Continue product improvement using **real production metrics** after launch
- Preserve modular ownership while enabling **evidence-gated** future evolution
- Lock evolution posture for search, providers, personalization, loyalty, pricing, mobile, and module extraction themes before ad-hoc delivery

Architecture objective:

- Introduce **platform-level continuous evolution abstractions** (metrics gate, search/provider/personalization/loyalty/pricing/mobile/module-extraction posture) without breaking module boundaries
- Preserve **Modular Monolith** until measured scale/team/ops evidence justifies extraction · **ADR required** for major transitions
- Preserve **P15 Search != ranking engine** until explicitly locked with evidence
- Preserve **P27 Analytics != Observability** · **P28 Performance deferred scope** · **P29 Hardening deferred scope**
- **No pre-commitment** to microservices · dedicated search cluster · mobile app · loyalty engine without evolution boundary acceptance

---

## 2. Preserved locked architecture

Post-P29 must preserve:

1. Modular Monolith — schema-per-module; no peer-schema FK; no shared DbContext.
2. **Evolution owns evolution posture contracts**; business modules remain SoR for domain facts.
3. **Metrics-driven gate** — no speculative evolution product without measured operational/product need.
4. **Microservice extraction != default next step** — module extraction only with scale/team/ops evidence · ADR required.
5. **Search ranking engine != Search read boundary** — P15 ownership unchanged unless ADR locks otherwise.
6. **Analytics != Observability != Performance != Hardening** — P27/P28/P29 boundaries unchanged.
7. P21–P29 ownership boundaries remain unchanged.
8. Build PASS ≠ Task PASS · evidence-based acceptance (ADR 0011 · ADR 0012).
9. Major architecture transitions require **Accepted ADR** — Cursor cannot self-accept ADRs.

---

## 3. Current SoT baseline snapshot

- ROADMAP Post-P29 lists potential evolutions **without pre-commitment**: dedicated Search engine · more providers · personalization · recommendation · loyalty · promotions · advanced pricing · mobile app · module extraction with evidence.
- Transition map § X Production Hardening complete via P29; no dedicated Continuous Evolution section yet — **ROADMAP Post-P29 is authoritative**.
- P15 Search boundary exists; hybrid read-model posture; ranking engine deferred.
- P27 Analytics downstream boundaries complete; no warehouse/streaming product.
- P28 Performance boundaries complete; distributed complexity deferred.
- P29 Hardening boundaries complete; security/ops vendor products deferred.
- No Evolution module, metrics-evolution gate product, or extraction orchestration in product code today.

---

## 4. Decision inventory for Post-P29 (open for architect locks)

| ID | Topic | Status |
|----|-------|--------|
| `Post-P29-R1` | Metrics-driven evolution gate vs speculative roadmap delivery | **RESOLVED** — real production metrics gate · T003 |
| `Post-P29-R2` | Dedicated search engine evolution boundary vs P15 Search | **RESOLVED** — search evolution theme · P15 preserved · T004 |
| `Post-P29-R3` | Provider expansion boundary | **RESOLVED** — module-owned provider expansion · T005 |
| `Post-P29-R4` | Personalization / recommendation evolution boundary | **RESOLVED** — theme only · ML DEFERRED · T006 |
| `Post-P29-R5` | Loyalty / promotions evolution boundary | **RESOLVED** — theme only · engine DEFERRED · T007 |
| `Post-P29-R6` | Advanced pricing evolution boundary | **RESOLVED** — Pricing SoR preserved · T008 |
| `Post-P29-R7` | Mobile app / client expansion boundary | **RESOLVED** — mobile-first web · native apps DEFERRED · T008 |
| `Post-P29-R8` | Module extraction / microservice evolution boundary + deferred scope | **RESOLVED** — evidence + ADR gate · microservices DEFERRED · T008 |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-Post-P29-PLAN` — Post-P29 architecture implementation plan (**ACCEPTED** · `012c07f`)
2. `TC-Post-P29-T002` — continuous evolution foundation boundary (**ACCEPTED** · `9a89aad`)
3. `TC-Post-P29-T003` — metrics-driven evolution gate boundary (**ACCEPTED** · `10ba6a9` · **Post-P29-R1 RESOLVED**)
4. `TC-Post-P29-T004` — dedicated search engine evolution boundary (**ACCEPTED** · `32a9ac9` · **Post-P29-R2 RESOLVED**)
5. `TC-Post-P29-T005` — provider expansion boundary (**ACCEPTED** · `d17f50e` · **Post-P29-R3 RESOLVED**)
6. `TC-Post-P29-T006` — personalization / recommendation evolution boundary (**ACCEPTED** · `bd468d1` · **Post-P29-R4 RESOLVED**)
7. `TC-Post-P29-T007` — loyalty / promotions evolution boundary (**ACCEPTED** · `17bce33` · **Post-P29-R5 RESOLVED**)
8. `TC-Post-P29-T008` — advanced pricing + mobile + module extraction + deferred scope (**ACCEPTED** · `1281c84` · **Post-P29-R6/R7/R8 RESOLVED**)
9. `TC-Post-P29-T009` — evidence pack (**ACCEPTED** · `9c11aaf` · **READY_FOR_GATE**)
10. `TC-Post-P29-GATE` — acceptance gate (**COMPLETE** · `f0d897b`)

> Note: `TC-Post-P29-T001` is reserved in roadmap numbering for first product task after PLAN acceptance; this plan uses T002+ following established progression where PLAN equals T001 authoring.

### Decision-to-task mapping (proposed progression)

| Decision | Primary task | Notes |
|----------|--------------|-------|
| `Post-P29-R1` | `TC-Post-P29-T003` | Metrics-driven evolution gate; no speculative delivery |
| `Post-P29-R2` | `TC-Post-P29-T004` | Dedicated search engine posture; P15 interaction |
| `Post-P29-R3` | `TC-Post-P29-T005` | Provider expansion posture |
| `Post-P29-R4` | `TC-Post-P29-T006` | Personalization/recommendation posture |
| `Post-P29-R5` | `TC-Post-P29-T007` | Loyalty/promotions posture |
| `Post-P29-R6` | `TC-Post-P29-T008` | Advanced pricing posture |
| `Post-P29-R7` | `TC-Post-P29-T008` | Mobile/client expansion posture |
| `Post-P29-R8` | `TC-Post-P29-T008` | Module extraction/microservice evidence gate + deferred catalog |

### TC-Post-P29-GATE — Acceptance gate

- Purpose: final Post-P29 acceptance evidence only; verify PLAN + T002–T009 accepted and Post-P29-R1–R8 RESOLVED.
- Delivered: `docs/plans/Post-P29-GATE-acceptance-evidence.md` · gate evidence architecture lock test · SoT sync marking **Post-P29 COMPLETE**.
- Forbidden in this task: new evolution product beyond accepted boundaries · unapproved ADR transitions.

### TC-Post-P29-T009 — Evidence pack

- Purpose: adversarial architecture review evidence and gate-readiness documentation without new product capability.
- Delivered: `docs/plans/Post-P29-T009-hardening-and-evidence-pack.md` · evidence-pack architecture lock test · **READY_FOR_GATE**.
- Forbidden in this task: search cluster product · microservice extraction · mobile app product · GATE execution.

### TC-Post-P29-T008 — Advanced pricing, mobile, module extraction, and deferred scope

- Purpose: consolidate advanced pricing, mobile/client expansion, module extraction evidence gate, and deferred evolution catalog; resolve R6/R7/R8.
- Delivered: `EvolutionAdvancedPricingBoundary` · `EvolutionMobileExpansionBoundary` · `EvolutionModuleExtractionBoundary` · `EvolutionOperationalBoundary` · `EvolutionDeferredScopeBoundary` · guardrail tests · **Post-P29-R6/R7/R8 RESOLVED**.
- Forbidden in this task: pricing engine rewrite · native mobile app · service mesh · module split product · API/frontend.

### TC-Post-P29-T007 — Loyalty / promotions evolution boundary

- Purpose: define loyalty and promotions evolution posture without loyalty engine product.
- Delivered: `EvolutionLoyaltyPromotionsBoundary` · guardrail tests · **Post-P29-R5 RESOLVED**.
- Forbidden in this task: loyalty points engine · promotion rules engine product · API/frontend.

### TC-Post-P29-T006 — Personalization / recommendation evolution boundary

- Purpose: define personalization and recommendation evolution posture without ML/recommendation product.
- Delivered: `EvolutionPersonalizationBoundary` · guardrail tests · **Post-P29-R4 RESOLVED**.
- Forbidden in this task: recommendation engine · ML model serving · API/frontend.

### TC-Post-P29-T005 — Provider expansion boundary

- Purpose: define multi-provider expansion posture without provider lock-in product.
- Delivered: `EvolutionProviderExpansionBoundary` · guardrail tests · **Post-P29-R3 RESOLVED**.
- Forbidden in this task: provider registry product · external integration rewrite · API/frontend.

### TC-Post-P29-T004 — Dedicated search engine evolution boundary

- Purpose: define dedicated search engine evolution posture vs P15 Search without OpenSearch/Elasticsearch product.
- Delivered: `EvolutionSearchEngineBoundary` · `EvolutionSearchInteractionBoundary` · guardrail tests · **Post-P29-R2 RESOLVED**.
- Forbidden in this task: search cluster product · ranking engine rewrite · API/frontend.

### TC-Post-P29-T003 — Metrics-driven evolution gate boundary

- Purpose: define production-metrics evolution gate without analytics warehouse or speculative feature delivery.
- Delivered: `EvolutionMetricsGateBoundary` · guardrail tests · **Post-P29-R1 RESOLVED**.
- Forbidden in this task: BI dashboard product · feature-flag vendor lock-in · API/frontend.

### TC-Post-P29-T002 — Continuous evolution foundation boundary

- Purpose: establish Platform-owned continuous evolution foundation markers without evolution product or premature extraction.
- Delivered: `TravelCore.Evolution` · `EvolutionFoundationBoundary` · `EvolutionOwnershipBoundary` · guardrail tests.
- Forbidden in this task: microservice extraction · search cluster · mobile app · loyalty engine · API/frontend · module ownership changes.

---

## 6. Scope (IN)

1. Authoritative Post-P29 plan + SoT alignment (plan-driven tasks only until architect locks R1–R8).
2. Metrics-driven evolution gate posture.
3. Search/provider/personalization/loyalty/pricing/mobile/module-extraction evolution boundaries.
4. ADR-gated major transition posture.
5. Architecture tests proving evolution boundaries do not break module ownership.
6. Evidence pack + GATE.

---

## 7. Out of scope (explicitly NOT in Post-P29 plan-driven early tasks)

- Product code beyond declared boundary scaffolding (until respective task envelopes)
- Microservice extraction or service mesh without ADR + evidence
- Dedicated search engine (Elasticsearch/OpenSearch) product deployment
- ML recommendation / personalization engine product
- Loyalty/promotion rules engine product
- Native mobile application product
- Advanced pricing engine rewrite
- Metrics warehouse / BI dashboard product
- Unapproved ADR architecture transitions

---

## 8. Deferred scope

- Microservice extraction / multi-region active-active
- Dedicated search cluster / ranking ML tuning
- Real-time recommendation streaming
- Loyalty points ledger product
- Promotion engine with stackable rule evaluation
- Native iOS/Android apps
- Advanced dynamic pricing optimization engine
- Module split automation tooling

---

## 9. Blockers / conflicts

| Item | Status |
|------|--------|
| P29 GATE acceptance | **RESOLVED** — `TC-P29-GATE` · `f866cb2` |
| ROADMAP Post-P29 section exists | **RESOLVED** — authoritative next phase |
| Microservices pre-commit forbidden | **LOCKED** — ROADMAP + transition map |
| P15 Search != ranking engine | **LOCKED** — must preserve until ADR |
| P27/P28/P29 boundaries | **LOCKED** — must preserve |
| Major transition ADR requirement | **LOCKED** — engineering quality constitution |

---

## 10. Architecture constraints (locked)

1. **Evolution boundaries live in Platform** or explicit boundary contracts — not scattered speculative flags in Domain modules.
2. **Metrics before major evolution** — no speculative roadmap-as-product.
3. **Module extraction requires evidence + ADR** — not default scalability path.
4. Module schemas remain isolated — no evolution-driven peer-schema FK shortcuts.
5. One task → one writer; evidence-based acceptance; GATE adds no new capability.

---

## 11. Validation strategy (phase-level)

- Plan tasks: `git diff --check` + docs coherence only.
- Product tasks (future): `dotnet build TravelCore.sln` + Evolution/Architecture tests relevant to task scope.
- GATE: full Post-P29 validation battery + clean working tree.

---

## 12. Done-when (plan-driven task TC-Post-P29-PLAN)

- `TC-Post-P29-PLAN` establishes the authoritative Post-P29 execution map with R1–R8 OPEN inventory, decision-to-task mapping, and task briefs through GATE.
- `Post-P29-GATE` closes the phase after R1–R8 are RESOLVED and T002–T009 are accepted.
