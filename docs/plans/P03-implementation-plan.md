# P03 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P03-PLAN` |
| Phase | P03 — Identity + Access + Party |
| Status | AWAITING_ARCHITECT_REVIEW |
| Baseline | `4eacff5` (`TC-P02-GATE`) |
| Authoritative sources | `docs/ROADMAP.md` · `docs/architecture/03-domain-map.md` · `04-module-boundaries.md` · `05-dependency-rules.md` · `10-ui-constitution.md` · `15-future-architecture-transition-map.md` · `18-backend-physical-structure.md` · `docs/domain/module-ownership-matrix.md` · `docs/ui/06-cross-domain-workflow-and-navigation.md` · ADR 0001–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P03** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

---

## 1. Phase Purpose

P03 باید **هویت احراز هویت (Identity)**، **مجوزدهی (Access)** و **هویت کسب‌وکار (Party)** را به‌صورت ماژول‌های جدا با مالکیت schema-per-module پیاده‌سازی کند تا:

1. Invariant قفل‌شدهٔ **Identity ≠ Party ≠ Access** در runtime و persistence اثبات شود.
2. پایهٔ **authz ادمین** روی ارزیابی Access باشد (نه مخفی‌کردن دکمه در UI).
3. **Agency identity** به‌عنوان مفهوم Party (نه silo احراز هویت جدا) شکل بگیرد.
4. حداقل یک **guided Admin workflow** برای Identity ↔ Party (select/create/link) بدون raw-ID UX و بدون domain-mirroring navigation وجود داشته باشد.
5. دسترسی Presentation آژانس فقط به‌صورت capability-surface مجاز تعریف شود — **بدون کپی منطق Tour/Pricing/Booking**.

P03 **ReferenceData/Destination (P04)**، **SEO engine (P05)**، **Media engine (P06)**، Marketplace آژانس (P13)، یا commerce کامل نیست.

---

## 2. Starting Baseline

Accepted P02 final baseline:

| Item | Value |
|------|--------|
| Commit | `4eacff5` |
| P00 / P01 / P02 | COMPLETE |
| Backend | Modular Monolith host + Platform foundations (UUID v7, NodaTime, Money, PostgreSQL, module DbContext proof, migrations runner, outbox convention, architecture guardrails, validation) |
| Frontend | Locale-prefixed App Router · Server Component First · AdminShell slots (non-domain-mirrored) · cross-domain workflow model (`docs/ui/06-…`) · API/read-model boundary · quality gate `npm run quality` |
| Identity/Access/Party code | **Not implemented** (conceptual ownership only) |

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P03 · `docs/architecture/15-future-architecture-transition-map.md` § C |
| Module ownership | `docs/architecture/03-domain-map.md` · `04-module-boundaries.md` · `05-dependency-rules.md` · `docs/domain/module-ownership-matrix.md` · `docs/domain/glossary.md` |
| Persistence | ADR 0001 · `08-persistence-and-migrations.md` · `18` / `26`–`29` / `32` |
| Identity technical | ADR 0002 · `23-uuid-v7-identity-foundation.md` |
| UI / workflow | ADR 0005–0008 · `10-ui-constitution.md` · `docs/ui/06-cross-domain-workflow-and-navigation.md` |
| Security hygiene | P01 T019 + `docs/quality/06-security-observability-and-performance.md` |
| Governance | ADR 0011–0014 · pipeline protocol |

---

## 4. Scope (In)

