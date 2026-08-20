# TC-P30-T002 Task Envelope (persistent · anti-truncation)

| Field | Value |
|-------|--------|
| Envelope-ID | `TC-P30-T002-ENVELOPE-CREATE` authored this file |
| Executable Task-ID | `TC-P30-T002` |
| Phase | P30 — Product Experience Foundation |
| Purpose of this file | Persist the full authorized execution envelope so ChatGPT UI truncation cannot destroy Pipeline integrity |
| Product code | **NO** — T002 is documentation / product-experience governance only |
| Baseline at envelope authoring | `7e324ec` |
| North Star prerequisite | `docs/product-experience/assets/travelcore-ui-ux-north-star.png` (PNG · locked) |

> **Do not execute `TC-P30-T002` from `TC-P30-T002-ENVELOPE-CREATE`.**  
> Execution of T002 requires a separate authorized cycle that points at this file (or pastes the live block below).

---

## Live execution block (complete)

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P30-T002

Phase:
P30 — Product Experience Foundation

Title:
Visual Benchmark and Product Direction Constitution

Status:
AUTHORIZED

Task-Type:
PRODUCT EXPERIENCE GOVERNANCE / DESIGN CONSTITUTION / DOCUMENTATION LOCK

Baseline:
7e324ec1cfeaf65528b88d54ab98782c0e6dec02

Auto-Execute:
YES (USER PIPELINE + architect authorization after Health Check PASS)

Stop-After-Result:
YES


======================================================================
0. PURPOSE
======================================================================

Create the official TravelCore Product Experience direction.

This task defines the visual and UX foundation for future Public, Admin,
and Agency experiences.

This task is documentation and product-experience governance only.

No UI implementation is allowed.


======================================================================
1. PIPELINE CONTROLLER CHECK
======================================================================

Before execution read:

docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md
docs/plans/P30-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Confirm:

- Current phase is P30
- North Star exists at docs/product-experience/assets/travelcore-ui-ux-north-star.png
- North Star is real PNG
- Task envelope is valid
- No conflicting unfinished product work
- Working tree CLEAN at start (or only authorized docs dirty during this task)


======================================================================
2. APPROVED VISUAL NORTH STAR
======================================================================

Required asset:

docs/product-experience/assets/travelcore-ui-ux-north-star.png

Rules:

- Do NOT generate a replacement
- Do NOT modify the image
- Do NOT redraw / crop / recompress
- North Star is DIRECTIONAL, not pixel-perfect
- Future UI must not materially regress below it in professional/product quality
- Does NOT authorize fake domain data or competitor cloning


======================================================================
3. CREATE / UPDATE ARTIFACTS
======================================================================

Create directory if needed:

docs/product-experience/


Create / ensure:

docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md
docs/product-experience/P30-VISUAL-ACCEPTANCE-CHECKLIST.md
docs/product-experience/P30-PUBLIC-EXPERIENCE-SPEC.md
docs/product-experience/P30-ADMIN-EXPERIENCE-SPEC.md
docs/product-experience/P30-AGENCY-EXPERIENCE-SPEC.md


Update:

docs/plans/P30-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md


======================================================================
4. REQUIRED CONTENT LOCKS
======================================================================

### 4.1 Core principle

TravelCore must never accept a technically correct but commercially weak
product surface as complete.

Automated tests are NECESSARY but NOT SUFFICIENT for major UI tasks.

Major P30 surfaces require:

Technical Gate
+
Visual/Product Gate

No Page-First Development.

One Design System.
Three Experiences:

1. Public Marketplace
2. Admin Console
3. Agency Portal


### 4.2 Product feeling

Lock desired feeling:

Premium · Modern · Trustworthy · Travel-first · Visual · Calm but rich ·
Conversion-oriented · Content-rich without clutter · Operationally serious ·
Mobile-first · Accessible · SEO-compatible

Must NOT feel like:

developer demo · framework starter · generic SaaS template · old agency site ·
plain CRUD admin · Bootstrap internal tool · unrelated cards · DB schema as forms


Organizing principle (not marketing copy unless later approved):

Discover + Trust + Book


### 4.3 Competitive benchmark policy

Iranian references: LastSecond · Tahagasht
Global maturity: Booking · Airbnb · Tripadvisor
Admin references: Stripe Dashboard · Shopify Admin · Linear · Vercel Dashboard

NO PIXEL CLONING
NO BRAND / LOGO / COLOR-SYSTEM / TRADE-DRESS COPYING

Output: TravelCore Design Language


### 4.4 Three experience principles

Public:
- discovery first
- confidence before purchase
- rich media
- clear pricing
- mobile first

Admin:
- professional
- efficient
- workflow oriented
- data intensive but usable
- Operational Command Center (not public restyle / not generic CRUD)

Agency:
- business focused
- operational speed
- trust
- clarity
- Sales Workspace (not Admin recolored)
- no fake commission/credit/wallet unless domain capability exists


### 4.5 Shared design language

Define how Public/Admin/Agency share:

- visual identity
- interaction principles
- component philosophy
- tokens / accessibility / spacing / icons / responsive primitives

Experience-specific components allowed. Avoid over-generalization.


### 4.6 Visual direction (philosophy — not final hex lock)

Direction from North Star:

- Primary family: Deep Ocean / trustworthy deep blue
- Accent family: Warm gold / sunset
- Surfaces: warm white / calm neutrals
- Dark surfaces: deep neutral/navy (not pure black by default)

Exact final token hex values belong to TC-P30-T003 unless already accepted tokens exist.

