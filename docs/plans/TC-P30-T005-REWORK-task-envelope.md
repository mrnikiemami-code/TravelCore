# TC-P30-T005 REWORK Task Envelope (persistent · anti-truncation)

| Field | Value |
|-------|--------|
| Envelope-ID | `TC-P30-T005-REWORK-ENVELOPE-CREATE` authored this file |
| Executable Task-ID | `TC-P30-T005-REWORK` |
| Phase | P30 — Product Experience Foundation |
| Title | Public Home Visual Rework — Travel Commerce Homepage |
| Purpose of this file | Persist the full authorized rework envelope so ChatGPT UI truncation cannot destroy Pipeline integrity |
| Product code | **YES** — visual/product-experience upgrade of existing Public Home (not a greenfield rewrite) |
| Baseline at envelope authoring | `183b8c77b76bfc91e8d3f8f7e2901b6a1f5027ed` |
| Prerequisites | `TC-P30-T005` Technical PASS (`d176045`) · Visual Checkpoint C **REWORK_REQUIRED** · T006 **BLOCKED** until Homepage visual quality accepted |

> **Do not execute `TC-P30-T005-REWORK` from `TC-P30-T005-REWORK-ENVELOPE-CREATE`.**  
> Execution requires a separate authorized cycle that points at this file (or pastes the live block below).

---

## Architect decision (locked)

```text
TC-P30-T005
Technical: PASS
Visual: REWORK_REQUIRED
Architect Decision: REWORK_REQUIRED
Reason: Homepage foundation exists, but commercial visual quality is below TravelCore target.
TC-P30-T006 = BLOCKED until Homepage is fixed.
```

Evidence SoT:

- `docs/product-experience/evidence/P30-T005/VISUAL-CHECKPOINT-C.md`
- Screenshots under `docs/product-experience/evidence/P30-T005/`

Do **not** throw away the current skeleton. Convert:

`Landing Page / foundation` → `Travel Commerce Homepage`

---

## Live execution block (complete)

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P30-T005-REWORK

Phase:
P30 — Product Experience Foundation

Title:
Public Home Visual Rework — Travel Commerce Homepage

Status:
AUTHORIZED

Task-Type:
PUBLIC HOME / VISUAL REWORK / PRODUCT EXPERIENCE

Baseline:
183b8c77b76bfc91e8d3f8f7e2901b6a1f5027ed

Prior Implementation:
d1760450c1e7efa316507ddc742bc0eeb8b57b2c (TC-P30-T005 foundation)

Auto-Execute:
YES (USER PIPELINE + architect authorization after Visual Checkpoint C REWORK_REQUIRED)

Stop-After-Result:
YES


======================================================================
0. PURPOSE
======================================================================

Raise the existing Public Home from foundation/skeleton quality to a
premium travel commerce homepage suitable for customer-facing review.

Aligned with:

- TravelCore Product Experience Constitution
- Design System 2.0
- North Star asset
- Public Shell (T004)
- Visual Checkpoint C findings

Commercial target feeling:

Premium · Modern · Trustworthy · Travel-first · Visual · Conversion-oriented

NOT:

- developer landing page
- empty wireframe board
- text + box only layout
- SaaS template feel


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
docs/product-experience/evidence/P30-T005/VISUAL-CHECKPOINT-C.md
docs/plans/P30-implementation-plan.md
docs/plans/TC-P30-T005-task-envelope.md
docs/plans/TC-P30-T005-REWORK-task-envelope.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Confirm:

- Current phase is P30
- T005 technical PASS exists
- Visual Checkpoint C REWORK_REQUIRED
- T006 remains blocked
- Working tree CLEAN at start
- North Star PNG present


======================================================================
2. REWORK REQUIREMENTS (MANDATORY)
======================================================================

Keep the existing section skeleton where useful. Upgrade visual/commercial quality.

### 2.1 Hero

FROM:
Gradient + text + buttons

TO:
- Travel image / destination visual treatment
- Search experience prominence
- Primary conversion CTAs

Target pattern (honest capability UI — not fake live inventory):

- Prompt like: «کجا می‌خواهید سفر کنید؟»
- Search fields: destination · date · travelers
- Primary search CTA

Do NOT invent live availability/prices in search results.
Search may route to existing honest surfaces (/tours, /hotels, /plan, /flights).

### 2.2 Destination cards

