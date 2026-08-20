# TC-P30-T003 Task Envelope (persistent · anti-truncation)

| Field | Value |
|-------|--------|
| Envelope-ID | `TC-P30-T003-ENVELOPE-CREATE` authored this file |
| Executable Task-ID | `TC-P30-T003` |
| Phase | P30 — Product Experience Foundation |
| Title | Design System 2.0 Foundation |
| Purpose of this file | Persist the full authorized execution envelope so ChatGPT UI truncation cannot destroy Pipeline integrity |
| Product code | **NO** — T003 is design-system foundation / documentation only |
| Baseline at envelope authoring | `4d8347b` |
| Prerequisites | `TC-P30-T002` PASS · Constitution + North Star locked |

> **Do not execute `TC-P30-T003` from `TC-P30-T003-ENVELOPE-CREATE`.**  
> Execution of T003 requires a separate authorized cycle that points at this file (or pastes the live block below).

---

## Live execution block (complete)

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P30-T003

Phase:
P30 — Product Experience Foundation

Title:
Design System 2.0 Foundation

Status:
AUTHORIZED

Task-Type:
DESIGN SYSTEM FOUNDATION / DOCUMENTATION LOCK

Baseline:
4d8347b18432eb2c81ec5a5344094ac43e1f06b3

Auto-Execute:
YES (USER PIPELINE + architect authorization after TC-P30-T002 ACCEPT)

Stop-After-Result:
YES


======================================================================
0. PURPOSE
======================================================================

Transform the approved Product Experience Constitution into a concrete
TravelCore Design System 2.0 foundation.

This task creates design-system documentation and foundation artifacts only.

No product pages.
No business features.
No backend / API / database / dependency changes.


======================================================================
1. PIPELINE CONTROLLER CHECK
======================================================================

Before execution read:

docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md
docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md
docs/product-experience/P30-VISUAL-ACCEPTANCE-CHECKLIST.md
docs/product-experience/P30-PUBLIC-EXPERIENCE-SPEC.md
docs/product-experience/P30-ADMIN-EXPERIENCE-SPEC.md
docs/product-experience/P30-AGENCY-EXPERIENCE-SPEC.md
docs/plans/P30-implementation-plan.md
docs/architecture/10-ui-constitution.md
docs/ui/01-design-system-architecture.md
docs/ui/02-responsive-mobile-architecture.md
docs/ui/03-rtl-ltr-bidi.md
docs/ui/05-accessibility-and-interaction.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Confirm:

- Current phase is P30
- TC-P30-T002 completed / accepted (or architect-authorized continuation)
- North Star exists and is PNG
- Constitution exists
- Working tree CLEAN at start
- No conflicting unfinished product work


======================================================================
2. APPROVED NORTH STAR ALIGNMENT
======================================================================

Required asset:

docs/product-experience/assets/travelcore-ui-ux-north-star.png

Rules:

- Do NOT modify the image
- Design System 2.0 must align with North Star / Constitution direction
- Exact final hex values may now be proposed as token candidates
- Tokens remain subject to Visual Checkpoint A before page-first work


======================================================================
3. CREATE / UPDATE ARTIFACTS
======================================================================

Create / ensure under docs/product-experience/ (or nested design-system folder):

docs/product-experience/DESIGN-SYSTEM-2.0.md

Optionally create supporting docs if needed:

docs/product-experience/design-system/tokens.md
docs/product-experience/design-system/typography.md
docs/product-experience/design-system/color-semantics.md
docs/product-experience/design-system/spacing-radius-elevation.md
docs/product-experience/design-system/components-principles.md
docs/product-experience/design-system/responsive-a11y.md

Update:

docs/plans/P30-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md

May update existing design-system architecture docs ONLY as non-conflicting
extensions that preserve Accepted UI Constitution / ADRs.


======================================================================
4. REQUIRED OUTPUT LOCKS
======================================================================

The Design System 2.0 foundation must define:

1. Design token philosophy
2. Typography system
3. Color semantic system
4. Spacing system
5. Radius system
6. Elevation philosophy
7. Component design principles
8. Responsive rules
9. Accessibility baseline

Also lock:

- One Design System / Three Experiences reuse rules
- Public / Admin / Agency shared vs experience-specific components
- State coverage: Loaded / Loading / Empty / Error / Partial Data
- Direction-neutral / RTL-LTR / bidi-safe technical values
- Light / Dark theme token architecture
- Alignment with Discover + Trust + Book
- No Page-First: pages consume this system; pages do not invent tokens


Preserve:

- Server Component First (ADR 0005)
- Direction-neutral UI (ADR 0006)
- Existing P02 primitives — extend, do not fork a second system


======================================================================
5. ALLOWED FILES
======================================================================

docs/product-experience/**
docs/plans/P30-implementation-plan.md
docs/plans/TC-P30-T003-task-envelope.md
docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md

Optional (extend only, no architecture rewrite):

docs/ui/01-design-system-architecture.md
docs/ui/02-responsive-mobile-architecture.md
docs/ui/05-accessibility-and-interaction.md


Frontend design foundation locations:

FORBIDDEN unless a later architect amendment explicitly authorizes a named path.
Default for TC-P30-T003 = documentation only under docs/**.


======================================================================
6. FORBIDDEN
======================================================================

Do NOT:

- execute TC-P30-T004 or later
- build real pages / product screens
- create production UI routes
- modify business features
- change backend / APIs / database / migrations
- install dependencies
- redesign architecture / domain ownership
- execute DEMOFEED
- invent next Task-ID
- modify North Star image
- pixel-clone competitors

Do NOT modify:

src/**
tests/**
package.json / package-lock.json / *.csproj
database / migrations


If unauthorized source files change unintentionally: restore before commit.


======================================================================
7. VALIDATION
======================================================================

Run:

git diff --check
git status --short
git diff --name-only

Require:

- only allowed documentation files changed
- North Star still present and PNG
- Constitution still present
- Design System 2.0 docs present
- no product code changes
- North Star alignment stated in Design System docs

Commit message:

docs(product-experience): add Design System 2.0 foundation

Push origin main.

Verify HEAD == origin/main and Working Tree CLEAN.


======================================================================
8. VISUAL CHECKPOINT NOTE
======================================================================

After TC-P30-T003 PASS and architect ACCEPT:

Visual Checkpoint A is expected (design primitives / representative
component board) before TC-P30-T004 shells.

Do NOT execute Checkpoint implementation unless separately authorized.


======================================================================
9. RESULT FORMAT
======================================================================

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P30-T003
Phase: P30 — Product Experience Foundation
Status: PASS | BLOCKED_REPOSITORY_STATE | BLOCKED_ARCHITECTURE_CONFLICT | BLOCKED_PREREQUISITE_MISSING

Include:

Repository · Branch · Baseline · Implementation-Commit · HEAD · origin/main · Working-Tree
Artifacts
Design System outputs checklist (tokens/typography/color/spacing/radius/elevation/components/responsive/a11y)
North Star alignment YES/NO
Product Code Changed: NO
Dependencies Changed: NO
Architecture Conflict YES/NO
Validation
Cumulative P30 Ledger
Next-State: AWAITING_ARCHITECT_REVIEW

STOP.
Do not execute TC-P30-T004.
Do not implement product pages.
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

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Created by `TC-P30-T003-ENVELOPE-CREATE` · persistent anti-truncation envelope |
