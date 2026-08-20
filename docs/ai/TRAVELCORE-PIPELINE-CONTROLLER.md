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

Then **STOP** (except for the post-result monitoring loop below while PIPELINE is active and USER-authorized).

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

## G. Post Result Monitoring Loop

After sending a RESULT to the authoritative channel (PIPELINE mode active):

1. **WAIT 100 seconds**
2. Read the same architect channel again
3. If a new valid `BEGIN_TRAVELCORE_CURSOR_TASK_V1` exists → enter `AUTHORIZED` / execute
4. If no task → **Refresh the same chat page**
5. **WAIT 100 seconds** (allow complete reload)
6. Read again
7. If task exists → execute
8. If no task → send:

```text
WAITING_FOR_NEXT_ARCHITECT_TASK
```

9. **WAIT 70 seconds**
10. Read again
11. If task exists → execute
12. If no task → Refresh the same page
13. **WAIT 80 seconds**
14. Read again
15. If still no task:

```text
STOP
Status = WAITING_ARCHITECT_INPUT
```

**Never invent work** during waiting.

---

## H. Task Acquisition Loop

```text
STATE = READ_ARCHITECT_CHAT
```

Actions:

1. Read newest messages from the authoritative architect channel
2. Find newest valid unexecuted `BEGIN_TRAVELCORE_CURSOR_TASK_V1`
3. If none → waiting / monitoring rules above
4. If found → Recovery Before Execution → execute only that task → RESULT → monitoring loop

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