1. Physical module scaffolding for **Identity**, **Access**, **Party** under `src/backend/Modules/` with separate DbContexts and PostgreSQL schemas.
2. Party domain: Person · Organization · Agency (business identity/profile) + persistence + owning APIs/contracts.
3. Identity domain: Account / authentication identity / credential storage model + persistence + owning APIs/contracts.
4. Optional Identity↔Party association owned per accepted boundary (Identity may reference `PartyId`; Party remains Party-owned).
5. Access domain: Permission · Role · assignment · authorization evaluation service.
6. Host integration for authentication ticket/session **after** architect-accepted transport choice (see Risks).
7. Admin authz baseline: Access-backed policies/filters; UI hide ≠ security.
8. Guided Admin workflow UI for Identity↔Party relationship actions: select · create · link · replace · unlink · inspect.
9. Agency Presentation access baseline (capability surface only).
10. Architecture/integration tests proving schema isolation and Identity≠Party≠Access.
11. Phase gate evidence.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| ReferenceData catalog + Destination hierarchy | **P04** |
| SEO engine / slug history / sitemap | **P05** |
| Media storage engine beyond existing P02 foundation | **P06+** |
| Full Agency marketplace / seller rules | **P13** |
| Tour / Booking / Payment / Pricing engines | later commerce phases |
| Social login / multi-IdP marketplace | later unless architect decides in-session |
| End-user Public registration UX polish beyond minimal authenticated Admin proof | later UX phases |
| Fine-grained Tour visibility policies inside Tour module | Tour phase |
| Copying domain logic into Admin/Agency panels | **Forbidden permanently** |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — no premature microservices.
2. Schema-per-module (ADR 0001); no cross-schema writes.
3. UUID v7 identities (ADR 0002).
4. **Identity ≠ Party ≠ Access**.
5. Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model.
6. Server-authoritative business rules and authorization.
7. Server Component First (ADR 0005); intentional Client islands only.
8. Direction-neutral / bidi / mobile-first / a11y for any Admin workflow UI (ADR 0006).
9. Locale ≠ Currency ≠ Calendar ≠ Timezone.
10. Domain ≠ navigation ≠ screen ≠ form ≠ workflow (`docs/ui/06`).
11. Raw IDs are not UX.
12. One Task → One Writer; evidence-based acceptance (ADR 0011).
13. No frontend authoritative authz/pricing/booking decisions.

---

## 7. Domain / Ownership Impact

| Module | Owns | Must not own | References |
|--------|------|--------------|------------|
| **Identity** | Account, credentials/authentication identity, security/account state, optional `PartyId` association ref | Person/Agency profile, Role/Permission taxonomy, Tour/Booking/Payment | May hold `PartyId` when linked |
| **Access** | Permission, Role, assignment, authorization evaluation | Credentials, Agency/Person profiles, Tour business policies, UI visibility as SoR | Identity/Party subject IDs (no EF nav into foreign entities) |
| **Party** | Party, Person, Organization, Agency business identity/profile, contact/commercial identifiers | Login credentials, Role/Permission definitions, Tour inventory, Booking, Payment, Destination | ReferenceData later (P04); until then avoid hard coupling |
| **Presentation** (Admin/Agency/Public) | UX composition only | Any combined aggregate or duplicated commerce logic | Explicit application/API contracts |

---

## 8. Cross-Domain Workflow Impact (Mandatory)

### 8.1 Backend ownership stays strict

Identity / Access / Party remain separate modules with separate schemas and command authority.

### 8.2 UI workflow composition (accepted)

Admin guided flow **MAY**:

1. create/authenticate Identity
2. create or select Party (Person/Organization/Agency)
3. link / replace / unlink association
4. assign Access roles/permissions to the correct subject type

in **one user journey** without domain-mirrored menus or raw UUID paste.

### 8.3 Relationship UX patterns required where Admin workflow exists

select · create · link · replace · unlink · inspect — per `docs/ui/06`.

### 8.4 Agency

Agency user authenticates via **Identity**, is authorized via **Access**, and acts on behalf of an **Agency Party**. Agency Panel does not become an Identity silo and does not own Tour/Pricing logic.

### 8.5 Admin navigation

Do **not** freeze final Admin IA as Identity / Access / Party top-level CRUD mirrors. Prefer workflow-oriented entries (e.g. “Accounts & parties”, “Roles”) justified by jobs-to-be-done.

---

## 9. Data / Persistence Impact

| Module | Schema (illustrative) | Introduces |
|--------|----------------------|------------|
| Identity | `identity` | Account/credential tables, optional party association column/table, module outbox if needed |
| Access | `access` | Permission, Role, RolePermission, SubjectRole/assignment tables |
| Party | `party` | Party root + Person/Organization/Agency specialization tables as designed by owning module |

