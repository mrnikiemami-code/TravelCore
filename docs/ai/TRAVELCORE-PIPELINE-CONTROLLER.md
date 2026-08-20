# TravelCore Pipeline Controller V3

| Field | Value |
|-------|--------|
| Document | `docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md` |
| Version | **V3** — File-Based Task Pipeline |
| Status | **ACTIVE** — mandatory under PIPELINE mode |
| Parent protocol | [`TRAVELCORE-PIPELINE-PROTOCOL.md`](TRAVELCORE-PIPELINE-PROTOCOL.md) |
| Recovery entry | [`../prompts/START-HERE-IF-CHATGPT-IS-LOST.md`](../prompts/START-HERE-IF-CHATGPT-IS-LOST.md) |
| Fast recovery | [`TRAVELCORE-RECOVERY-CONTEXT.md`](TRAVELCORE-RECOVERY-CONTEXT.md) |
| Inbox conventions | [`../pipeline/README.md`](../pipeline/README.md) |
| Governance | ADR 0013 · ADR 0014 |
| Task-ID (this rewrite) | `TC-PIPELINE-CONTROLLER-V3-REWRITE-001` |

This document is the **single** Cursor execution contract for TravelCore PIPELINE mode.

It replaces prior mixed models (chat-envelope scanning, scroll discovery, permanent daemon polling).

---

## 1. Authority Model

| Role | Actor |
|------|--------|
| Architect | ChatGPT (authoritative channel) |
| Implementation Agent | Cursor |
| Human Execution Authority | USER |
| Source of Truth | **Repository** |

### Critical rules

| Rule | Meaning |
|------|---------|
| Repository is SoT | Chat memory is not durable architecture authority |
| Chat is communication | Architect RESULT delivery + cues for **task file** availability |
| Chat is **not** task source | Do **not** treat chat prose as an executable task |
| Cursor PASS ≠ Architect ACCEPT | RESULT success ≠ product/architecture acceptance |

### Authoritative architect channel

```text
https://chatgpt.com/g/g-p-6a79dbc6468c8191a5e74afa2d82a8be-travelcore/c/6a8039a8-2014-83ed-be9f-813280b23bcb
```

Do not switch to another ChatGPT conversation as the architect command channel.

If the channel is unavailable:

```text
STOP
Status = BLOCKED_ARCHITECT_CHANNEL_UNAVAILABLE
```

---

## 2. Core Architecture — File-Based Task Pipeline

### Source of Task

**ONLY files.**

| File | Role |
|------|------|
| `.task.md` | Executable implementation / docs / governance task |
| `.gate.md` | Checkpoint / review / decision task |
| `.result.md` | Optional repository audit artifact only — **not** primary Architect communication |

### Explicitly forbidden as task source

- Searching chat text for `BEGIN_TRAVELCORE_CURSOR_TASK_V1` / `END_…` as the execution authority
- Scroll-based discovery of task envelopes inside conversation history
- ROADMAP / deferred items as execution commands
- Examples (`__EXAMPLE` / `NON_EXECUTABLE_EXAMPLE`)
- Historical / already-executed Task-IDs (replay)
- Inventing the next task

### Preferred runnable locations

```text
docs/pipeline/inbox/<Task-ID>.task.md
docs/pipeline/inbox/<Gate-ID>.gate.md
```

Architect may also provide a downloadable task/gate file. Cursor must **read that file** (persist to inbox when useful) before execution.

---

## 3. Task Lifecycle (only lifecycle)

```text
TASK FILE
    ↓
VALIDATION
    ↓
EXECUTION
    ↓
RESULT TO ARCHITECT CHAT
    ↓
WAITING MODE
    ↓
NEXT TASK FILE
```

One consistent model. No alternate acquisition paths.

---

## 4. Validation

A file is executable only when **all** hold:

| Check | Requirement |
|-------|-------------|
| Extension | `.task.md` or `.gate.md` |
| Markers | Exact `BEGIN_TRAVELCORE_CURSOR_TASK_V1` … `END_TRAVELCORE_CURSOR_TASK_V1` (unsuffixed) |
| Task-ID | Present and non-empty |
| Phase | Present |
| Scope | Explicit allow / forbid |
| Validation | Present |
| Freshness | Task-ID not already executed / PASS-replayed |
| Integrity | Not nested inside a RESULT-only sample; not an example |

Without a valid file:

```text
Status = BLOCKED_NO_AUTHORIZED_TASK
```

Do **not** execute.

**No Envelope = No Execution** (file envelope required).

---

## 5. EXECUTION MODE

When a valid task/gate file exists:

1. Read the file  
2. Validate (§4)  
3. Run Recovery Before Execution (§9)  
4. Execute **only** this task/gate  

### During EXECUTION MODE — forbidden

- Looking for the next task  
- Reading future tasks  
- Refreshing Architect chat to hunt for work  
- Parallel execution  
- Inventing missing requirements  
- Continuous polling  

Only the **current** authorized file is active.

---

## 6. Result Contract

After completion, Cursor **MUST** send to the **Architect chat**:

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
...
END_TRAVELCORE_CURSOR_RESULT_V1
```

### Required RESULT fields

- Task-ID  
- Status  
- Summary  
- Changed files  
- Validation  
- Commit (when applicable)  
- Evidence (when applicable)  
- HEAD status  
- Working Tree status  
- Next-State  

### Additional required fields for UI / Product Experience tasks

- Evidence paths  
- Visual review summary  
- Known limitations  
- Acceptance risks  

See §8 Visual Evidence Review.

Optional: write `docs/pipeline/results/<Task-ID>.result.md` for repository audit only. It does **not** replace Architect-chat RESULT.

After RESULT:

```text
Next-State = AWAITING_ARCHITECT_REVIEW
```

Then leave EXECUTION MODE and enter **WAITING MODE** (§7).

---

## 7. WAITING MODE

### Important

The worker is **NOT** a permanent background daemon.

The worker activates for waiting **ONLY after RESULT is sent**.

### Lifecycle

```text
RESULT SENT
    ↓
