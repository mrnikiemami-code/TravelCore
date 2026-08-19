# P29 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P29-PLAN` |
| Phase | P29 — Production Hardening |
| Status | **P29 COMPLETE / ACCEPTED** · GATE executed |
| Baseline | `fef29ab` (`docs: complete P28 acceptance gate`) |
| Authoritative sources | `docs/ROADMAP.md` § P29 · `docs/PROJECT-STATE.md` · `docs/architecture/02-technology-baseline.md` · `docs/architecture/04-module-boundaries.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/07-data-architecture.md` · `docs/architecture/08-persistence-and-migrations.md` · `docs/architecture/10-ui-constitution.md` · `docs/architecture/14-engineering-quality-constitution.md` · `docs/architecture/15-future-architecture-transition-map.md` § X · `docs/architecture/21-health-check-foundation.md` · `docs/architecture/22-observability-logging-and-correlation-foundation.md` · P06 Media · P25 Notification · P26 SEO · P27 Analytics · P28 Performance |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Production Hardening phase.

> **Envelope note:** `TC-P29-PLAN` ACCEPTED · `TC-P29-T002`–`T009` ACCEPTED · `TC-P29-GATE` COMPLETE · **P29 COMPLETE** · **do not start Post-P29 without architect envelope**.

---

## 0. Next-phase resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior phase status | **P28 COMPLETE / ACCEPTED** (`TC-P28-GATE` `fef29ab`) |
| Authoritative next phase | **P29 — Production Hardening** |
| Declared status before this plan | **PLANNED / NOT_STARTED** |
| Dedicated Hardening module in SoT today? | **NO** — security/hardening themes exist in quality constitution and persistence docs only |
| Platform Health exists? | **YES** — `TravelCore.Health` · minimal operational health foundation |
| Platform Observability exists? | **YES** — `TravelCore.Observability` · correlation/metrics posture · **Observability != Product Analytics** |
| Performance boundaries complete? | **YES** — P28 boundaries accepted; hardening must not reopen cache/CDN/scale product |
| Security product implemented? | **NO** — rate limiting · audit-event storage · secret management · backup/DR product deferred |

---

## 1. Phase purpose

P29 introduces **production hardening boundaries** after meaningful product surfaces, platform foundations, analytics, and performance posture exist — without turning Hardening into a business-rule owner or scattering security concerns across Domain modules.

Business purpose (from SoT):

- Establish security, operational resilience, and deployment posture for production readiness
- Preserve modular ownership while enabling future hardening work
- Lock audit, backup/DR, secret, and verification boundaries before Post-P29 evolution

Architecture objective:

- Introduce **platform-level production hardening abstractions** (security review posture, rate limiting, audit, content/file security, backup/DR, health/observability extension, deployment/secrets, production verification) without breaking module boundaries
- Preserve **PostgreSQL as SoR** · **Redis != SoR** · **Health != rich diagnostics API**
- Preserve **Observability != Product Analytics** · **Audit metadata != full audit-event product** until explicitly locked
- Preserve **Media file security != Media delivery ownership** (P06 boundary)
- **Evidence-based hardening** — no vendor lock-in or penetration-test theater without boundary contracts

---

## 2. Preserved locked architecture

P29 must preserve:

1. Modular Monolith — schema-per-module; no peer-schema FK; no shared DbContext.
2. **Domain modules own business authorization facts**; Platform/Hardening owns cross-cutting security posture contracts only.
3. **Secrets never in business tables** — provider credentials · API keys · DB passwords · signing keys → secret/configuration infrastructure.
4. **Health checks remain minimal** — no topology, connection strings, secrets, or exception details in public responses.
5. **Observability != Product Analytics** — P27 boundary unchanged.
6. **Performance boundaries != optimization product** — P28 deferred scope unchanged.
7. **SEO/Content/Destination/Booking/Payment/Media ownership** unchanged.
8. P21–P28 ownership boundaries remain unchanged.
9. Build PASS ≠ Task PASS · evidence-based acceptance (ADR 0011 · ADR 0012).

---

## 3. Current SoT baseline snapshot