Also lock:

restrained color · semantic color · strong contrast · premium imagery ·
purposeful whitespace · coherent spacing · modern rounded geometry ·
restrained elevation · low noise · consistent icons · clear hierarchy

Avoid:

excessive gradients · glassmorphism · neon · random colors · excessive shadows ·
tiny dense cards · ornamental animation · low-contrast fake luxury


### 4.7 Typography / image-first / trust

- Excellent FA / AR / EN readability
- No page invents its own typography system
- Technical values remain bidi-safe
- Travel is a visual product — image-first public surfaces when data exists
- No fabricated factual imagery
- No fake urgency / ratings / scarcity / discounts / logos


### 4.8 Public / Hotel / Tour / Destination hierarchies

Lock Home hierarchy intent:

Hero · Search · Destinations · Tours · Hotels · Stories/UGC · Trust · Footer

Lock Hotel Listing/Detail and Tour Listing/Detail hierarchies while preserving:

Place ≠ HotelBooking
TourProduct ≠ TourDeparture
Pricing/Booking ownership unchanged

Destination pages = discovery hubs, not encyclopedia dumps.


### 4.9 Admin data grid / workflow

Lock professional Data Grid contract direction for later T008:

server pagination · filtering · sorting · column control · saved views ·
selection · bulk actions · export boundary · keyboard · loading/empty/error ·
responsive card/list transform on mobile

Lock: Workflow over Database Form


### 4.10 Cross-cutting

- FA = RTL · AR = RTL · EN = LTR · direction-neutral components
- Light/Dark token architecture; dark is intentional, not invert
- Loading / Empty / Error / Partial Data required for major shared components
- Accessibility direction: keyboard · focus · landmarks · contrast · reduced-motion
- SEO as product experience (semantic hierarchy, crawlable content, internal links)
- Server Component First preserved
- Motion: subtle · purposeful · fast · respectful
- Experience → Data → Commercial order locked
- DEMOFEED remains DEFERRED until after P30 Experience


### 4.11 Visual acceptance protocol

Create checklist covering:

Product feel · hierarchy · imagery · composition · typography · spacing ·
conversion · trust · responsive · RTL/LTR · a11y · domain truth · design system ·
North Star regression

Lock:

Cursor Implementation
→ Automated Validation
→ Screenshot Evidence
→ Architect/User Visual Review
→ ACCEPT or REWORK

Automated tests alone are insufficient for major visual surfaces.

Visual rejection => REWORK_REQUIRED (legitimate Pipeline stop).

User may request visual checkpoint («ببینیم الان چه شکلی شده»).


### 4.12 P30 execution map lock

Update plan sequence:

TC-P30-PLAN ACCEPTED
→ TC-P30-T002 (this task)
→ TC-P30-T003 Design System 2.0
→ Visual Checkpoint A
→ TC-P30-T004 Shells
→ Checkpoint B
→ TC-P30-T005 Home
→ Checkpoint C
→ TC-P30-T006 Hotel
→ Checkpoint D
→ TC-P30-T007 Tour
→ Checkpoint E
→ TC-P30-T008 Admin foundation (+ data grid)
→ Checkpoint F
→ TC-P30-T009 Agency foundation
→ Checkpoint G
→ TC-P30-GATE
→ DEMOFEED may be reconsidered


======================================================================
5. ALLOWED FILES
======================================================================

docs/product-experience/**
docs/plans/P30-implementation-plan.md
docs/plans/TC-P30-T002-task-envelope.md  (already exists; may sync status notes only)
docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md


Approved North Star asset path (read-only in T002):

docs/product-experience/assets/travelcore-ui-ux-north-star.png


======================================================================
6. FORBIDDEN
======================================================================

Do NOT modify:

src/**
tests/**
database
migrations
dependencies
APIs
frontend implementation
backend implementation

Do NOT:

- execute TC-P30-T003
- implement UI / create components / pages
- execute DEMOFEED
- invent next Task-ID
- fake product/domain facts
- pixel-clone competitors
- redesign Identity/Access/Party/Place/Tour/Pricing/Booking/Payment ownership


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
- no product code changes

Commit message:

docs(product-experience): lock P30 design constitution and visual north star

Push origin main.

Verify HEAD == origin/main and Working Tree CLEAN.


======================================================================
8. RESULT FORMAT
======================================================================

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P30-T002
Phase: P30 — Product Experience Foundation
Status: PASS | BLOCKED_APPROVED_NORTH_STAR_MISSING | BLOCKED_REPOSITORY_STATE | BLOCKED_ARCHITECTURE_CONFLICT

Include:

Repository · Branch · Baseline · Implementation-Commit · HEAD · origin/main · Working-Tree
Artifacts paths
North Star evidence
Product Experience locks
Public/Admin/Agency locks
Cross-cutting locks
Visual Acceptance Protocol
Recovery Lock
P30 Execution Map
DEMOFEED Status
Architecture Conflict YES/NO
Ownership Conflict YES/NO
Product Code Changed: NO
Validation
Cumulative P30 Ledger
Next-State: AWAITING_ARCHITECT_REVIEW

STOP.
Do not execute TC-P30-T003.
Do not implement UI.
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
| 2026-08-20 | Created by `TC-P30-T002-ENVELOPE-CREATE` · persistent anti-truncation envelope |
| 2026-08-20 | Executed via `TC-P30-T002-EXECUTE` · constitution artifacts authored · AWAITING_ARCHITECT_REVIEW |
