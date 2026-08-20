# TC-PIPELINE-AUTOMATION-V2-POC Task Envelope (persistent · anti-truncation)

| Field | Value |
|-------|--------|
| Envelope-ID | `TC-PIPELINE-AUTOMATION-V2-POC-ENVELOPE-CREATE` authored this file |
| Executable Task-ID | `TC-PIPELINE-AUTOMATION-V2-POC` |
| Phase | Post-P30 — Pipeline Automation |
| Title | Pipeline Automation V2 — Repository Inbox Proof of Concept |
| Purpose of this file | Persist the full authorized POC envelope so ChatGPT UI truncation cannot destroy Pipeline integrity |
| Product code | **NO** — docs / pipeline conventions / thin trigger evaluation only |
| Baseline at envelope authoring | `91508ef` (`main`) |
| Prerequisites | `TC-CURSOR-CAPABILITY-REVIEW-001` PASS (research) · USER PIPELINE mode · Architect authorization for POC execute cycle |
| Related research | Capability review concluded Option B (Cursor + repository task queue) is the simplest reliable spine |

> **Do not execute `TC-PIPELINE-AUTOMATION-V2-POC` from `TC-PIPELINE-AUTOMATION-V2-POC-ENVELOPE-CREATE`.**  
> Envelope-create only persists this file.  
> Execution requires a separate authorized cycle that points at this file (or pastes the live block below).

---

## Architect decision (locked for POC design)

```text
Target workflow (V2):

Architect Task
        |
        v
Repository Task Inbox
        |
        v
Cursor Automation Trigger
        |
        v
Execution
        |
        v
Repository Result

Design constraints:
- Do NOT build a large orchestration system
- Do NOT change product / frontend / backend code
- Prefer thinnest reliable Cursor-native + repo-queue approach
- Keep TRAVELCORE_CURSOR_TASK_V1 / RESULT_V1 and recovery protocol compatible
```

Prior capability findings (non-executable summary):

- Cloud Agents + Automations / API / CLI exist
- No native ChatGPT watcher or forever-poll of custom envelope files
- Closest native spine: repo inbox event → Automation webhook/push → agent → RESULT in repo

---

## Live execution block (complete)

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-PIPELINE-AUTOMATION-V2-POC

Phase:
Post-P30 — Pipeline Automation

Title:
Pipeline Automation V2 — Repository Inbox Proof of Concept

Status:
AUTHORIZED (only when a separate live execute cycle cites this file)

Task-Type:
PIPELINE / AUTOMATION POC / DOCS + THIN CONVENTION

Baseline:
91508ef (or current origin/main at execute time if architect updates)

Auto-Execute:
YES (USER PIPELINE + architect authorization for this Task-ID)

Stop-After-Result:
YES


======================================================================
0. PURPOSE
======================================================================

Prove a minimal repository-based automation workflow for TravelCore
Pipeline — without building a platform and without touching product code.

The POC must evaluate and document (and only then, if needed, add thin
docs/convention artifacts) the six areas below.

Aligned with:

- docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md
- docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md
- docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
- docs/ai/01-chatgpt-cursor-handoff-protocol.md
- docs/plans/TC-PIPELINE-AUTOMATION-V2-POC-task-envelope.md
- TC-CURSOR-CAPABILITY-REVIEW-001 findings (Option B preferred)


======================================================================
1. PIPELINE CONTROLLER CHECK
======================================================================

Before execution read:

docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md
docs/ai/01-chatgpt-cursor-handoff-protocol.md
docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md
docs/plans/TC-PIPELINE-AUTOMATION-V2-POC-task-envelope.md

Confirm:

- This Task-ID is the latest complete valid unexecuted envelope
- No replay of an already-PASS Task-ID
- No product-code scope creep
- Envelope-create cycle is NOT this execute cycle


======================================================================
2. POC EVALUATION REQUIREMENTS (MANDATORY)
======================================================================

The POC must evaluate and record decisions for:

----------------------------------------------------------------------
2.1 Repository task inbox location
----------------------------------------------------------------------

Propose ONE canonical inbox path under the repository, e.g. candidate:

  docs/pipeline/inbox/

Document:

