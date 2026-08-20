# TC-P30-T005 Task Envelope (persistent · anti-truncation)

| Field | Value |
|-------|--------|
| Envelope-ID | `TC-P30-T005-ENVELOPE-CREATE` authored this file |
| Executable Task-ID | `TC-P30-T005` |
| Phase | P30 — Product Experience Foundation |
| Title | Public Home Experience |
| Purpose of this file | Persist the full authorized execution envelope so ChatGPT UI truncation cannot destroy Pipeline integrity |
| Product code | **YES** — first sellable Public Home experience (not shells-only) |
| Baseline at envelope authoring | `603c942` |
| Prerequisites | `TC-P30-T002` ACCEPTED · `TC-P30-T003` ACCEPTED · `TC-P30-T004` ACCEPTED WITH NOTES · North Star + Design System 2.0 + Public Shell locked |

> **Do not execute `TC-P30-T005` from `TC-P30-T005-ENVELOPE-CREATE`.**  
> Execution of T005 requires a separate authorized cycle that points at this file (or pastes the live block below).

---

## Live execution block (complete)

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P30-T005

Phase:
P30 — Product Experience Foundation

Title:
Public Home Experience

Status:
AUTHORIZED

Task-Type:
PUBLIC HOME / SELLABLE PRODUCT SURFACE

Baseline:
603c942415ff87cb16e87d0f278b1bc2d236dfcf

Auto-Execute:
YES (USER PIPELINE + architect authorization after TC-P30-T004 ACCEPT WITH NOTES)

Stop-After-Result:
YES


======================================================================
0. PURPOSE
======================================================================

Define and implement the first sellable TravelCore public home experience.

This is NOT a technical demo page.
This is NOT an empty wireframe.
This is the first product surface a customer should be able to see and feel.

Goal:

A premium travel commerce homepage aligned with:

- TravelCore Product Experience Constitution
- Design System 2.0
- North Star asset
- Public Shell from TC-P30-T004

Commercial objective:

Create a homepage foundation that can compete visually with:

- lastsecond.ir
- tahagasht.com
- modern travel commerce platforms

WITHOUT pixel-cloning competitors.
WITHOUT inventing fake commerce facts.


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
docs/plans/P30-implementation-plan.md
docs/plans/TC-P30-T005-task-envelope.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Confirm:

- Current phase is P30
- TC-P30-T004 accepted (or architect-authorized continuation)
- North Star PNG exists
- Design System 2.0 exists
- Public Shell components exist
- Working tree CLEAN at start


======================================================================
2. PRODUCT RULE (LOCKED)
======================================================================

Architect lock for T005:

«این اولین صفحه فروش واقعی است، نه نمونه فنی.»

Must feel like:

Premium · Modern · Trustworthy · Travel-first · Visual · Conversion-oriented

Must NOT feel like:

- developer landing page
- framework starter
- generic SaaS template
- empty wireframe board
- backend schema as UI
- placeholder-heavy layout
- generic dashboard


======================================================================
3. REQUIRED HOME STRUCTURE
======================================================================

Implement (or substantially upgrade) the public locale home so it includes
a curated composition — not personalized ML feed:

1. Hero / primary travel intent (visual, strong CTA)
2. Travel search / discovery entry (honest capability only)
3. Featured / popular destinations
4. Tour discovery
5. Hotel discovery
6. Trust signals
7. Content inspiration (stories / travelogues when data exists)
8. Conversion CTA
9. Professional footer (via PublicFooter / PublicShell)

Empty / missing data rules:

- Prefer high-quality empty states over fake rows
- Never invent availability, prices, discounts, ratings, review counts
- Sections may omit when no valid data — remaining layout must still feel premium


======================================================================
4. QUALITY FOCUS
======================================================================

- visual quality vs North Star
- trust building
- travel discovery
- conversion readiness
- mobile-first experience
- SEO readiness (preserve existing locale SEO contracts)
- Core Web Vitals awareness (no heavy unnecessary client bundles)


======================================================================
5. ALLOWED FILES
======================================================================

docs/product-experience/**
docs/plans/P30-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/plans/TC-P30-T005-task-envelope.md

Frontend (home / public experience only):

src/frontend/web/src/app/[locale]/page.tsx
src/frontend/web/src/features/home-discovery/**
src/frontend/web/src/components/shell/** (consume / minor extend only)
src/frontend/web/src/components/ui/** (reuse primitives; no second design system)
src/frontend/web/src/styles/** (token mapping only if required for home polish)

If additional home-adjacent files are strictly required, list + justify in RESULT.
Prefer minimal diff.


======================================================================
6. FORBIDDEN
======================================================================

Do NOT:

- execute TC-P30-T006 or later
- execute DEMOFEED
- redesign backend / domain / APIs / database / migrations
- change ownership boundaries (Search ≠ discovery UI ownership lies)
- install unrelated dependencies by default
- pixel-clone competitors
- invent fake commerce facts
- modify North Star image
- invent next Task-ID

Do NOT modify unless required for compile green:

package.json / package-lock.json (default FORBIDDEN)
*.csproj / database / migrations
admin/** product workflows (out of scope)
agency portal rewrite (out of scope)


======================================================================
7. VISUAL EVIDENCE
======================================================================

Provide:

- desktop screenshot path or board note for public home
- mobile-width evidence (or documented viewport check)
- evidence note under docs/product-experience/evidence/

Architect visual review remains mandatory.
Cursor PASS ≠ Architect ACCEPT.


======================================================================
8. VALIDATION
======================================================================

Run:

git diff --check
git status --short
git diff --name-only
npm run typecheck (in src/frontend/web)

Require:

- allowed paths only (or justified list)
- North Star still PNG
- Constitution + Design System + Public Shell still present
- home feels like a real product surface (not wireframe)
- Working Tree CLEAN after push

Commit message:

feat(ui): add P30 public home experience foundation

Push origin main.

Verify HEAD == origin/main and Working Tree CLEAN.


======================================================================
9. RESULT FORMAT
======================================================================

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P30-T005
Phase: P30 — Product Experience Foundation
Status: PASS | BLOCKED_REPOSITORY_STATE | BLOCKED_ARCHITECTURE_CONFLICT | BLOCKED_PREREQUISITE_MISSING

Include:

Repository · Branch · Baseline · Implementation-Commit · HEAD · origin/main · Working-Tree
Artifacts (home sections / components)
Visual evidence paths
North Star alignment YES/NO
Product Code Changed: YES
Dependencies Changed: YES/NO
Architecture Conflict YES/NO
Validation
Cumulative P30 Ledger
Next-State: AWAITING_ARCHITECT_REVIEW

STOP.
Do not execute TC-P30-T006.
Do not execute DEMOFEED.
Do not infer next task.

END_TRAVELCORE_CURSOR_RESULT_V1

END_TRAVELCORE_CURSOR_TASK_V1
```

---

## Usage rule

1. Architect / USER may authorize execution by referencing this file.
2. Cursor must load **this complete block**, not a truncated chat paste.
3. Preferred execute authorization form:

```text
Execute TC-P30-T005 from docs/plans/TC-P30-T005-task-envelope.md
```

(inside a valid BEGIN/END envelope or equally explicit authorized execute cycle)

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Created by `TC-P30-T005-ENVELOPE-CREATE` · first sellable public home envelope · anti-truncation |
