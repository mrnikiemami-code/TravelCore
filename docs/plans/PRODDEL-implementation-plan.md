# Product Delivery — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-PRODDEL-PLAN` |
| Track | Product Delivery Sequence (post-validation evolution) |
| Status | **COMPLETE / ACCEPTED** (`TC-PRODDEL-GATE`) |
| Baseline | `7964056` (`feat(seo): complete SEOVAL T001-GATE and SEO Validation track`) |
| Authoritative sources | `docs/ROADMAP.md` § Product Delivery Sequence · UIVAL + SEOVAL evidence · `docs/ui/*` |
| Frontend root | `src/frontend/web` |

This document is the **authoritative execution plan** for delivering UIVAL/SEOVAL-validated surfaces to production entry points — **not** new backend boundary phases.

> **Envelope note:** Master roadmap (P00–P29 + Post-P29), UI Validation, and SEO Validation are **COMPLETE**. Product Delivery promotes validated patterns to production routes.

---

## 0. Transition resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior track status | **SEO Validation COMPLETE / ACCEPTED** (`TC-SEOVAL-GATE` `7964056`) |
| Authoritative next track | **Product Delivery Sequence** |
| UIVAL dev validation routes | **RETAINED** (noindex) — production routes are separate |
| SEO contracts on public pages | **DELIVERED** (P05 + SEOVAL) — PRODDEL wires production entry |
| New infrastructure phase without ADR? | **FORBIDDEN** |

---

## 1. Track purpose

Promote validated UI/SEO patterns to **production public routes**:

1. Replace P02 foundation smoke home with production Home / Discovery
2. Confirm production archetype routes match UIVAL/SEOVAL contracts
3. Guard transactional surfaces (noindex) and dev-only links off production home
4. Defer full production deployment / E2E farm to operations (T015)

Product Delivery **does not**:

- Introduce new backend bounded contexts
- Replace architect backlog for major new features (hotel catalog detail, real providers, etc.)
- Run live Search Console or production crawl in CI

---

## 2. Validation strategy

| Gate | Tooling |
|------|---------|
| Lint / typecheck / build | `npm run quality` |
| Deterministic checks | `proddel-*-checks.mjs` per task + gate |
| Evidence | `docs/plans/PRODDEL-T00N-*-delivery-evidence.md` |

---

## 3. Ordered task map

| Task | Primary deliverable |
|------|---------------------|
| `TC-PRODDEL-PLAN` | This plan + SoT sync |
| `TC-PRODDEL-T001` | Production home uses `HomeDiscoveryView` |
| `TC-PRODDEL-T002` | Home SEO metadata via compose contract |
| `TC-PRODDEL-T003` | Production home excludes dev-only discovery links |
| `TC-PRODDEL-T004` | Tour listing production route |
| `TC-PRODDEL-T005` | Destination landing production route |
| `TC-PRODDEL-T006` | Tour detail production route |
| `TC-PRODDEL-T007` | Place pages production route |
| `TC-PRODDEL-T008` | Content pages production route |
| `TC-PRODDEL-T009` | Transactional noindex guard |
| `TC-PRODDEL-T010` | Planner / flights / visa entry points |
| `TC-PRODDEL-T011` | Programmatic landing pages production |
| `TC-PRODDEL-T012` | Public shell consistency on production home |
| `TC-PRODDEL-T013` | Locale root constitution |
| `TC-PRODDEL-T014` | UIVAL dev routes retained (noindex) |
| `TC-PRODDEL-T015` | Production deployment / E2E deferral evidence |
| `TC-PRODDEL-GATE` | Track acceptance |

---

## 4. Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial Product Delivery plan after SEOVAL COMPLETE · baseline `7964056` |