Rules:

- Module-owned DbContext + migrations only.
- No shared “Users” dumping table across modules.
- No EF navigation across module DbContexts.
- Transactions remain module-local; cross-module consistency via accepted contracts/outbox patterns from P01 — not dual-write hacks.

**This planning task creates no schemas/tables.**

---

## 10. API / Contract Impact

| Contract family | Owner | Notes |
|-----------------|-------|-------|
| Identity commands/queries | Identity | create account, credential verify hooks, link/unlink Party ref |
| Party commands/queries | Party | create/update Person/Organization/Agency; search/select read models |
| Access commands/queries | Access | manage taxonomy; assign; evaluate authorization |
| Workflow read models | Presentation composition | compose Identity+Party+Access DTOs without merging ownership |
| Auth ticket issuance | Host + Identity | after transport decision (Risk R1) |

Boundaries:

- API Contract ≠ EF entity.
- Page View Model ≠ Domain aggregate.
- Authorization decisions return from Access evaluation — UI may mirror for UX only.

---

## 11. Frontend Impact

| Area | Plan |
|------|------|
| Routes | Locale-prefixed Admin routes only where needed for P03 proof (FA/EN minimum) |
| Composition | Server Components for pages/workflows; Client islands only for interactive forms/dialogs |
| Workflow | Identity↔Party guided flow using T010 patterns |
| Shell | Reuse AdminShell slots; do not invent domain-mirrored sidebar as architecture |
| States | loading / empty / error / unauthorized |
| Responsive | mobile-first; no desktop-only critical path |
| RTL/LTR/bidi | required for any new Admin UI |
| a11y | landmarks, labels, focus, min touch targets |
| Money/media | not primary in P03; reuse foundations if incidental |
| SEO | Admin routes remain non-indexable / controlled; no P05 SEO engine |

---

## 12. Security / Access Impact

1. Credentials never logged; secrets never committed (P01 hygiene).
2. Password/hashing algorithm choice must follow Platform security baseline — no plaintext.
3. Authorization is Access-owned and server-enforced on every mutating/admin endpoint.
4. UI disable/hide is not security.
5. Subject model must distinguish Identity subject vs Party subject where assignments differ.
6. Tenant/Agency acting-as semantics: Identity authenticates; Access authorizes; Party identifies the business actor — without inventing multi-tenant marketplace rules.

---

## 13. Testing Strategy

| Layer | When | Purpose |
|-------|------|---------|
| Unit | from first domain task | invariants, hashing helpers, evaluation pure logic |
| Architecture tests | as modules appear | project refs, no cross-DbContext leakage, Identity≠Party≠Access |
| Integration (real PostgreSQL) | after each module migration | schema ownership + CRUD smoke |
| API validation | with endpoints | authz allow/deny matrix |
| Frontend quality | with Admin workflow UI | `npm run quality` + FA/EN RTL/LTR spot checks |
| Gate | TC-P03-GATE | end-to-end phase proofs |

Tests are distributed across tasks — not only the gate.

---

## 14. Ordered Task Map

### TC-P03-T001 — Identity/Access/Party module scaffolding

- **Purpose:** Create physical module projects, host registration hooks, schema naming conventions, and empty DbContext shells without business features.
- **Prerequisites:** `TC-P03-PLAN` accepted.
- **Allowed:** `src/backend/Modules/{Identity,Access,Party}/**` scaffolding · solution entries · README/ownership docs pointers · host DI registration stubs.
- **Forbidden:** entities/tables/APIs/UI · ReferenceData · Tour.
- **Validation:** build solution · architecture guardrails for project placement · `git diff --check`.
- **Done-when:** three modules exist as compile-ready shells with clear ownership boundaries.

### TC-P03-T002 — Party domain + persistence baseline

- **Purpose:** Implement Party aggregate concepts (Person/Organization/Agency business identity) with module DbContext + first migration.
- **Prerequisites:** T001.
- **Allowed:** Party domain/persistence/application contracts · Party schema migration · owning Minimal API endpoints for create/get/search stubs.
- **Forbidden:** credentials · roles · Admin UI · Destination/ReferenceData catalogs.
- **Validation:** integration migration proof · unit tests for invariants · architecture tests.
- **Done-when:** Party can be persisted and queried via Party-owned contracts.