WAIT 80 seconds
    ↓
Check Architect communication channel for next task FILE availability
```

**File availability** means a new authorized `.task.md` / `.gate.md` (downloadable file / inbox file / explicit file handoff) — **not** scanning chat prose for BEGIN/END envelopes and **not** scroll-mining conversation history.

If next authorized task file exists:

```text
Read task file
    ↓
Validate
    ↓
Execute
```

If no task file exists:

```text
Refresh Architect channel
    ↓
WAIT 80 seconds
    ↓
Check again
```

### Waiting rules

- Continue waiting while PIPELINE mode remains active  
- Do **not** permanently stop only because no task is temporarily available  
- Do **not** invent work while waiting  
- Do **not** treat Architect discussion without a file as authorization  

Optional idle cue (not a substitute for RESULT):

```text
WAITING_FOR_NEXT_ARCHITECT_TASK
```

---

## 8. Visual Evidence Review (UI / Product tasks)

Applies to Public / Admin / Agency experience tasks and any task requiring screenshots or visual checkpoints.

**Before** sending RESULT, Cursor **MUST**:

1. Verify evidence exists under:

```text
docs/product-experience/evidence/<Task-ID>/
```

2. Inspect generated screenshots (not merely confirm files were written).

3. Self-check:

| Check | Question |
|-------|----------|
| North Star | Follows `docs/product-experience/assets/travelcore-ui-ux-north-star.png` direction? |
| Product feeling | Feels like professional travel commerce? |
| Layout | Visible layout defects? |
| Responsive | Mobile / desktop acceptable? |

4. Include in RESULT: evidence paths, visual review summary, known limitations, acceptance risks.

**Forbidden:** RESULT that only says `Screenshot created` without assessment.

North Star is **directional**, not pixel-perfect cloning authorization.

Visual self-review does **not** replace Architect ACCEPT.

---

## 9. Recovery Before Execution

Before every task/gate, verify from repository:

| Check | Source |
|-------|--------|
| Current Phase | `PROJECT-STATE.md` · `TRAVELCORE-RECOVERY-CONTEXT.md` |
| Last accepted work | `PROJECT-STATE.md` · `ROADMAP.md` |
| Current authorized work | Valid `.task.md` / `.gate.md` only |
| Open blockers / locks | Recovery · ADRs · constitutions |

Also read: `docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`

On conflict with accepted SoT:

```text
STOP
Status = RECOVERY_CONFLICT
```

Do not autonomously “fix” Accepted architecture to match chat.

---

## 10. Gate Support

`.gate.md` uses the same validation / RESULT / waiting contract.

```text
READ GATE
    ↓
VALIDATE
    ↓
PERFORM REQUIRED CHECK
    ↓
RESULT
    ↓
WAITING MODE
```

A Gate does **not** break Pipeline continuity.

---

## 11. Stop Conditions

Pipeline stops **only** when:

| Condition | Action |
|-----------|--------|
| USER disables PIPELINE mode | STOP |
| Architect channel unavailable | `BLOCKED_ARCHITECT_CHANNEL_UNAVAILABLE` |
| Recovery conflict | `RECOVERY_CONFLICT` |
| Pipeline completed (explicit SoT / USER end) | STOP / COMPLETED |

**Not allowed:** stopping solely because no task file is temporarily available.

---

## 12. State Machine (acceptance)

```text
AUTHORIZED
    ↓
EXECUTING
    ↓
RESULT_READY
    ↓
ARCHITECT_REVIEW
    ↓
ACCEPTED          ← Architect only
    ↓
NEXT_TASK_ALLOWED ← next authorized file only
```

`Cursor PASS` ≠ `ACCEPTED`.

---

## 13. Removed / Invalid Prior Models

The following are **removed** and must not be followed:

| Removed model | Why |
|---------------|-----|
| Scroll chat to find `BEGIN_TASK` envelopes | Chat is not task source |
| Scan conversation history as execution authority | Truncation / false positives |
| Permanent always-on worker polling | Not V3 worker model |
| Chat text alone as executable envelope | File-based only |
| Finite “give up after N waits” as primary stop | Temporary empty queue ≠ stop |

If an older note conflicts with this V3 document, **this document wins**.

---

## 14. Failure Modes

| Failure | Response |
|---------|----------|
| No valid `.task.md` / `.gate.md` | `BLOCKED_NO_AUTHORIZED_TASK` |
| Infer work from ROADMAP | Forbidden |
| Treat Cursor PASS as ACCEPT | Keep `AWAITING_ARCHITECT_REVIEW` |
| Hunt next task during EXECUTION | Forbidden |
| RESULT without visual self-check on UI tasks | Incomplete · §8 |
| Switch architect channel | STOP / unavailable |
| Invent next task | Forbidden |

---

## 15. Relationship to Protocol Family

| Layer | Role |
|-------|------|
| ADR 0013 / 0014 | Accepted governance |
| `TRAVELCORE-PIPELINE-PROTOCOL.md` | Canonical protocol entry |
| **This Controller V3** | Mandatory Cursor PIPELINE execution contract |
| `docs/pipeline/` | Inbox / results conventions |
| Recovery docs | Durable position / emergency packet |

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial Controller Mode |
| 2026-08-21 | Worker lifecycle / file-task / visual evidence increments |
| 2026-08-21 | **V3 rewrite** — single file-based model · remove chat scroll acquisition · `TC-PIPELINE-CONTROLLER-V3-REWRITE-001` |
