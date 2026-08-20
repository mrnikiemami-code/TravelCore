# TravelCore Pipeline Controller Mode

| Field | Value |
|-------|--------|
| Document | `docs/ai/TRAVELCORE-PIPELINE-CONTROLLER.md` |
| Status | **ACTIVE** — mandatory for Cursor execution under PIPELINE |
| Parent protocol | [`TRAVELCORE-PIPELINE-PROTOCOL.md`](TRAVELCORE-PIPELINE-PROTOCOL.md) |
| Recovery entry | [`../prompts/START-HERE-IF-CHATGPT-IS-LOST.md`](../prompts/START-HERE-IF-CHATGPT-IS-LOST.md) |
| Fast recovery context | [`TRAVELCORE-RECOVERY-CONTEXT.md`](TRAVELCORE-RECOVERY-CONTEXT.md) |
| Governance | ADR 0013 · ADR 0014 |

This document defines **Cursor execution controller behavior**.

It does **not** replace the Pipeline Protocol. It extends it with a permanent control contract so Cursor:

- operates from the authoritative architect channel
- preserves execution continuity
- follows BEGIN / RESULT envelopes
- executes only authorized tasks
- recovers safely when context is lost
- under PIPELINE, runs as a controlled **worker** with EXECUTION MODE and WAITING MODE (§G)

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

Browser chat is the **architect communication channel** (transport), not durable architecture authority.

---

## B. Authorized Execution Rule

A task may execute **ONLY** when a valid live envelope exists:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
...
END_TRAVELCORE_CURSOR_TASK_V1
```

### Required fields (minimum)

- `Task-ID`
- `Phase`
- Scope (explicit allow / forbid)
- Validation requirements

### Without a valid envelope

```text
STOP
Status = BLOCKED_NO_AUTHORIZED_TASK
```

### Forbidden without authorization

- infer next task
- execute roadmap items because they exist
- execute deferred work
- continue previous unfinished ideas
- expand scope
- treat Cursor PASS as Architect ACCEPT

Historical chat, examples, results, and quoted envelopes are **non-executable**.

---

## C. Recovery Before Execution

Before **every** task, Cursor must verify from repository documents:

| Check | Source |
|-------|--------|
| Current Phase | `PROJECT-STATE.md` · `TRAVELCORE-RECOVERY-CONTEXT.md` |
| Last Accepted Gate / Task | `PROJECT-STATE.md` · `ROADMAP.md` |
| Current Authorized Task | Valid envelope only |
| Next Allowed Task | SoT (do not invent) |
| Open Blockers | SoT · recovery context |
| Locked Decisions | ADRs · constitutions · product-experience locks |

Also read:

`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`

If conflict between envelope and accepted repository architecture / recovery state:

```text
STOP
Status = RECOVERY_CONFLICT
```

Do not resolve architecture conflicts autonomously.

---

## D. Execution State Machine

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
| `AUTHORIZED` | Valid envelope present · recovery checks pass |
| `EXECUTING` | Scoped implementation in progress |
| `RESULT_READY` | `TRAVELCORE_CURSOR_RESULT_V1` prepared |
| `ARCHITECT_REVIEW` | Result returned · awaiting architect |
| `ACCEPTED` | Architect acceptance recorded / SoT synced when required |
| `NEXT_TASK_ALLOWED` | Only after acceptance (or explicit authorized next envelope) |

`Cursor PASS` ≠ `ACCEPTED`.

Only architect acceptance creates acceptance state.

---

## E. Result Contract

Every completion **MUST** return:

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
...
END_TRAVELCORE_CURSOR_RESULT_V1
```

### Required result content (minimum)

- Task-ID
- Phase
- Status
- Commit (when applicable)
- Validation evidence
- Changed scope
- Recovery / ledger state

The result MUST be returned to the **same** authoritative architect channel.

Never replace the result envelope with:

- normal explanations alone
- informal summaries
- marketing-style progress notes

After a normal result:

```text
Next-State = AWAITING_ARCHITECT_REVIEW
```