### TC-P03-T003 — Identity domain + credential persistence baseline

- **Purpose:** Implement authentication Account/credential model in Identity schema (no public polished signup product yet).
- **Prerequisites:** T001; T002 recommended before association fields are wired.
- **Allowed:** Identity domain/persistence · secure credential hashing · Identity APIs for account create/status · optional nullable Party association field without UX.
- **Forbidden:** Access taxonomy · Admin workflow UI · OAuth marketplace.
- **Validation:** no plaintext secrets · integration tests · security checklist.
- **Done-when:** Identity accounts persist under Identity ownership only.

### TC-P03-T004 — Identity ↔ Party association contracts

- **Purpose:** Backend-authoritative link/unlink/replace association between Account and Party.
- **Prerequisites:** T002 · T003.
- **Allowed:** Identity-owned association commands · Party verification queries via contracts · tests for unlink rules.
- **Forbidden:** raw-ID Admin screens · merging tables across schemas · Access role UI.
- **Validation:** integration tests for link/replace/unlink · architecture boundary tests.
- **Done-when:** association works via APIs without cross-schema writes.

### TC-P03-T005 — Access taxonomy (Permission/Role) + persistence

- **Purpose:** Create Access-owned Permission/Role model and migration.
- **Prerequisites:** T001.
- **Allowed:** Access domain/persistence · seed of minimal Admin permission catalog (explicit list) · CRUD contracts for taxonomy management (admin-only later).
- **Forbidden:** evaluating authz in UI · storing credentials · Party profile fields.
- **Validation:** migration + unit tests · architecture tests.
- **Done-when:** Roles/Permissions persist in `access` schema.

### TC-P03-T006 — Authorization evaluation service

- **Purpose:** Implement Access evaluation (“is subject allowed to perform permission X?”) as server authority.
- **Prerequisites:** T005; subject ID conventions from T003/T002.
- **Allowed:** pure evaluation service · deny-by-default · architecture/unit tests · API probe endpoints for evaluation.
- **Forbidden:** hiding buttons as “security” · frontend policy engines · Tour policy engine.
- **Validation:** allow/deny matrix tests.
- **Done-when:** Access evaluation is the authoritative decision point.

### TC-P03-T007 — Subject role/permission assignment

- **Purpose:** Assign roles/permissions to Identity and/or Party subjects per accepted subject model.
- **Prerequisites:** T005 · T006 · T003 (and T002 if Party subjects are in scope).
- **Allowed:** assignment tables/APIs · validation that subject exists via contracts · tests.
- **Forbidden:** UI-only authorization · cross-module EF includes.
- **Validation:** assignment + evaluation integration tests.
- **Done-when:** assigned subjects evaluate correctly.

### TC-P03-T008 — Host authentication ticket/session integration

- **Purpose:** Wire host authentication so a verified Identity can obtain an authenticated session/ticket for Admin APIs.
- **Prerequisites:** T003 · architect resolution of **Risk R1** (cookie vs bearer vs both).
- **Allowed:** ASP.NET Core auth registration in host · Identity login/logout endpoints · secure cookie/bearer as decided · tests.
- **Forbidden:** inventing IdP marketplace · putting authz rules in JWT claims as SoR replacing Access · Agency commerce.
- **Validation:** authenticated/unauthenticated API probes · security hygiene.
- **Done-when:** Admin API can require authenticated Identity without Access yet attached to every endpoint (Access enforced from T009).

### TC-P03-T009 — Admin authz baseline (Access-backed)

- **Purpose:** Enforce Access evaluation on Admin endpoints/policies; demonstrate hide≠authz.
- **Prerequisites:** T006 · T007 · T008.
- **Allowed:** host filters/policies · endpoint metadata · unauthorized/forbidden results · tests proving UI cannot bypass.
- **Forbidden:** frontend-only gates · duplicating permission strings as business rules outside Access.
- **Validation:** allow/deny integration matrix on sample Admin endpoints.
- **Done-when:** unauthenticated/unauthorized calls fail server-side.

