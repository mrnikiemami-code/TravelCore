# TravelCore Pipeline Session Runner — Specification

| Field | Value |
|-------|--------|
| Document | `docs/ai/TRAVELCORE-PIPELINE-SESSION-RUNNER-SPEC.md` |
| Status | **SPECIFICATION** — not implemented |
| Task-ID | `TC-PIPELINE-SESSION-RUNNER-SPEC-001` |
| Phase | Post-P30 — Pipeline Automation |
| Parent | [`TRAVELCORE-PIPELINE-PROTOCOL.md`](./TRAVELCORE-PIPELINE-PROTOCOL.md) · [`TRAVELCORE-PIPELINE-CONTROLLER.md`](./TRAVELCORE-PIPELINE-CONTROLLER.md) |
| Inbox/Results | [`../pipeline/README.md`](../pipeline/README.md) |
| Prior research | `TC-PIPELINE-SESSION-RUNNER-RESEARCH-001` · `TC-PIPELINE-AUTOMATION-V2-TRIGGER-CONNECT-RESEARCH-001` |

This document defines the **smallest reliable architecture** for a continuous TravelCore pipeline loop.

It does **not** authorize implementation. Implementation requires a separate authorized envelope.

---

## 0. Goal

Enable:

```text
START PIPELINE
        ↓
Pipeline Session Runner
        ↓
Detect authorized task
        ↓
Execute through Cursor
        ↓
Receive RESULT
        ↓
Wait for Architect ACCEPT
        ↓
Continue with next authorized task
        ↓
Stop only at required human gates
```

**Hard honesty:**

- Cursor does **not** natively own this session/gate loop.
- Runner is a **thin orchestrator**; Cursor is the **executor**.
- Architect creates the next authorized task — Runner never invents it.
- **Cursor PASS ≠ Architect ACCEPT.**
- **Repository is Source of Truth.**

---

## 1. Pipeline Session concept

A **Pipeline Session** is a single long-lived orchestration context started by the USER (`START PIPELINE`) that:

1. Watches for authorized work in the repository inbox.
2. Dispatches at most **one** executable task at a time to Cursor.
3. Collects the Cursor RESULT artifact from the repository.
4. **Stops** until Architect ACCEPT (or an explicit gate resolution) is recorded in SoT.
5. Then waits for the **next** authorized inbox task (created by Architect, not by Runner).
6. Ends only on `COMPLETED`, `BLOCKED` (unresolved), or USER stop.

### Session properties

| Property | Rule |
|----------|------|
| Cardinality | One active session per repository (MVP) |
| Task concurrency | Exactly one task executing at a time |
| Authority to invent work | **None** |
| Durable SoT | Repository (`docs/pipeline/**`, recovery, PROJECT-STATE) |
| Ephemeral runtime | Runner process memory (must be recoverable from repo) |

### Non-session

- A single chat paste without session start
- Cloud Agent conversation alone (executor substrate, not session owner)
- Automation Memories (soft notes, not protocol ledger)

---

## 2. State machine

### Required states

| State | Meaning |
|-------|---------|
| `IDLE` | Session exists or process ready; no work claimed |
| `RUNNING` | Session active; scanning / coordinating |
| `TASK_DETECTED` | Valid unexecuted inbox envelope found |
| `EXECUTING` | Executor (Cursor) invoked for current Task-ID |
| `WAITING_RESULT` | Executor running or RESULT not yet in repo |
| `WAITING_ARCHITECT_ACCEPT` | Cursor RESULT present; Architect decision pending |
| `CONTINUE` | ACCEPT recorded; ready to scan for next authorized task |
| `BLOCKED` | Cannot proceed without human/Architect resolution |
| `COMPLETED` | Session ended (USER stop, phase complete signal, or explicit end) |

### Transitions (normative)