Sending RESULT ends **EXECUTION MODE** only. While PIPELINE mode remains active, the worker enters **WAITING MODE** (§G) — it does **not** terminate the worker session.

---

## F. Browser Chat Continuity

### Authoritative architect channel (current)

```text
https://chatgpt.com/g/g-p-6a79dbc6468c8191a5e74afa2d82a8be-travelcore/c/6a8039a8-2014-83ed-be9f-813280b23bcb
```

### Rules

- Do **not** switch to another ChatGPT conversation as architect command channel
- Do **not** create parallel architect chats
- Do **not** continue from another conversation’s memory
- Do **not** close / abandon / replace the protected architect tab during an active PIPELINE cycle
- Browser chat is **communication only** — repository SoT remains authoritative

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

## G. Pipeline Worker Lifecycle

When USER activates **PIPELINE** mode, Cursor behaves as a **controlled worker**.

### Worker goal

1. Find an authorized task from the Architect channel  
2. Execute **only** that task  
3. Return RESULT to the Architect channel  
4. End EXECUTION MODE  
5. Enter WAITING MODE for the next Architect task  
6. Continue the cycle until PIPELINE is disabled or blocked  

### Operating modes

| Mode | Meaning |
|------|---------|
| **EXECUTION MODE** | One authorized task is being acquired / recovered / implemented / RESULT-delivered |
| **WAITING MODE** | RESULT already sent · session stays alive · wait/poll for next valid envelope |

```text
PIPELINE active
      ↓
EXECUTION MODE  (acquire → recover → execute → RESULT)
      ↓
WAITING MODE    (wait 80s · read · acquire · refresh · repeat)
      ↓
  (new valid task) → EXECUTION MODE
  (stop condition) → STOP
```

---

### G.1 EXECUTION MODE — Task Acquisition

Worker starts with:

```text
STATE = READ_ARCHITECT_CHANNEL
```

#### Procedure

1. Open the authoritative architect channel (§F).  
2. **Scroll to the bottom** of the conversation first (newest state).  
3. Confirm the newest conversation state.  
4. Move **upward** through conversation history.  
5. Search **only** for a complete executable envelope:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
...
END_TRAVELCORE_CURSOR_TASK_V1
```

#### Executable only when all hold

- BEGIN marker exists (exact, unsuffixed)  
- END marker exists (exact, unsuffixed)  
- Required fields exist (`Task-ID`, `Phase`, scope allow/forbid, validation)  
- `Task-ID` exists  
- `Task-ID` is new (not already executed / PASS-replayed)  
- Task is **not** inside a RESULT envelope  
- Task is **not** a quoted example (`__EXAMPLE` / `NON_EXECUTABLE_EXAMPLE`)  
- Task is **not** historical / already completed  

#### Ignore

- incomplete TASK fragments  
- RESULT messages / RESULT envelopes  
- normal explanations  
- examples  
- old completed tasks  
- architectural discussion **without** a complete envelope  

#### After valid task detection

```text
AUTHORIZED
    ↓
Recovery Before Execution (§C)
    ↓
EXECUTE
```

---

### G.2 During Execution — monitoring disabled

While EXECUTION MODE is active for a claimed task, **worker waiting/acquisition is disabled**.

The worker **MUST NOT**:

- scan architect chat for another task  
- refresh the browser to hunt for work  
- execute parallel tasks  
- start another worker cycle  

Only the **current authorized task** is active.

---

### G.3 After Execution — RESULT then leave EXECUTION MODE

When implementation finishes, the worker **MUST** create and send to the authoritative architect channel:

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
...
END_TRAVELCORE_CURSOR_RESULT_V1
```

After RESULT delivery:

- EXECUTION MODE ends  
- Worker enters **WAITING MODE**  
- Worker session does **not** terminate solely because RESULT was sent  

---

### G.4 WAITING MODE — Continuous Waiting Loop

After RESULT delivery (PIPELINE still active):

The worker does **NOT** terminate.  
The worker waits for the next Architect instruction.