### TC-P03-T010 — Identity ↔ Party guided Admin workflow UI

- **Purpose:** Implement mobile-first Admin workflow for select/create/link/replace/unlink/inspect without raw IDs and without domain-silo navigation freeze.
- **Prerequisites:** T004 · T008 · T009 · existing AdminShell + `docs/ui/06`.
- **Allowed:** locale-prefixed Admin routes · Server Component pages · minimal Client islands for forms · WorkflowViewModels via API boundary · FA/EN RTL/LTR.
- **Forbidden:** mirroring Identity/Access/Party as three mandatory CRUD menus · embedding authz engine in React · P04 Destination UI.
- **Validation:** `npm run quality` · FA/EN probes · a11y checklist · no raw-ID critical path.
- **Done-when:** an admin can complete Identity↔Party linking journey on mobile and desktop.

### TC-P03-T011 — Agency presentation access baseline

- **Purpose:** Prove Agency user path: Identity login + Access permission + acting Party=Agency, with Agency Panel capability surface stub (no commerce logic).
- **Prerequisites:** T002 (Agency party) · T007 · T008 · T009.
- **Allowed:** minimal Agency presentation route/capability gate · contracts only · docs for non-ownership of Tour/Pricing.
- **Forbidden:** Tour/Booking/Payment implementation · marketplace · copying domain services into Agency Panel.
- **Validation:** permission-gated access tests · architecture review checklist.
- **Done-when:** Agency capability surface is Access-gated and commerce-free.

### TC-P03-T012 — Phase hardening tests & evidence pack

- **Purpose:** Consolidate architecture/integration/UI evidence; close residual gaps before gate.
- **Prerequisites:** T001–T011.
- **Allowed:** additional tests · evidence markdown under `docs/plans/` · guardrail tightenings in-scope for P03 boundaries.
- **Forbidden:** new product features · P04 scope.
- **Validation:** full backend test set applicable · frontend quality if UI present · `git diff --check`.
- **Done-when:** evidence pack ready for `TC-P03-GATE`.

### TC-P03-GATE — P03 Acceptance Gate

- **Purpose:** Verify P03 exit criteria end-to-end; mark phase COMPLETE only on PASS.
- **Prerequisites:** T001–T012 accepted.
- **Allowed:** validation + state hygiene · tiny unambiguous P03 regression fixes only.
- **Forbidden:** starting P04 · new modules outside P03 · history rewrite.
- **Validation:** full gate checklist in §16.
- **Done-when:** architect-accepted COMPLETE or explicit FAIL/BLOCKED.

---

## 15. Dependency Graph

```text
TC-P03-PLAN (architect accept)
 └─> T001 scaffolding
      ├─> T002 Party domain/persistence
      ├─> T003 Identity domain/persistence
      │     └─> T004 Identity↔Party association
      │            └─> T010 guided Admin workflow UI
      ├─> T005 Access taxonomy
      │     └─> T006 evaluation
      │            └─> T007 assignments
      │                   └─> T009 Admin authz baseline
      └─> T008 auth ticket/session (after R1)
             ├─> T009
             ├─> T010
             └─> T011 Agency presentation baseline
                    └─> T012 hardening/evidence
                           └─> TC-P03-GATE
```

Parallelism note: after T001, **T002** and **T005** may proceed in parallel; **T003** may proceed in parallel with T005; **T004** waits for T002+T003; **T008** waits for T003 + R1 decision.

---

## 16. Acceptance Strategy (Gate must verify)

1. Three modules exist with separate schemas and no cross-schema writes.
2. Identity ≠ Party ≠ Access proven by architecture tests + code ownership.
3. Party supports Person/Organization/Agency business identity baseline.
4. Identity stores credentials securely; no secret leakage.
5. Access evaluation deny-by-default; assignments work for intended subjects.
6. Authenticated Admin API path exists; unauthorized denied server-side.
7. Guided Identity↔Party Admin workflow works FA/EN, mobile+desktop, no raw-ID UX.
8. Agency presentation baseline is Access-gated and contains no commerce domain logic.
9. Server Component First retained; Client islands contained.
10. Frontend does not become authz authority.
11. P04+ features absent.
12. Evidence pack complete; quality/tests green; working tree clean after gate hygiene.