- Where authorized task envelopes land
- Naming convention (Task-ID based)
- Relationship to existing docs/plans/*-task-envelope.md persistence
- What is NOT an inbox (chat paste alone, historical quotes, examples)

----------------------------------------------------------------------
2.2 Authorized task detection
----------------------------------------------------------------------

Document how a worker/Automation decides a task is executable:

- BEGIN_TRAVELCORE_CURSOR_TASK_V1 … END_TRAVELCORE_CURSOR_TASK_V1
- Protocol-Version: 1
- Complete, untruncated
- Auto-Execute: YES (when PIPELINE USER mode applies)
- Latest complete unexecuted for that Task-ID
- Not an illustrative / suffixed / quoted sample
- Recovery / SoT conflict checks still apply

Explicit non-detection:

- Chat history without repo persistence
- Partial envelopes
- Already executed Task-IDs (replay)

----------------------------------------------------------------------
2.3 Cursor execution trigger options
----------------------------------------------------------------------

Compare at least these options and recommend ONE for the POC:

A. Cursor Automations (push / path filter / webhook / cron)
B. Cloud Agents API / SDK follow-up runs
C. Local headless CLI (`agent -p`) invoked by a tiny watcher
D. Manual paste into IDE Agent (baseline / fallback)

POC must pick the smallest reliable option that fits TravelCore.
Prefer Option B from capability review: repo queue + Cursor trigger.
Do NOT implement a large orchestrator.

----------------------------------------------------------------------
2.4 Result storage convention
----------------------------------------------------------------------

Propose ONE canonical result location, e.g. candidate:

  docs/pipeline/results/

Document:

- RESULT file naming (Task-ID)
- Required TRAVELCORE_CURSOR_RESULT_V1 markers
- Commit / PR comment expectations (minimal)
- How Architect reads acceptance state from repo SoT

----------------------------------------------------------------------
2.5 Recovery compatibility
----------------------------------------------------------------------

POC must remain compatible with:

- TRAVELCORE-RECOVERY-CONTEXT.md updates after accepted work
- PROJECT-STATE / ROADMAP authority when conflicted
- No Envelope = No Execution
- Cursor PASS ≠ Architect ACCEPT
- After RESULT: wait for architect; do not invent next task

Document any inbox/result convention touch-points for recovery docs
(without rewriting product architecture).

----------------------------------------------------------------------
2.6 Replay protection
----------------------------------------------------------------------

Document mandatory replay guards:

- Same Task-ID with prior PASS/ACCEPT must not re-execute
- Duplicate commits for replayed Task-ID forbidden
- Inbox item lifecycle: received → claimed/executing → completed/failed
- Idempotent handling if Automation fires twice on same commit
- Illustrative envelopes in docs must remain non-executable
  (suffixed markers / clearly marked samples)


======================================================================
3. ALLOWED WORK (EXECUTE CYCLE ONLY)
======================================================================

When TC-PIPELINE-AUTOMATION-V2-POC is separately authorized:

ALLOWED:

- Docs under docs/pipeline/** (inbox/results conventions, README, samples
  that are explicitly non-executable)
- Thin evidence / decision record for the POC under docs/plans/ or
  docs/pipeline/
- Optional minimal example inbox/result filenames (empty or sample with
  non-live markers)
- Updates to recovery/controller docs ONLY if strictly required for
  inbox/result pointers (prefer minimal)

FORBIDDEN:

- Frontend / backend / product code changes
- Large orchestration services, daemons, platforms
- Changing P30 product experience implementation
- Executing unrelated product tasks
- Enabling always-on ChatGPT UI scraping as the primary design
- Broad refactors of docs/ai protocol family


======================================================================
4. OUT OF SCOPE
======================================================================

- Full unattended ChatGPT↔Cursor closed loop without human/Architect
- Replacing Architect acceptance with automation
- Multi-repo orchestration
- Billing / team Automations production hardening beyond POC notes
- DEMOFEED / TC-P30-T006 / product UI work


======================================================================
5. DELIVERABLES (EXECUTE CYCLE)
======================================================================

Minimum:

1. Written POC decision record covering §2.1–§2.6
2. Recommended inbox path + result path
3. Recommended trigger option (A/B/C/D) with rationale
4. Replay + recovery compatibility notes
5. Explicit “what we will NOT build” list
6. RESULT envelope with Status PASS or FAIL

Optional (only if needed for clarity):

- docs/pipeline/README.md
- placeholder inbox/results directories with .gitkeep
- one clearly marked NON-EXECUTABLE sample envelope


======================================================================
6. VALIDATION (EXECUTE CYCLE)
======================================================================

- git diff --check
- Only allowed paths changed (docs/pipeline/** and/or the POC decision
  doc under docs/plans/** — no src/**)
- No product code
- Working Tree CLEAN after commit
- HEAD == origin/main after push


======================================================================
7. COMMIT / PUSH (EXECUTE CYCLE)
======================================================================

Suggested commit message:

docs: add pipeline automation v2 poc conventions

Push origin/main.
Working Tree CLEAN.


======================================================================
8. RESULT FORMAT
======================================================================

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version:
1

Task-ID:
TC-PIPELINE-AUTOMATION-V2-POC

Status:
PASS | FAIL

Include:
- Inbox location decision
- Detection rules summary
- Trigger recommendation
- Result storage convention
- Recovery compatibility
- Replay protection
- Commit / Validation / HEAD == origin/main / Working Tree CLEAN

Next-State:
AWAITING_ARCHITECT_REVIEW

STOP.

END_TRAVELCORE_CURSOR_RESULT_V1


END_TRAVELCORE_CURSOR_TASK_V1
```

---

## Envelope-create cycle (this file authoring)

| Item | Value |
|------|--------|
| Task-ID | `TC-PIPELINE-AUTOMATION-V2-POC-ENVELOPE-CREATE` |
| Allowed change | `docs/plans/TC-PIPELINE-AUTOMATION-V2-POC-task-envelope.md` only |
| Commit | `docs: add pipeline automation v2 poc envelope` |
| Implementation | **Not started** — wait for separate `TC-PIPELINE-AUTOMATION-V2-POC` execute authorization |

---

## Non-goals (hard)

- Do not implement Automation / webhook / CLI worker in the envelope-create cycle
- Do not modify `src/**`
- Do not invent product tasks
- Do not treat this markdown’s fenced live block as already executing
