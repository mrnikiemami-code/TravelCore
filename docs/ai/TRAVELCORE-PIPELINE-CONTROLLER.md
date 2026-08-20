# TravelCore Pipeline Controller Mode

| Field | Value |
|-------|--------|
| Document | `docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md` |
| Status | **ACTIVE** — mandatory for Cursor execution under PIPELINE |
| Parent protocol | [`TRAVELCORE-PIPELINE-PROTOCOL.md`](TRAVELCORE-PIPELINE-PROTOCOL.md) |
| Recovery entry | [`../prompts/START-HERE-IF-CHATGPT-IS-LOST.md`](../prompts/START-HERE-IF-CHATGPT-IS-LOST.md) |
| Fast recovery context | [`TRAVELCORE-RECOVERY-CONTEXT.md`](TRAVELCORE-RECOVERY-CONTEXT.md) |
| Inbox conventions | [`../pipeline/README.md`](../pipeline/README.md) |
| Governance | ADR 0013 · ADR 0014 |

This document defines **Cursor execution controller behavior**.

It does **not** replace the Pipeline Protocol. It extends it with a permanent control contract so Cursor:

- uses **file-based** authorized task input (`.task.md` / `.gate.md`)
- executes only validated authorized files
- sends RESULT to the authoritative Architect chat
- after RESULT, runs a **bounded waiting cycle** (not a permanent background daemon)
- recovers safely when context is lost

---

## A. Authority Model

| Role | Actor |
|------|--------|
| Architect | ChatGPT |
| Implementation Agent | Cursor |
| Human Execution Authority | USER |
| Source of Truth | **Repository recovery / SoT documents** |

### Critical rule

Chat conversation is **not** the permanent source of truth.

**Repository state is.**

Browser chat is the **architect communication channel** (RESULT delivery + waiting cue), not durable architecture authority.

---

## B. New Pipeline Model — File-Based Task Input

Pipeline uses **file-based** task input.

### Supported input types

| Type | Purpose | Example |
|------|---------|---------|
| `.task.md` | Executable implementation / docs tasks | `TC-P30-T006.task.md` |
| `.gate.md` | Checkpoints / evidence / decision requests | `TC-P30-HOTEL-VISUAL.gate.md` |
| `.result.md` | Optional repository artifact only | `TC-P30-T006.result.md` |

`.result.md` in the repository is **optional** and is **not** the primary communication channel to the Architect.

Primary RESULT delivery remains the **Architect chat** (§E).

### Simplified lifecycle

```text
Architect creates task file
        ↓
Cursor reads task file
        ↓
Cursor executes
        ↓
Cursor sends RESULT to Architect chat
        ↓
WAITING cycle (80s)
        ↓
(next authorized file / cue) → execute again
```

The worker is **NOT** a permanent background worker.

---

## C. Authorized Execution Rule

A task/gate may execute **ONLY** when a valid authorized **file** exists:

```text
*.task.md
or
*.gate.md
```

The file body must contain a complete live envelope:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
...
END_TRAVELCORE_CURSOR_TASK_V1
```

(or the gate-equivalent BEGIN/END markers defined for `.gate.md` when used)

### Required validation (minimum)

- Correct naming format (Task-ID based recommended)
- Valid BEGIN/END markers (exact, unsuffixed)
- `Task-ID` exists
- Scope exists (allow / forbid)
- Validation requirements exist
- Not already executed / replayed
- Not an example (`__EXAMPLE` / `NON_EXECUTABLE_EXAMPLE`)

### Without a valid `.task.md` / `.gate.md`

```text
STOP
Status = BLOCKED_NO_AUTHORIZED_TASK
```

### Forbidden

- inventing tasks
- reading ROADMAP as an execution command
- executing historical / completed tasks
- executing examples
- using normal chat text alone as the task source
- treating ChatGPT UI paste fragments as authorized when no file exists

Historical chat, examples, results, and quoted envelopes are **non-executable**.

---

## D. Recovery Before Execution

Before **every** task/gate, Cursor must verify from repository documents:

| Check | Source |
|-------|--------|
| Current Phase | `PROJECT-STATE.md` · `TRAVELCORE-RECOVERY-CONTEXT.md` |
| Last Accepted Gate / Task | `PROJECT-STATE.md` · `ROADMAP.md` |
| Current Authorized Task | Valid `.task.md` / `.gate.md` only |
| Next Allowed Task | SoT (do not invent) |
| Open Blockers | SoT · recovery context |
| Locked Decisions | ADRs · constitutions · product-experience locks |

Also read:

`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`

If conflict between file envelope and accepted repository architecture / recovery state:

```text
STOP
Status = RECOVERY_CONFLICT
```

Do not resolve architecture conflicts autonomously.

---

## E. Execution State Machine

No transition may be skipped.

```text
AUTHORIZED
    ↓
