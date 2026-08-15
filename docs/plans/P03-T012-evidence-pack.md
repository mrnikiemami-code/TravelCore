# P03 Evidence Pack — TC-P03-T012

**Task:** TC-P03-T012 — Phase hardening tests & evidence pack  
**Baseline HEAD:** `45aedb2`  
**Date:** 2026-08-16  
**Scope:** Validation / evidence only — no product features; gate not executed.

## 1. Phase map (accepted through T011)

| Task | Commit | Notes |
|------|--------|--------|
| T001 | `afdf73c` | Module scaffolding |
| T002 | `393b7df` | Party domain/persistence |
| T003 | `5730074` | Identity credentials |
| T004 | `91e530a` | Identity↔Party association |
| T005 | `00dd11d` | Access taxonomy |
| T006 | `86f7107` | Deny-by-default evaluator |
| T007 | `089c396` | Subject role assignments |
| T008 | `289180c` (+ `7c22c80`) | HttpOnly cookie auth (R1) |
| T009 | `2843127` | Admin Access-backed authz |
| T010 | `446d557` | Guided Admin Identity↔Party UI |
| T011 | `45aedb2` | Agency presentation Access baseline |

## 2. Architectural invariants verified

- Identity ≠ Party ≠ Access (schemas `identity` / `party` / `access`)
- No cross-schema FK; existence via contracts
- Authentication (cookie) ≠ Authorization (Access evaluator/policies)
- Agency panel commerce flags false; non-ownership documented (`docs/architecture/33-agency-presentation-non-ownership.md`)
- R1 = Secure HttpOnly cookie (`TravelCore.Identity`); Bearer deferred

## 3. Validation battery (this task)

| Suite | Result |
|-------|--------|
| ArchitectureTests | 17 PASS |
| Access.UnitTests | 5 PASS |
| Identity.UnitTests | 6 PASS |
| Party.UnitTests | 6 PASS |
| Host.IntegrationTests | 10 PASS |
| Persistence.IntegrationTests | 13 PASS |
| Frontend `npm run quality` | PASS |
| `git diff --check` | PASS |
| HEAD == origin/main | `45aedb2` (pre-commit) |

## 4. Host authz matrices (representative)

- Admin `/api/admin/access/roles`: 401 → 403 → 200 after assignment
- Agency `/api/agency/panel/capabilities`: 401 → 403 → 200 with Agency party + `agency.panel.open`
- Cookie login/logout/me + auth does not grant Access alone

## 5. Frontend evidence

- Locale Admin workflow: `/[locale]/admin/accounts` + `/onboard`
- Agency stub: `/[locale]/agency` (noindex)
- Client islands allowlisted; no Access engine in React

## 6. Ready for gate?

**YES — evidence pack ready for `TC-P03-GATE`.**  
This task does **not** execute the gate.