```text
IDLE
  --(START PIPELINE)--> RUNNING

RUNNING
  --(no valid inbox task)--> IDLE | RUNNING (poll; no invent)
  --(valid task found)--> TASK_DETECTED

TASK_DETECTED
  --(replay / invalid / SoT conflict)--> BLOCKED
  --(claim OK)--> EXECUTING

EXECUTING
  --(executor started)--> WAITING_RESULT
  --(start failure)--> BLOCKED

WAITING_RESULT
  --(RESULT PASS|FAIL written)--> WAITING_ARCHITECT_ACCEPT
  --(timeout / missing RESULT)--> BLOCKED
  --(executor error)--> BLOCKED

WAITING_ARCHITECT_ACCEPT
  --(Architect ACCEPT + AUTO_CONTINUE eligible)--> CONTINUE
  --(Architect REWORK / REJECT with new authorized task)--> CONTINUE
    (only after new envelope appears; do not invent)
  --(Architect BLOCK / unresolved gate)--> BLOCKED
  --(timeout waiting ACCEPT)--> BLOCKED (or remain waiting per MVP policy)

CONTINUE
  --(scan)--> RUNNING

BLOCKED
  --(human/Architect resolution + valid next envelope or clear)--> RUNNING | IDLE
  --(USER abort)--> COMPLETED

RUNNING | IDLE
  --(USER STOP / explicit session end)--> COMPLETED
```

### Invariants

1. Never transition from `WAITING_RESULT` to `CONTINUE` without Architect ACCEPT.
2. Never transition to `EXECUTING` without a complete valid envelope.
3. `CONTINUE` does not create tasks; it only re-enters `RUNNING` scan.
4. `Cursor PASS` alone must land in `WAITING_ARCHITECT_ACCEPT`, never `CONTINUE`.

---

## 3. Runner responsibilities

### Runner MAY

| Action | Notes |
|--------|-------|
| Watch inbox | `docs/pipeline/inbox/*.task.md` |
| Validate envelopes | Markers, Protocol-Version, completeness, Auto-Execute, non-example |
| Prevent replay | Block if RESULT already PASS/ACCEPT for Task-ID |
| Trigger executor | Cursor Automation webhook, Cloud Agents API, or local `agent -p` (one chosen in impl) |
| Collect results | Read `docs/pipeline/results/<Task-ID>.result.md` |
| Maintain session state | Under `docs/pipeline/session/` (see §6) |
| Claim inbox items | `*.task.md` → `*.executing.md` lifecycle |
| Emit BLOCKED reasons | Structured, durable |

### Runner MUST NOT

| Forbidden | Reason |
|-----------|--------|
| Invent tasks | Architect-only |
| Change roadmap / product direction | Architect + SoT |
| Bypass Architect approval | PASS ≠ ACCEPT |
| Modify product / FE / BE unless task allows | Scope |
| Scrape ChatGPT UI as primary transport | Fragile; out of MVP |
| Run multiple tasks concurrently | Session invariant |
| Treat Automation Memories as ACCEPT | Soft only |
| Expand deferred work (e.g. DEMOFEED) without envelope | Protocol |

---

## 4. Executor boundary (authority model)

| Role | Authority | Actor |
|------|-----------|--------|
| **Architect** | Decision authority — authorize tasks, ACCEPT/REJECT/REWORK, product/architecture gates | ChatGPT (architect channel) |
| **Runner** | Orchestration authority — detect, sequence, gate, recover session | Thin Pipeline Session Runner |
| **Cursor** | Implementation executor — execute one authorized envelope, write RESULT | Cloud Agent / Automation / IDE Agent |
| **Repository** | Source of Truth — envelopes, RESULT, ACCEPT records, recovery, ADRs | Git (`TravelCore`) |
| **USER** | Execution authority — START/STOP PIPELINE | Human |

### Flow of authority

```text
Architect writes authorized envelope → repo inbox
        ↓
Runner detects + validates + claims
        ↓
Cursor executes exactly that envelope
        ↓
Cursor writes RESULT → repo results
        ↓
Architect ACCEPT (or gate) → repo SoT signal
        ↓
Runner CONTINUE → wait for next Architect envelope
```

Chat is transport for Architect decisions until persisted; **git wins** on conflict.

---

## 5. Gate model

### AUTO_CONTINUE

Automatic progression **from `CONTINUE` → next scan** is allowed only when **all** hold:

1. Cursor RESULT exists for current Task-ID.
2. Architect ACCEPT (or explicit `AUTO_CONTINUE` grant) is recorded in SoT for that Task-ID.
3. No open `BLOCKED` reason remains.
4. Task did not declare a hard stop / human product gate without resolution.
5. A **new** authorized inbox task exists **or** session simply returns to idle scan (never invent).

> Note: AUTO_CONTINUE means “Runner may look for the next authorized task.”  
> It does **not** mean “Runner may create the next task.”

### ARCHITECT_REVIEW_REQUIRED

Enter / remain in `WAITING_ARCHITECT_ACCEPT` when:

- Cursor RESULT `Status: PASS` or `FAIL` is written
- Task envelope requested architect review / Stop-After-Result
- Visual / product / architecture acceptance needed
- Default for all pipeline tasks unless Architect explicitly waived (rare; must be in envelope)

### BLOCKED

Enter `BLOCKED` when:

- Invalid / truncated envelope
- Replay detected
- Source-of-truth conflict
- Executor failure / missing RESULT / timeout
- Architect explicit block
- Unresolved human confirmation gate
- DEMOFEED or forbidden scope without authorization

Runner stops dispatching until resolution is recorded and USER/Architect clear the block.

---

## 6. Session state storage

### Recommendation

| Item | Path |
|------|------|
| Session directory | `docs/pipeline/session/` |
| Active session file | `docs/pipeline/session/CURRENT.md` (or `CURRENT.json`) |
| Optional history | `docs/pipeline/session/history/<Session-ID>.md` |

### Minimal `CURRENT` fields

```text
Session-ID: <uuid-or-timestamp>
Status: IDLE|RUNNING|TASK_DETECTED|EXECUTING|WAITING_RESULT|WAITING_ARCHITECT_ACCEPT|CONTINUE|BLOCKED|COMPLETED
Current-Task-ID: <Task-ID or empty>
Last-Result-Path: <path or empty>
Last-Accept-Task-ID: <Task-ID or empty>
Blocked-Reason: <text or empty>
Updated-At: <ISO-8601>
Executor-Mode: automation-webhook|cloud-agents-api|local-cli
Head-At-Update: <git sha>
```

### Format choice (MVP)

- Prefer **Markdown** for human readability and SoT alignment (`CURRENT.md`).
- JSON allowed later if tooling needs it; not required for V1.

### Persistence rules

1. Update `CURRENT` on every state transition.
2. Commit session state with or immediately after orchestration events (impl detail).
3. On Runner crash: recover from `CURRENT` + inbox + results — do not trust process memory alone.
4. Session state is **orchestration SoT**, not product architecture SoT.

---

## 7. Error handling

| Condition | Runner behavior |
|-----------|-----------------|
| **Invalid envelope** | Do not execute → `BLOCKED` or skip with durable note; never partial-run |
| **Duplicate / replay Task-ID** | `REPLAY_BLOCKED` → do not execute; clear/archive inbox signal |
| **Failed execution** (Cursor FAIL / crash) | Ensure RESULT or write Runner failure note → `WAITING_ARCHITECT_ACCEPT` or `BLOCKED` |
| **Missing RESULT** | Wait until timeout → `BLOCKED` (`MISSING_RESULT`) |
| **Timeout** | Configurable; MVP default: mark `BLOCKED` with reason; no auto-retry storm |
| **Repository conflict / SoT conflict** | `SOURCE_OF_TRUTH_CONFLICT` → `BLOCKED`; do not “fix” Accepted ADRs |
| **Executor busy** (`409 agent_busy`) | Wait/retry with backoff once; then `BLOCKED` if persistent |
| **Self-trigger Automation loop** | Prefer path-filtered webhook or prompt no-op; Runner must not re-claim completed Task-ID |

### Invalid envelope examples

- Missing BEGIN/END markers
- `__EXAMPLE` / `NON_EXECUTABLE_EXAMPLE`
- Truncated body
- Missing Task-ID
- Chat-only quote without inbox (unless live execute cycle explicitly cites — session MVP prefers **inbox only**)

---

## 8. Recovery compatibility

Runner **must** preserve:

| Rule | Enforcement |
|------|-------------|
| No Envelope = No Execution | No dispatch without valid inbox task |
| Cursor PASS ≠ Architect ACCEPT | RESULT → `WAITING_ARCHITECT_ACCEPT` only |
| Repository is Source of Truth | Read recovery / PROJECT-STATE / ADRs before execute |
| Controller checks | Align with `TRAVELCORE-PIPELINE-CONTROLLER.md` |
| Do not invent next task | `CONTINUE` only scans |
| Replay protection | Per Pipeline V2 decision record |
| After RESULT wait | No silent next product task |

On session restart: read `docs/pipeline/session/CURRENT.md`, inbox, results, recovery context — resume state machine without re-executing accepted/PASS-replay tasks.

---

## 9. Security boundaries