EXECUTING
    ↓
RESULT_READY
    ↓
ARCHITECT_REVIEW
    ↓
ACCEPTED
    ↓
NEXT_TASK_ALLOWED
```

| State | Meaning |
|-------|---------|
| `AUTHORIZED` | Valid `.task.md` / `.gate.md` · recovery checks pass |
| `EXECUTING` | Scoped implementation / gate work in progress |
| `RESULT_READY` | `TRAVELCORE_CURSOR_RESULT_V1` prepared |
| `ARCHITECT_REVIEW` | Result returned to Architect chat · awaiting architect |
| `ACCEPTED` | Architect acceptance recorded / SoT synced when required |
| `NEXT_TASK_ALLOWED` | Only after acceptance (or explicit next authorized file) |

`Cursor PASS` ≠ `ACCEPTED`.

Only architect acceptance creates acceptance state.

---

## F. Result Contract

After execution, Cursor **MUST** send the result **ONLY** to the authoritative Architect chat.

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
...
END_TRAVELCORE_CURSOR_RESULT_V1
```

### Required result content (minimum)

- Task-ID
- Status
- Summary
- Changed files
- Validation
- Commit (when applicable)
- Evidence when required
- Next-State
- HEAD / Working Tree status when applicable

For UI / Product Experience tasks (§K.1), RESULT **must also** include:

- evidence paths
- visual review summary
- known limitations
- acceptance risks

Optional: also write `docs/pipeline/results/<Task-ID>.result.md` as a repository artifact (not a substitute for Architect-chat RESULT).

Never replace the result envelope with:

- normal explanations alone
- informal summaries
- marketing-style progress notes

After a normal result:

```text
Next-State = AWAITING_ARCHITECT_REVIEW
```

Sending RESULT ends **EXECUTION MODE**. Cursor then enters **WAITING MODE** (§I) while PIPELINE remains active.

---

## G. Browser Chat Continuity

### Authoritative architect channel (current)

```text
https://chatgpt.com/g/g-p-6a79dbc6468c8191a5e74afa2d82a8be-travelcore/c/6a8039a8-2014-83ed-be9f-813280b23bcb
```

### Rules

- Do **not** switch to another ChatGPT conversation as architect command channel
- Do **not** create parallel architect chats
- Do **not** continue from another conversation’s memory
- Do **not** close / abandon / replace the protected architect tab during an active waiting cycle
- Browser chat is for **RESULT delivery** and **waiting cues** — repository files remain the executable SoT

### If the authoritative channel is unavailable

```text
STOP
Status = BLOCKED_ARCHITECT_CHANNEL_UNAVAILABLE
```

If browser context unexpectedly changes:

```text
STOP
```

Do not invent a substitute channel.

---

## H. Task Acquisition (File-First)

When PIPELINE mode is active, Cursor looks for the next authorized **file** task.

### Priority

1. Read the provided / next authorized task file (`.task.md` or `.gate.md`)
2. Validate naming, BEGIN/END, Task-ID, scope, validation, replay
3. Execute only a valid authorized task/gate