#### Procedure

1. **Wait 80 seconds.**  
2. Read the **same** authoritative architect channel.  
3. Run Task Acquisition (§G.1): bottom → upward → newest complete valid unexecuted envelope.  
4. If found → return to **EXECUTION MODE** (Recovery → EXECUTE).  
5. If not found → **Refresh** the same architect channel.  
6. **Wait another 80 seconds.**  
7. Repeat the same acquisition procedure.  

**Never invent work** during waiting.

Optional status line (may be sent periodically when idle for clarity; not a substitute for RESULT):

```text
WAITING_FOR_NEXT_ARCHITECT_TASK
```

---

### G.5 Continuous Pipeline Rule

Sending RESULT does **NOT** mean the worker session is finished.

The worker session remains active while **PIPELINE mode is active**.

The worker **stops only when**:

| Stop condition | Status / action |
|----------------|-----------------|
| USER disables PIPELINE mode (`TRAVELCORE_MODE: HUMAN` or equivalent) | STOP |
| Architect channel unavailable | `BLOCKED_ARCHITECT_CHANNEL_UNAVAILABLE` |
| Recovery conflict | `RECOVERY_CONFLICT` |
| Pipeline completed (explicit SoT / USER end) | STOP / COMPLETED |

There is **no** automatic “give up after N waits” stop while PIPELINE remains active and no stop condition above applies.

---

### G.6 Safety Rules (unchanged, mandatory)

- **No Envelope = No Execution**  
- **Cursor PASS ≠ Architect ACCEPT**  
- **Repository remains Source of Truth**  
- Never invent the next task  
- Never bypass Architect approval  

---

## H. Task Acquisition Loop (summary)

```text
STATE = READ_ARCHITECT_CHANNEL
```

Normative detail: §G.1.

Summary:

1. Bottom of architect channel first (newest)  
2. Move upward  
3. Newest complete valid unexecuted `TRAVELCORE_CURSOR_TASK_V1` only  
4. If none → WAITING MODE rules (§G.4)  
5. If found → Recovery → execute only that task → RESULT → WAITING MODE  

---

## I. Visual / Product Experience Protection

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

---

## J. Failure Modes This Controller Prevents

| Failure mode | Required response |
|--------------|-------------------|
| Inferring work from ROADMAP / deferred items | `BLOCKED_NO_AUTHORIZED_TASK` |
| Continuing after chat/tab loss without recovery | Recovery Packet · default HUMAN |
| Switching to another architect chat | `BLOCKED_ARCHITECT_CHANNEL_UNAVAILABLE` / STOP |
| Treating Cursor PASS as ACCEPT | Keep `AWAITING_ARCHITECT_REVIEW` |
| Executing product work from governance tasks | Scope violation · STOP |
| Skipping visual evidence for major UI | Visual acceptance protocol |
| Terminating worker solely because RESULT was sent | Remain in WAITING MODE while PIPELINE active (§G.5) |
| Scanning architect chat during EXECUTION MODE | Forbidden (§G.2) |
| Inventing next task while waiting | Forbidden · wait for Architect envelope |

---

## K. Relationship to Existing Protocol

| Layer | Role |
|-------|------|
| ADR 0013 / 0014 | Accepted governance |
| `TRAVELCORE-PIPELINE-PROTOCOL.md` | Canonical protocol entry |
| **This Controller** | Mandatory Cursor execution control contract |
| `TRAVELCORE-RECOVERY-CONTEXT.md` | Fast durable position snapshot |
| `START-HERE-IF-CHATGPT-IS-LOST.md` | Emergency recovery packet generation |

Pipeline Controller Mode is **mandatory** for Cursor execution under PIPELINE.

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial Controller Mode · `TC-PIPELINE-CONTROLLER-MODE-001` |
| 2026-08-21 | Worker lifecycle · EXECUTION / WAITING modes · continuous 80s wait · acquisition bottom→up · `TC-PIPELINE-CONTROLLER-WORKER-LOOP-REVISION-001` |