| Actor | May | Must not |
|-------|-----|----------|
| **Runner** | Read inbox/results/session; validate; claim; call configured executor; write session state; archive completed inbox items | Hold product secrets beyond executor config; invent envelopes; push product commits itself (executor does code writes); bypass gates |
| **Cursor (executor)** | Execute **one** authorized task scope; write RESULT + allowed artifacts | Expand scope; invent next Task-ID; treat PASS as ACCEPT; scrape Architect chat as authority |
| **Architect** | Authorize envelopes; ACCEPT/REJECT/REWORK; set gates; update product direction via SoT tasks | Be replaced by Runner automation |
| **USER** | START/STOP session; resolve operational blocks | Be required for every AUTO_CONTINUE after ACCEPT (optional presence) |

### Secrets

- Automation webhook API keys / Cursor API keys live outside git (env / Cursor dashboard).
- Never commit secrets into `docs/pipeline/session/`.

### Trust

- Inbox content is **untrusted input** until validated.
- Prefer Automation Memories **off** for inbox-driven runs (poisoning risk).

---

## 10. MVP implementation boundary (V1)

### V1 includes

1. Spec (this document) — **done when ACCEPTED**
2. Session state files under `docs/pipeline/session/`
3. Thin Runner process or script that:
   - START/STOP
   - Poll or watch `docs/pipeline/inbox/*.task.md`
   - Validate + replay-check
   - Claim lifecycle
   - Trigger **one** executor mode (prefer: Automation webhook **or** local authorized agent; pick in implement task)
   - Wait for `docs/pipeline/results/<Task-ID>.result.md`
   - Transition to `WAITING_ARCHITECT_ACCEPT`
   - Resume only after ACCEPT signal in SoT
4. ACCEPT signal MVP (choose one, document in implement task):
   - `docs/pipeline/results/<Task-ID>.accept.md`, **or**
   - Architect-updated line in `docs/pipeline/session/CURRENT.md`, **or**
   - Explicit ACCEPT task envelope
5. Structured BLOCKED reasons
6. Compatibility with existing Pipeline V2 inbox/results

### V1 excludes

| Excluded | Why |
|----------|-----|
| Large orchestrator / microservices | Overkill |
| Dashboard UI | Out of scope |
| Database / message broker | Repo SoT sufficient |
| ChatGPT browser scraping | Fragile |
| Multi-repo sessions | Not yet |
| Automatic Architect task generation | Violates protocol |
| Product / FE / BE changes | Forbidden unless later envelope |
| DEMOFEED execution | Unless explicit envelope |
| Full Cloud Agents platform rewrite | Unnecessary |
| Concurrent multi-task fan-out | Invariant |

### Implementation order (future authorized tasks)

1. `…-RUNNER-SPEC` ACCEPT (this doc)
2. Session directory + ACCEPT convention docs
3. Runner MVP (watch → execute → wait ACCEPT → loop)
4. Optional: wire Cursor Automation webhook as executor trigger
5. Harden timeouts / idempotency

---

## 11. Relation to existing artifacts

| Artifact | Role vs Runner |
|----------|----------------|
| `TRAVELCORE-PIPELINE-PROTOCOL.md` | Parent protocol — Runner obeys |
| `TRAVELCORE-PIPELINE-CONTROLLER.md` | Cursor execution contract — Runner invokes Cursor under it |
| `TRAVELCORE-RECOVERY-CONTEXT.md` | Fast recovery SoT — Runner reads before execute |
| `docs/pipeline/inbox/` | Authorized runnable queue |
| `docs/pipeline/results/` | Cursor RESULT store |
| Cursor Automation POC | Optional executor trigger — not session owner |

---

## 12. Acceptance criteria for this specification

Architect may ACCEPT this SPEC when it clearly defines:

1. Session concept  
2. State machine (all required states)  
3. Runner MAY / MUST NOT  
4. Authority boundaries  
5. Gate model (AUTO_CONTINUE / ARCHITECT_REVIEW_REQUIRED / BLOCKED)  
6. Session storage recommendation  
7. Error handling  
8. Recovery compatibility  
9. Security boundaries  
10. V1 include/exclude  

**Implementation is out of scope for `TC-PIPELINE-SESSION-RUNNER-SPEC-001`.**

---

## 13. Non-executable summary

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1__EXAMPLE
Task-ID: TC-PIPELINE-SESSION-RUNNER-SPEC-001
Note: illustrative only — live RESULT is produced by the execute cycle, not this section
END_TRAVELCORE_CURSOR_RESULT_V1__EXAMPLE
```