Canonical runnable locations (preferred):

- `docs/pipeline/inbox/<Task-ID>.task.md`
- `docs/pipeline/inbox/<Gate-ID>.gate.md`

Architect may also provide a downloadable task file; Cursor must persist/read it as a file before execution (chat-only fragments are insufficient).

### No execution without

```text
.task.md
or
.gate.md
```

### After valid file detection

```text
AUTHORIZED
    ↓
Recovery Before Execution (§D)
    ↓
EXECUTION MODE
```

---

## I. Pipeline Modes — EXECUTION and WAITING

### Important worker model

The worker is **NOT** always running.

| Mode | When active |
|------|-------------|
| **EXECUTION MODE** | While implementing / validating the current authorized file |
| **WAITING MODE** | **Only after** RESULT has been sent to Architect chat |

Worker becomes active for waiting **ONLY after sending RESULT**.

---

### I.1 EXECUTION MODE

When a task/gate starts, Cursor enters **EXECUTION MODE**.

During execution, worker monitoring is **disabled**.

Cursor **MUST NOT**:

- search for the next task
- refresh Architect chat to hunt for work
- run another task
- execute parallel work
- continuously poll

Only the **current** authorized file is active.

---

### I.2 After RESULT — enter WAITING MODE

Lifecycle:

```text
RESULT SENT
    ↓
WAIT 80 seconds
    ↓
READ ARCHITECT CHAT
```

If a new task file reference or authorized `.task.md` / `.gate.md` exists:

```text
Return to TASK ACQUISITION (§H)
```

If no task exists:

```text
Refresh the same Architect chat page
    ↓
WAIT 80 seconds
    ↓
Read again
```

Repeat this cycle.

**Never invent work** during waiting.

Optional idle cue (not a substitute for RESULT):

```text
WAITING_FOR_NEXT_ARCHITECT_TASK
```

---

### I.3 Worker Lifecycle Rule

The waiting worker exists only for:

1. Waiting for the next Architect instruction / file cue
2. Detecting the next authorized `.task.md` / `.gate.md`

The worker **MUST NOT**:

- continuously poll as a permanent daemon outside the post-RESULT cycle
- continuously scan during EXECUTION MODE
- run waiting loops during execution
- create tasks
- decide next product/architecture steps

---

### I.4 Continuous Pipeline (file-based)

```text
TASK FILE
    ↓
CURSOR EXECUTION
    ↓
RESULT TO ARCHITECT CHAT
    ↓
WAIT 80 seconds
    ↓
CHECK ARCHITECT / FILES
    ↓
NEW TASK FILE
    ↓
EXECUTE AGAIN
```

The cycle continues until:

| Stop condition | Action |
|----------------|--------|
| USER disables PIPELINE mode | STOP |
| Architect channel unavailable | `BLOCKED_ARCHITECT_CHANNEL_UNAVAILABLE` |
| Recovery conflict | `RECOVERY_CONFLICT` |
| Pipeline completed (explicit SoT / USER end) | STOP / COMPLETED |

---

## J. Gate Support

Pipeline supports:

| File | Role |
|------|------|
| `.task.md` | Execute implementation / docs work |
| `.gate.md` | Collect evidence / perform checkpoint / request decision |

- **Task** = implementation work under scope
- **Gate** = checkpoint / evidence / decision support

A Gate does **not** break Pipeline.

After Gate RESULT:

```text
Return to WAITING MODE cycle (§I.2)
```

---

## K. Visual / Product Experience Protection

For UI / Product Experience tasks (including P30):

Before execution verify the approved North Star exists:

```text
docs/product-experience/assets/travelcore-ui-ux-north-star.png
```

Rules:

- Visual implementation must be evaluated against the approved North Star
- The North Star is **directional**, not pixel-perfect authorization to clone or fake data
- Visual tasks require screenshot / visual evidence before architect acceptance
- Automated tests alone are **not sufficient** for major visual surfaces

Related constitution (when present):