---

## 17. Risks and Deferred Decisions

| ID | Item | Classification |
|----|------|----------------|
| R1 | Auth ticket transport: cookie session vs bearer JWT vs both for Admin/API | **REQUIRES_ARCHITECT_DECISION_BEFORE_TASK_T008** |
| R2 | Exact password hashing algorithm / KDF parameters | **RESOLVED_BY_EXISTING_ARCHITECTURE** direction (secure one-way hash; no plaintext) — concrete library choice confirmed in T003 against Platform security docs without inventing a custom crypto scheme |
| R3 | Whether Party-subject and Identity-subject both receive Role assignments in v1 | **REQUIRES_ARCHITECT_DECISION_BEFORE_TASK_T007** if matrix docs remain ambiguous; default proposal for review: Identity-centric assignments first, Party-centric acting-as later via Access policies |
| R4 | External login providers (Google/Apple/…) | **DEFERRED_TO_LATER_PHASE** |
| R5 | Full Agency marketplace semantics | **DEFERRED_TO_LATER_PHASE** (P13) |
| R6 | ReferenceData dependency for country/phone catalogs inside Party | **DEFERRED_TO_LATER_PHASE** (P04); Party stores opaque fields/minimal value objects until then |
| R7 | Public website end-user account UX beyond Admin proof | **DEFERRED_TO_LATER_PHASE** |

If R1 is not decided when T008 is issued, Cursor must **STOP** with `BLOCKED_ARCHITECTURE_CONFLICT` rather than inventing a transport.

---

## 18. Phase Exit Criteria (P03 COMPLETE)

P03 may be marked COMPLETE only when all are true:

1. Identity, Access, and Party modules are implemented with separate persistence ownership.
2. Core invariants Identity ≠ Party ≠ Access are test-proven.
3. Admin authz baseline is Access-backed and server-enforced.
4. Identity↔Party guided workflow exists and passes mobile/RTL-LTR/a11y baseline checks.
5. Agency presentation access baseline exists without commerce logic leakage.
6. Auth ticket/session path exists per architect-accepted R1.
7. Architecture/integration/frontend quality evidence accepted.
8. `TC-P03-GATE` PASS and accepted.
9. P04 remains NOT_STARTED until explicitly confirmed.

---

## Plan self-review (PROOF checklist)

| Check | Result |
|-------|--------|
| Every exit criterion owned by ≥1 task | YES (T001–T012 + GATE) |
| Backend ownership explicit | YES §7 |
| Workflow-oriented frontend explicit | YES §8 · T010 |
| Mobile/RTL/LTR/bidi/a11y considered | YES §11 · T010 |
| Server-authoritative decisions | YES §6 · T006 · T009 |
| Tests early enough | YES T002+ onward |
| Later-phase exclusions | YES §5 |
| Architect can issue T001 without redesign | YES — pending plan acceptance |

### Quality failure-mode review

| Mode | Result |
|------|--------|
| A Domain-silo UI | PASS — T010 forbids mechanical CRUD menus |
| B Frontend authority leak | PASS — Access/Identity server-owned |
| C Cross-schema ownership leak | PASS — schema-per-module tasks |
| D Premature generalization | PASS — no IdP marketplace/framework explosion |
| E Phase leakage | PASS — P04+ deferred |
| F Big-bang tasks | PASS — split scaffolding/domain/authz/UI/gate |
| G Missing evidence | PASS — per-task validation + T012 + GATE |
| H Client expansion | PASS — Server Component First |
| I I18n coupling | PASS — locale≠currency/auth |
| J Raw-ID UX | PASS — relationship patterns mandatory |

---

## Implementation note for future Cursor tasks

Do **not** start `TC-P03-T001` until this plan is **Architect Accepted**.  
This document alone does not authorize product code changes beyond planning/state hygiene performed under `TC-P03-PLAN`.