- Engineering Quality Constitution §7 locks Security From Day One; exact tooling packages intentionally deferred.
- Health foundation documented; default framework health response only (`TravelCore.Health`).
- Observability foundation documented; correlation headers and middleware exist; no APM vendor product.
- Persistence doc §8 declares audit metadata on rows; high-risk business audit events deferred to Security/Foundation phase — **this is P29 scope**.
- P06 Media delivered upload/validation/delivery; malware/AV scanning **DEFERRED** (P06-R7) — P29 must address file-security boundary without reopening Media delivery ownership.
- P26 SEO graph boundaries complete; production SEO verification is posture-only in P29.
- P28 Performance boundaries complete; load-test harness and CDN product remain DEFERRED.
- No rate limiter, audit-event store, backup automation, secret manager integration, or CI/CD YAML in product code today.

---

## 4. Decision inventory for P29 (open for architect locks)

| ID | Topic | Status |
|----|-------|--------|
| `P29-R1` | Security foundation / authorization review boundary vs domain modules | **RESOLVED** — domain owns authorization facts · Platform cross-cutting posture · T003 boundary only |
| `P29-R2` | Rate limiting / abuse protection boundary | **RESOLVED** — cross-cutting posture · no WAF/rate-limit product · T004 boundary only |
| `P29-R3` | Audit trail / compliance event boundary vs row metadata | **RESOLVED** — row metadata vs audit-event product · no mega-table · T005 boundary only |
| `P29-R4` | Content sanitization / file security boundary vs P06 Media | **RESOLVED** — content/file security posture · P06 delivery preserved · AV DEFERRED · T006 |
| `P29-R5` | Backup/restore / DR / DB recovery boundary | **RESOLVED** — backup/DR/DB recovery posture · cloud backup DEFERRED · T007 |
| `P29-R6` | Health / observability / metrics / tracing / error monitoring extension | **RESOLVED** — Health/Observability extension · APM DEFERRED · T008 |
| `P29-R7` | Deployment / CI/CD / environment config / secret management boundary | **RESOLVED** — deployment/secrets posture · secret manager/CI YAML DEFERRED · T008 |
| `P29-R8` | Production verification (SEO/mobile/a11y) + operational runbooks + deferred scope | **RESOLVED** — verification + runbooks posture · audit products DEFERRED · T008 |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P29-PLAN` — P29 architecture implementation plan (**ACCEPTED** · `6aab050`)
2. `TC-P29-T002` — production hardening foundation boundary (**ACCEPTED** · `8308bb2`)
3. `TC-P29-T003` — security / authorization review boundary (**ACCEPTED** · `ae4ecbf` · **P29-R1 RESOLVED**)
4. `TC-P29-T004` — rate limiting / abuse protection boundary (**ACCEPTED** · `96cd326` · **P29-R2 RESOLVED**)
5. `TC-P29-T005` — audit / compliance event boundary (**ACCEPTED** · `8d52ace` / fix `11051a9` · **P29-R3 RESOLVED**)
6. `TC-P29-T006` — content sanitization / file security boundary (**ACCEPTED** · `79fab46` · **P29-R4 RESOLVED**)
7. `TC-P29-T007` — backup/restore / DR / DB recovery boundary (**ACCEPTED** · `f2d636a` · **P29-R5 RESOLVED**)
8. `TC-P29-T008` — operational platform hardening + production verification + runbooks + deferred scope (**ACCEPTED** · `471a2e7` · **P29-R6/R7/R8 RESOLVED**)
9. `TC-P29-T009` — evidence pack (**ACCEPTED** · `30ec571` · **READY_FOR_GATE**)
10. `TC-P29-GATE` — acceptance gate (**COMPLETE** · `f866cb2`)

> Note: `TC-P29-T001` is reserved in roadmap numbering for first product task after PLAN acceptance; this plan uses T002+ following established P25/P26/P27/P28 progression where PLAN equals T001 authoring.

### Decision-to-task mapping (proposed progression)

| Decision | Primary task | Notes |
|----------|--------------|-------|
| `P29-R1` | `TC-P29-T003` | Security foundation; authorization review posture; domain vs platform split |
| `P29-R2` | `TC-P29-T004` | Rate limiting / abuse protection boundary |
| `P29-R3` | `TC-P29-T005` | Audit trail vs row metadata; high-risk business events |
| `P29-R4` | `TC-P29-T006` | Content sanitization; file security; P06 Media interaction |
| `P29-R5` | `TC-P29-T007` | Backup/restore; DR; DB recovery posture |
| `P29-R6` | `TC-P29-T008` | Extend Health/Observability; error monitoring posture |
| `P29-R7` | `TC-P29-T008` | Deployment/CI/CD/env/secrets boundary (no YAML product in early tasks) |
| `P29-R8` | `TC-P29-T008` | Production SEO/mobile/a11y verification + runbooks + deferred catalog |

### TC-P29-GATE — Acceptance gate

- Purpose: final P29 acceptance evidence only; verify PLAN + T002–T009 accepted and P29-R1–R8 RESOLVED.
- Delivered: `docs/plans/P29-GATE-acceptance-evidence.md` · gate evidence architecture lock test · SoT sync marking **P29 COMPLETE**.
- Forbidden in this task: new hardening product beyond accepted boundaries · Post-P29 evolution · penetration-test vendor lock-in.

### TC-P29-T009 — Evidence pack

- Purpose: adversarial architecture review evidence and gate-readiness documentation without new product capability.
- Delivered: `docs/plans/P29-T009-hardening-and-evidence-pack.md` · evidence-pack architecture lock test · SoT sync · **READY_FOR_GATE**.
- Forbidden in this task: production security vendor lock-in · backup automation product · CI/CD YAML beyond boundary · GATE execution.

### TC-P29-T008 — Operational platform hardening and deferred scope

- Purpose: consolidate Health/Observability extension, deployment/secrets posture, production verification, operational runbooks, and deferred hardening catalog; resolve R6/R7/R8.
- Delivered: `HardeningOperationalBoundary` · `HardeningDeferredScopeBoundary` · `HardeningHealthObservabilityInteractionBoundary` · `HardeningDeploymentSecretsBoundary` · `HardeningProductionVerificationBoundary` · hardening guardrail tests · **P29-R6/R7/R8 RESOLVED**.
- Forbidden in this task: APM vendor product · secret manager integration · CI pipeline YAML · production penetration testing · API/frontend product.

### TC-P29-T007 — Backup/restore / DR / DB recovery boundary

- Purpose: define backup ownership, restore posture, DR principles, and DB recovery boundary without cloud backup product.
- Delivered: `HardeningBackupDrBoundary` · `HardeningDbRecoveryBoundary` · guardrail tests · **P29-R5 RESOLVED**.
- Forbidden in this task: cloud backup vendor · automated restore drills product · multi-region active-active · API/frontend.

### TC-P29-T006 — Content sanitization / file security boundary

- Purpose: define content sanitization and file-security interaction with P06 Media without reopening delivery ownership or implementing AV scanner product.
- Delivered: `HardeningContentSanitizationBoundary` · `HardeningMediaFileSecurityInteractionBoundary` · guardrail tests · **P29-R4 RESOLVED**.
- Forbidden in this task: malware scanner vendor · Media delivery rewrite · upload pipeline product · API/frontend.

### TC-P29-T005 — Audit / compliance event boundary

- Purpose: define audit-event storage posture vs row metadata; high-risk business audit events boundary without audit product.
- Delivered: `HardeningAuditBoundary` · `HardeningRowMetadataInteractionBoundary` · guardrail tests · **P29-R3 RESOLVED**.
- Forbidden in this task: audit log storage product · SIEM integration · cross-module audit mega-table · API/frontend.

### TC-P29-T004 — Rate limiting / abuse protection boundary

- Purpose: define rate limiting and abuse-protection posture without middleware product or vendor lock-in.
- Delivered: `HardeningRateLimitBoundary` · guardrail tests · **P29-R2 RESOLVED**.
- Forbidden in this task: WAF vendor · DDoS product · distributed rate-limit store product · API/frontend.

### TC-P29-T003 — Security / authorization review boundary

- Purpose: define security foundation and authorization review posture; domain vs platform responsibility without auth product rewrite.
- Delivered: `HardeningSecurityBoundary` · `HardeningDomainAuthorizationInteractionBoundary` · guardrail tests · **P29-R1 RESOLVED**.
- Forbidden in this task: identity provider lock-in · OAuth/OIDC product · permission engine rewrite · API/frontend.

### TC-P29-T002 — Production hardening foundation boundary

- Purpose: establish Platform-owned production hardening foundation markers without security product or premature tooling.
- Delivered: `TravelCore.Hardening` · `HardeningFoundationBoundary` · `HardeningOwnershipBoundary` · guardrail tests (**ACCEPTED** · `8308bb2`).
- Forbidden in this task: rate limiter · audit store · secret manager · backup automation · API/frontend · module ownership changes.

---

## 6. Scope (IN)

1. Authoritative P29 plan + SoT alignment (plan-driven tasks only until architect locks R1–R8).
2. Security/authorization review posture.
3. Rate limiting / abuse protection boundaries.
4. Audit trail / compliance event boundaries.
5. Content sanitization / file security boundaries (with P06 interaction).
6. Backup/restore / DR / DB recovery posture.
7. Health / Observability extension / error monitoring posture.
8. Deployment / CI/CD / env config / secret management boundaries.
9. Production SEO/mobile/a11y verification posture + operational runbooks.
10. Architecture tests proving hardening boundaries do not break module ownership.
11. Evidence pack + GATE.

---

## 7. Out of scope (explicitly NOT in P29 plan-driven early tasks)

- Product code beyond declared boundary scaffolding (until respective task envelopes)
- Penetration testing vendor engagement or SAST/DAST product selection
- Identity provider / OAuth / OIDC product implementation
- WAF / CDN security edge product
- Automated backup/restore infrastructure deployment
- Secret manager vendor integration (Vault/AWS Secrets Manager/etc.)
- CI/CD YAML / branch protection / dependency bot product
- Malware/AV scanner product (may reference P06-R7 defer boundary)
- Multi-region DR active-active product
- Post-P29 Continuous Evolution features

---

## 8. Deferred scope

- Penetration testing as recurring vendor engagement
- SAST/DAST / dependency scanning vendor products
- SIEM / centralized log aggregation product
- Hardware security module (HSM) product
- Zero-trust mesh / service mesh security product
- Automated chaos engineering product
- Production load-test infrastructure beyond P28 boundary
- Real-time security incident response automation

---

## 9. Blockers / conflicts

| Item | Status |
|------|--------|
| P28 GATE acceptance | **RESOLVED** — `TC-P28-GATE` · `fef29ab` |
| Health foundation exists | **RESOLVED** — extend, do not replace |
| Observability vs Analytics separation | **LOCKED** — must preserve P27 boundary |
| Media file security vs P06 delivery | **LOCKED** — boundary interaction only; no delivery rewrite |
| P06-R7 malware/AV defer | **LOCKED** — file-security boundary must acknowledge defer |
| Performance deferred scope | **LOCKED** — must not reopen P28 CDN/cache/scale product |
| Secrets in business tables | **LOCKED** — forbidden by persistence architecture |

---

## 10. Architecture constraints (locked)

1. **Security boundaries live in Platform** or explicit boundary contracts — not scattered in Domain modules.
2. **Health != rich diagnostics** · **Audit metadata != audit-event product** until explicitly locked.
3. **Secrets != business data** · never persisted in module schemas.
4. Module schemas remain isolated — no hardening-driven peer-schema FK shortcuts.
5. **Observability != Product Analytics** — P27 ownership preserved.
6. **Media delivery != file security scanner** — P06 ownership preserved.
7. One task → one writer; evidence-based acceptance; GATE adds no new capability.

---

## 11. Validation strategy (phase-level)

- Plan tasks: `git diff --check` + docs coherence only.
- Product tasks (future): `dotnet build TravelCore.sln` + Hardening/Architecture/Integration tests relevant to task scope.
- GATE: full P29 validation battery + clean working tree.

---

## 12. Done-when (plan-driven task TC-P29-PLAN)

- `TC-P29-PLAN` establishes the authoritative P29 execution map with R1–R8 OPEN inventory, decision-to-task mapping, and task briefs through GATE.
- `P29-GATE` closes the phase after R1–R8 are RESOLVED and T002–T009 are accepted.
