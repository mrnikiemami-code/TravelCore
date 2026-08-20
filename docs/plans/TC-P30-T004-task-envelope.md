# TC-P30-T004 Task Envelope (persistent · anti-truncation)

| Field | Value |
|-------|--------|
| Envelope-ID | `TC-P30-T004-ENVELOPE-CREATE` authored this file |
| Executable Task-ID | `TC-P30-T004` |
| Phase | P30 — Product Experience Foundation |
| Title | Application Shells Foundation |
| Purpose of this file | Persist the full authorized execution envelope so ChatGPT UI truncation cannot destroy Pipeline integrity |
| Product code | **YES (shells only)** — Public / Admin / Agency application shells; no full product pages |
| Baseline at envelope authoring | `de27e0f` |
| Prerequisites | `TC-P30-T002` ACCEPTED · `TC-P30-T003` ACCEPTED · North Star + Design System 2.0 locked |

> **Do not execute `TC-P30-T004` from `TC-P30-T004-ENVELOPE-CREATE`.**  
> Execution of T004 requires a separate authorized cycle that points at this file (or pastes the live block below).

---

## Live execution block (complete)

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P30-T004

Phase:
P30 — Product Experience Foundation

Title:
Application Shells Foundation

Status:
AUTHORIZED

Task-Type:
APPLICATION SHELLS / IMPLEMENTATION-READY UI FOUNDATION

Baseline:
de27e0f22f15d5f2196655dc51c48d62c0c5157a

Auto-Execute:
YES (USER PIPELINE + architect authorization after TC-P30-T003 ACCEPT)

Stop-After-Result:
YES


======================================================================
0. PURPOSE
======================================================================

Build TravelCore Application Shells for three experiences:

1. Public Marketplace shell
2. Admin Console shell
3. Agency Portal shell

Shells must be implementation-ready and visually aligned with the
North Star / Design System 2.0 — not abstract-only documentation.

Commercial Demo Requirement:

- Application Shells must be implementation-ready and visually aligned
  with the North Star.
- The goal is to enable rapid creation of sellable public experiences.
- Avoid abstract-only shell definitions.

T004 creates shell chrome / layout foundations.
T004 does NOT build full Home / Hotel / Tour product pages (those are T005+).


======================================================================
1. PIPELINE CONTROLLER CHECK
======================================================================

Before execution read:

docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md
docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md
docs/product-experience/DESIGN-SYSTEM-2.0.md
docs/product-experience/P30-VISUAL-ACCEPTANCE-CHECKLIST.md
docs/product-experience/P30-PUBLIC-EXPERIENCE-SPEC.md
docs/product-experience/P30-ADMIN-EXPERIENCE-SPEC.md
docs/product-experience/P30-AGENCY-EXPERIENCE-SPEC.md
docs/plans/P30-implementation-plan.md
docs/architecture/10-ui-constitution.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Confirm:

- Current phase is P30
- TC-P30-T002 accepted
- TC-P30-T003 accepted
- North Star PNG exists
- Design System 2.0 docs exist
- Working tree CLEAN at start
- No conflicting unfinished product work


======================================================================
2. APPROVED NORTH STAR ALIGNMENT
======================================================================

Required asset:

docs/product-experience/assets/travelcore-ui-ux-north-star.png

Rules:

- Do NOT modify the North Star image
- Shells must align with Deep Ocean + Warm Gold + calm neutrals direction
- Prefer Design System 2.0 token candidates where wiring is practical
- Material visual regression below North Star is forbidden
- Provide screenshot evidence of shells for architect visual review


======================================================================
3. REQUIRED SHELL OUTPUTS
======================================================================

### Public shell

- brand / header
- primary navigation
- travel product navigation entry points
- account / auth entry (honest — no fake features)
- locale-aware structure
- search entry (honest capability only)
- responsive mobile navigation
- professional footer

### Admin shell

- sidebar
- topbar
- breadcrumb region
- command / quick-nav affordance (lean)
- content workspace frame
- responsive behavior for dense ops

### Agency shell

