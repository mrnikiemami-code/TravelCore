# Execution State Machine

Companion to:

- [`TRAVELCORE-PIPELINE-PROTOCOL.md`](TRAVELCORE-PIPELINE-PROTOCOL.md) (canonical entry)
- [`01-chatgpt-cursor-handoff-protocol.md`](01-chatgpt-cursor-handoff-protocol.md)
- [`03-human-confirmation-gates.md`](03-human-confirmation-gates.md)
- [`../architecture/16-agent-handoff-and-phase-gates.md`](../architecture/16-agent-handoff-and-phase-gates.md)

---

## 1. Per-Task Cycle States

```text
IDLE
  ↓
PRECHECK
  ↓
EXECUTING
  ↓
VERIFYING
  ↓
REPORTING
  ↓
AWAITING_ARCHITECT_REVIEW
  ↓
IDLE
```

| State | Owner | Meaning |
|-------|-------|---------|
| `IDLE` | Pipeline | Waiting for a new explicit task envelope |
| `PRECHECK` | Cursor | Remote/branch/cleanliness/deps/replay/gates |
| `EXECUTING` | Cursor | Scoped write/read work for **one** Task-ID |
| `VERIFYING` | Cursor | Applicable quality gates + diff review |
| `REPORTING` | Cursor | Emit `TRAVELCORE_CURSOR_RESULT_V1` |
| `AWAITING_ARCHITECT_REVIEW` | ChatGPT | Architect acceptance barrier |

Cursor must **STOP** after `REPORTING`. It does not self-transition into the next Task.

---

## 2. Phase Boundary States

```text
PHASE_CLOSING
  ↓
READY_AWAITING_HUMAN_CONFIRMATION
  ↓
HUMAN CONFIRMATION (USER token)
  ↓
NEXT PHASE MAY START
```

Closing Phase `Pn` does **not** start `Pn+1`.

Required USER token form:

```text
TRAVELCORE_PHASE_CONFIRM: Pxx
```

Only a **USER-authored** message counts. Assistant text containing the token does not authorize.

---

## 3. Failure / Stop Paths

| Path | Transition | Action |
|------|------------|--------|
| Verification failure | → `FAIL` → REPORT → STOP | Architect may issue correction task |
| Preconditions / conflict | → `BLOCKED` → REPORT → STOP | No autonomous repair of history/ADR |
| Architecture decision needed | → `ARCHITECTURAL_DECISION_REQUIRED` → STOP | No silent ADR invention |
| Human breakpoint | → `HUMAN_CONFIRM_NEEDED` → STOP | Pipeline stopped until USER resolves |
| Automation pause | → paused | No new automatic task |

---

## 4. Progress Ledger States (Human-Facing)

Allowed states include:

| State | Meaning |
|-------|---------|
| `NOT_STARTED` | Not begun |
| `IN_PROGRESS` | Actively executing / phase open |
| `AWAITING_ARCHITECT_REVIEW` | Cursor reported; architect not yet accepted |
| `AWAITING_HUMAN_CONFIRMATION` | Waiting for USER token |
| `COMPLETE` | Accepted / synchronized in durable repo state |
| `BLOCKED` | Cannot proceed safely |
| `FAIL` | Failed verification |

Important:

```text
Cursor PASS
≠ COMPLETE (when architect acceptance is required)

Cursor PASS
→ AWAITING_ARCHITECT_REVIEW
→ (after architect acceptance / state sync)
→ COMPLETE
```

---

## 5. Pipeline Health Labels

| Label | Meaning |
|-------|---------|
| `NORMAL` | May accept next explicit same-phase task (when protocol active and not human-gated) |
| `STOPPED — HUMAN_CONFIRM_NEEDED` | Hard stop for human decision |
| `ACTIVE` | ADR 0013 handoff protocol accepted/active |
| `OFF` / `HUMAN` | Automatic PIPELINE loop not running (default; see Proposed ADR 0014) |

---

## 6. Out-of-Order Detection

If expected sequence is approximately:

```text
T001 → T001A → T002 → T002A → T003
```

but repository evidence shows material skipping (e.g. `T001` COMPLETE, `T003` COMPLETE, `T002` NOT_STARTED):

- do **not** silently continue as normal
- return `BLOCKED` or escalate to `HUMAN_CONFIRM_NEEDED` according to cause
- do not invent catch-up Task-IDs

---

## 7. Ledger Scope Rules

- **Current phase:** list tasks cumulatively (not only the newest)
- **Prior completed phases:** compact summary (e.g. `P00 => COMPLETE`)
- Durable detail remains in `PROJECT-STATE`, ROADMAP, Git history, ADRs

---

## 8. NON_EXECUTABLE_EXAMPLE — state board

```text
TravelCore Progress
P00                          => COMPLETE
TC-GOV-T001                  => AWAITING_ARCHITECT_REVIEW
P01                          => NOT_STARTED
Pipeline                     => NOT_ACTIVE_UNTIL_ADR_0013_ACCEPTED
HUMAN_CONFIRM_NEEDED         => NO
Next                         => Architect review of TC-GOV-T001 / ADR 0013
```
