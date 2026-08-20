# TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST Task Envelope (persistent · anti-truncation)

| Field | Value |
|-------|--------|
| Envelope-ID | `TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST-ENVELOPE-CREATE` authored this file |
| Executable Task-ID | `TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST` |
| Phase | Post-P30 — Pipeline Automation |
| Title | Pipeline Automation V2 — Minimal Inbox Trigger Test |
| Purpose of this file | Persist the full authorized trigger-test envelope so ChatGPT UI truncation cannot destroy Pipeline integrity |
| Product code | **NO** — verification artifact / lifecycle proof only |
| Baseline at envelope authoring | `e91aaf6` (`main`) |
| Prerequisites | `TC-PIPELINE-AUTOMATION-V2-POC` conventions on `main` · inbox/results paths exist · USER PIPELINE mode · separate execute authorization |
| Conventions SoT | `docs/pipeline/README.md` · `docs/pipeline/TC-PIPELINE-AUTOMATION-V2-POC-DECISION.md` |

> **Do not execute `TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST` from `TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST-ENVELOPE-CREATE`.**  
> Envelope-create only persists this file.  
> **Do not implement the Cursor Automation trigger in this create cycle.**  
> **Do not run the trigger test in this create cycle.**  
> Execution requires a later authorized cycle (typically via inbox drop + configured trigger, or live cite of this file).

---

## Architect intent (locked)

```text
Expected future flow:

Task file appears in docs/pipeline/inbox/
        ↓
Automation detects it
        ↓
Cursor executes authorized task
        ↓
Result written to docs/pipeline/results/

This envelope prepares that test — it does not install Automations
and does not execute the test.
```

---

## Live execution block (complete)

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST

Phase:
Post-P30 — Pipeline Automation

Title:
Pipeline Automation V2 — Minimal Inbox Trigger Test

Status:
AUTHORIZED (only when a separate live execute cycle cites this file
or a matching inbox task is dropped under pipeline rules)

Task-Type:
PIPELINE / TRIGGER TEST / VERIFICATION ONLY

Baseline:
e91aaf6 (or current origin/main at execute time if architect updates)

Auto-Execute:
YES (USER PIPELINE + architect authorization for this Task-ID)

Stop-After-Result:
YES


======================================================================
0. PURPOSE
======================================================================

Prove — with the smallest safe change set — that an authorized task
originating from the repository inbox path can be executed and can
write a RESULT under the canonical results path.

This is a lifecycle / trigger verification test, not product work.

Aligned with:

- docs/pipeline/README.md
- docs/pipeline/TC-PIPELINE-AUTOMATION-V2-POC-DECISION.md
- docs/plans/TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST-task-envelope.md
- docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md
- docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md


======================================================================
1. PIPELINE CONTROLLER CHECK
======================================================================

Before execution read:

docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md
docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md
docs/pipeline/README.md
docs/pipeline/TC-PIPELINE-AUTOMATION-V2-POC-DECISION.md
docs/plans/TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST-task-envelope.md

Confirm:

- Task-ID is latest complete valid unexecuted envelope
- No replay (no prior PASS/ACCEPT RESULT for this Task-ID)
- Envelope-create cycle is NOT this execute cycle
- Trigger implementation is out of scope unless a prior authorized
  task explicitly configured it; this test may still run via manual
  IDE paste (fallback D) if Automations are not yet wired
- No product-code scope creep


======================================================================
2. EXPECTED INBOX SIGNAL (WHEN USING AUTOMATION PATH)
======================================================================

Runnable signal file (when Automations are used):

  docs/pipeline/inbox/TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST.task.md

Must contain the live TRAVELCORE_CURSOR_TASK_V1 body for this Task-ID
(or a pointer that the agent must execute this persistent plans envelope).

Lifecycle (per POC decision):

  received (*.task.md)
    → claimed/executing (*.executing.md optional)
    → completed after RESULT commit (inbox cleared/archived)


======================================================================
3. ALLOWED WORK (EXECUTE CYCLE ONLY)
======================================================================

ALLOWED:

- Create verification RESULT artifact:
    docs/pipeline/results/TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST.result.md
- Optional thin evidence note under docs/pipeline/ (lifecycle checklist only)
- Inbox lifecycle hygiene for this Task-ID only
  (claim/complete the matching inbox file if present)
- Commit + push as specified below

FORBIDDEN:

- Product code changes
- Frontend changes
- Backend changes
- Database changes
- Dependency changes
- ChatGPT browser automation
- Large worker / orchestrator implementation
- Configuring team Automations/secrets unless a separate authorized
  task explicitly allows it (not this Task-ID by default)
- src/** changes of any kind
- Unrelated P30 / DEMOFEED work


======================================================================
4. EXECUTE STEPS (MINIMAL)
======================================================================

1. Detect authorization (inbox *.task.md and/or live cite of this file).
2. Optionally rename inbox item to *.executing.md (claim).
3. Write RESULT file proving:
   - Task-ID
   - How the task was triggered (Automation | manual paste | other)
   - Inbox path observed (or “none — live cite”)
   - Result path written
   - Lifecycle status (received → executing → completed)
   - Confirmation: zero product/src changes
4. Clear or archive inbox item for this Task-ID.
5. git diff --check · commit · push · Working Tree CLEAN.


======================================================================
5. VALIDATION (EXECUTE CYCLE)
======================================================================

- git diff --check
- Allowed paths only under docs/pipeline/** (RESULT + optional thin notes
  + this Task-ID inbox lifecycle files)
- No src/** · no package manifests · no product docs drive-by edits
- HEAD == origin/main after push
- Working Tree CLEAN


======================================================================
6. COMMIT / PUSH (EXECUTE CYCLE)
======================================================================

Suggested commit message:

docs: record pipeline v2 trigger test result

Push origin/main.
Working Tree CLEAN.


======================================================================
7. RESULT FORMAT
======================================================================

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version:
1

Task-ID:
TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST

Status:
PASS | FAIL

Include:
- Trigger mode used (Automation | manual | other)
- Inbox observation
- Result artifact path
- Lifecycle proof
- Confirmation: no product/src changes
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
| Task-ID | `TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST-ENVELOPE-CREATE` |
| Allowed change | `docs/plans/TC-PIPELINE-AUTOMATION-V2-TRIGGER-TEST-task-envelope.md` only |
| Commit | `docs: add pipeline trigger test envelope` |
| Trigger implementation | **Not started** |
| Trigger test execution | **Not started** |

---

## Non-goals (hard for create cycle)

- Do not implement Cursor Automations / webhooks / CLI workers
- Do not drop the inbox `*.task.md` for the test yet (unless a later authorized task says so)
- Do not write the trigger-test RESULT yet
- Do not modify `src/**` or product code
- Do not treat this markdown’s fenced live block as already executing