FROM:
Text navigation intents only

TO:
Visual cards with:
- Image treatment
- Destination name
- Supporting signal when real (e.g. count) OR omit numbers if unknown
- Clear entry CTA

No invented destination inventory / fake “7 tours” counts unless derived from real data.

### 2.3 Tour cards

FROM:
Section CTA only

TO:
Card presentation with:
- Image treatment
- Title
- Destination
- Duration (when known)
- Price ONLY if real authoritative data exists; otherwise omit price
- View CTA

If catalog empty: premium empty state (not a single bare muted line).

### 2.4 Hotel section

Even if backend composition is empty, UI must feel ready:

- Popular hotels band structure
- Card placeholders / premium empty composition
- Not a thin one-line empty sentence alone

Still: never invent hotel ratings/prices/availability.

### 2.5 Trust strip

Add a commercial trust strip feel, e.g. capability cues:

- travel support
- secure payment posture (no fake certifications)
- curated discovery
- trustworthy experience

No fabricated partner logos, review counts, or guarantees that are not product-true.

### 2.6 Visual quality law

Every major section must include at least one of:

- imagery
- card composition
- interaction
- decision-useful information

The page must not remain “text + boxes only”.


======================================================================
3. HONESTY RULES (LOCKED)
======================================================================

- Prefer high-quality empty / designed skeleton over fake commerce facts
- Never invent availability, prices, discounts, ratings, review counts
- Prefer omit over fabricate
- DEMOFEED remains DEFERRED unless separately authorized


======================================================================
4. ALLOWED FILES
======================================================================

docs/product-experience/**
docs/plans/P30-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/plans/TC-P30-T005-REWORK-task-envelope.md

Frontend (home / public experience only):

src/frontend/web/src/app/[locale]/page.tsx
src/frontend/web/src/features/home-discovery/**
src/frontend/web/src/components/shell/** (consume / minor extend only)
src/frontend/web/src/components/ui/** (reuse primitives; no second design system)
src/frontend/web/src/styles/** (token mapping only if required)

If additional home-adjacent files are strictly required, list + justify in RESULT.
Prefer minimal diff.


======================================================================
5. FORBIDDEN
======================================================================

Do NOT:

- execute TC-P30-T006
- execute DEMOFEED
- redesign backend / domain / APIs / database / migrations
- invent fake commerce facts
- pixel-clone competitors
- modify North Star image
- invent next Task-ID

Do NOT touch unless required for compile green:

package.json / package-lock.json (default FORBIDDEN)
*.csproj / database / migrations
admin/** product workflows
agency portal rewrite


======================================================================
6. VISUAL EVIDENCE
======================================================================

After rework, provide:

- desktop screenshot of /fa
- mobile screenshot of /fa
- evidence note under docs/product-experience/evidence/P30-T005/
- explicit North Star comparison

Architect visual review remains mandatory.
Cursor PASS ≠ Architect ACCEPT.


======================================================================
7. VALIDATION
======================================================================

Run:

git diff --check
git status --short
git diff --name-only
npm run typecheck (in src/frontend/web)

Require:

- allowed paths only (or justified list)
- homepage feels closer to travel commerce than foundation skeleton
- honesty rules intact
- Working Tree CLEAN after push

Commit message:

feat(ui): rework P30 public home toward travel commerce quality

Push origin main.

Verify HEAD == origin/main and Working Tree CLEAN.


======================================================================
8. RESULT FORMAT
======================================================================

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P30-T005-REWORK
Phase: P30 — Product Experience Foundation
Status: PASS | REWORK_RECOMMENDED | BLOCKED_REPOSITORY_STATE | BLOCKED_ARCHITECTURE_CONFLICT

Include:

Repository · Branch · Baseline · Implementation-Commit · HEAD · origin/main · Working-Tree
Artifacts / sections upgraded
Visual evidence paths
North Star alignment YES/PARTIAL/NO
Honesty preserved YES/NO
Product Code Changed: YES
Dependencies Changed: YES/NO
Validation
Recommended architect decision
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
Execute TC-P30-T005-REWORK from docs/plans/TC-P30-T005-REWORK-task-envelope.md
```

(inside a valid BEGIN/END envelope or equally explicit authorized execute cycle)

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Created by `TC-P30-T005-REWORK-ENVELOPE-CREATE` · visual rework SoT after Checkpoint C REWORK_REQUIRED |