- dashboard shell chrome
- partner / sales-oriented navigation frame
- overview regions (placeholders OK if labeled honestly)
- not a full P24 rewrite

Reuse / extend existing:

src/frontend/web/src/components/shell/**

Do not fork a second shell system.


======================================================================
4. ALLOWED FILES
======================================================================

docs/product-experience/** (shell notes / evidence links only if needed)
docs/plans/P30-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/plans/TC-P30-T004-task-envelope.md

Frontend (shells only):

src/frontend/web/src/components/shell/**
src/frontend/web/src/app/**/layout.tsx
src/frontend/web/src/app/**/loading.tsx (only if required by shell wiring)
src/frontend/web/src/styles/** (token mapping needed for shells only)

If additional shell-adjacent files are strictly required, list them in RESULT
and justify. Prefer minimal diff.


======================================================================
5. FORBIDDEN
======================================================================

Do NOT:

- execute TC-P30-T005 or later
- build full Public Home / Hotel / Tour commerce pages
- execute DEMOFEED
- redesign backend / domain / APIs / database
- install unrelated dependencies
- invent fake commerce data / availability / prices
- pixel-clone competitors
- modify North Star image
- invent next Task-ID

Do NOT modify unless required for shell compile only:

tests/** (except if existing shell tests break and must stay green)
package.json / package-lock.json (default FORBIDDEN; only if architect later amends)
*.csproj / database / migrations


======================================================================
6. VISUAL / QUALITY GATE (in-task)
======================================================================

Because architect authorized T004 after T003 ACCEPT:

- Capture screenshot evidence of Public / Admin / Agency shells
  (desktop + at least one mobile width for Public)
- Attach / store evidence paths under docs/product-experience/ if needed
- Architect visual review remains mandatory (Cursor PASS ≠ Architect ACCEPT)
- Visual Checkpoint A may be satisfied by shell board evidence in this task
  IF shells demonstrate token/component direction; do not invent a separate
  Checkpoint task unless architect issues one


======================================================================
7. VALIDATION
======================================================================

Run:

git diff --check
git status --short
git diff --name-only

Prefer:

- frontend typecheck / lint for touched packages if available and fast
- app still builds / shells render without runtime crash

Require:

- only allowed paths changed (or justified shell-adjacent list in RESULT)
- North Star still present and PNG
- Constitution + Design System 2.0 still present
- shells are real UI (not docs-only placeholders pretending to be done)

Commit message:

feat(ui): add P30 application shells foundation

Push origin main.

Verify HEAD == origin/main and Working Tree CLEAN.


======================================================================
8. RESULT FORMAT
======================================================================

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P30-T004
Phase: P30 — Product Experience Foundation
Status: PASS | BLOCKED_REPOSITORY_STATE | BLOCKED_ARCHITECTURE_CONFLICT | BLOCKED_PREREQUISITE_MISSING

Include:

Repository · Branch · Baseline · Implementation-Commit · HEAD · origin/main · Working-Tree
Artifacts (shell components / layouts)
Screenshot / visual evidence paths
North Star alignment YES/NO
Product Code Changed: YES (shells)
Dependencies Changed: YES/NO
Architecture Conflict YES/NO
Validation
Cumulative P30 Ledger
Next-State: AWAITING_ARCHITECT_REVIEW

STOP.
Do not execute TC-P30-T005.
Do not execute DEMOFEED.
Do not infer next task.

END_TRAVELCORE_CURSOR_RESULT_V1

END_TRAVELCORE_CURSOR_TASK_V1
```

---

## Usage rule

1. Architect / USER may authorize execution by referencing this file.
2. Cursor must load **this complete block**, not a truncated chat paste.
3. Chat truncation of ChatGPT messages must not be treated as a missing Task when this file is present and explicitly authorized.
4. Preferred execute authorization form:

```text
Execute TC-P30-T004 from docs/plans/TC-P30-T004-task-envelope.md
```

(inside a valid BEGIN/END envelope or equally explicit authorized execute cycle)

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Created by `TC-P30-T004-ENVELOPE-CREATE` · persistent anti-truncation envelope · commercial demo shell requirement locked |