- `docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md`
- `docs/product-experience/P30-VISUAL-ACCEPTANCE-CHECKLIST.md`

### K.1 Visual Evidence Review (mandatory before RESULT)

Applies to:

- Public Experience tasks
- Admin Experience tasks
- Agency Experience tasks
- Any task requiring screenshots or visual checkpoints

**Before** sending `BEGIN_TRAVELCORE_CURSOR_RESULT_V1`, Cursor **MUST**:

1. **Verify evidence files exist** under:

```text
docs/product-experience/evidence/<Task-ID>/
```

2. **Open and inspect** the generated screenshots (not merely confirm file creation).

3. **Perform visual self-check** against at least:

| Check | Question |
|-------|----------|
| North Star | Does implementation follow North Star direction? |
| Product feeling | Does it feel like a professional travel commerce product? |
| Layout defects | Are there visible layout defects? |
| Responsive | Are mobile and desktop states acceptable? |

4. **Include visual review in RESULT** — required fields:

- evidence paths
- visual review summary
- known limitations
- acceptance risks

**Forbidden RESULT pattern:**

```text
Screenshot created
```

alone, without visual assessment.

This rule does **not** replace Architect ACCEPT.

`Cursor PASS` still means **AWAITING_ARCHITECT_REVIEW**.

---

## L. Failure Modes This Controller Prevents

| Failure mode | Required response |
|--------------|-------------------|
| Inferring work from ROADMAP / deferred items | `BLOCKED_NO_AUTHORIZED_TASK` |
| Executing from chat text without `.task.md` / `.gate.md` | `BLOCKED_NO_AUTHORIZED_TASK` |
| Continuing after chat/tab loss without recovery | Recovery Packet · default HUMAN |
| Switching to another architect chat | `BLOCKED_ARCHITECT_CHANNEL_UNAVAILABLE` / STOP |
| Treating Cursor PASS as ACCEPT | Keep `AWAITING_ARCHITECT_REVIEW` |
| Executing product work from governance tasks | Scope violation · STOP |
| Skipping visual evidence for major UI | Visual acceptance protocol |
| Claiming \"Screenshot created\" without visual self-check | Forbidden (§K.1) · incomplete RESULT |
| Searching for next task during EXECUTION MODE | Forbidden (§I.1) |
| Permanent always-on polling daemon | Forbidden (§I.3) |
| Inventing next task while waiting | Forbidden |

---

## M. Core Rules (mandatory)

- **No Envelope = No Execution** (file envelope required)
- **Cursor PASS ≠ Architect ACCEPT**
- **Repository remains Source of Truth**
- Never invent next task
- Never bypass Architect approval

---

## N. Relationship to Existing Protocol

| Layer | Role |
|-------|------|
| ADR 0013 / 0014 | Accepted governance |
| `TRAVELCORE-PIPELINE-PROTOCOL.md` | Canonical protocol entry |
| **This Controller** | Mandatory Cursor execution control contract (file-task mode) |
| `docs/pipeline/` | Inbox / results conventions |
| `TRAVELCORE-RECOVERY-CONTEXT.md` | Fast durable position snapshot |
| `START-HERE-IF-CHATGPT-IS-LOST.md` | Emergency recovery packet generation |

Pipeline Controller Mode is **mandatory** for Cursor execution under PIPELINE.

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial Controller Mode · `TC-PIPELINE-CONTROLLER-MODE-001` |
| 2026-08-21 | Worker lifecycle · EXECUTION / WAITING · `TC-PIPELINE-CONTROLLER-WORKER-LOOP-REVISION-001` |
| 2026-08-21 | File-based task mode · `.task.md` / `.gate.md` · waiting only after RESULT · `TC-PIPELINE-CONTROLLER-FILE-TASK-MODE-REVISION-001` |
| 2026-08-21 | Visual Evidence Review before RESULT · `TC-PIPELINE-VISUAL-EVIDENCE-REVIEW-RULE-001` |
