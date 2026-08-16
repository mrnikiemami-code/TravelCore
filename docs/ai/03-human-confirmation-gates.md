# Human Confirmation Gates

Companion to:

- [`TRAVELCORE-PIPELINE-PROTOCOL.md`](TRAVELCORE-PIPELINE-PROTOCOL.md) (canonical entry)
- [`01-chatgpt-cursor-handoff-protocol.md`](01-chatgpt-cursor-handoff-protocol.md)
- [`02-execution-state-machine.md`](02-execution-state-machine.md)
- [`04-human-and-pipeline-modes.md`](04-human-and-pipeline-modes.md) (Proposed ADR 0014)
- [`../architecture/16-agent-handoff-and-phase-gates.md`](../architecture/16-agent-handoff-and-phase-gates.md)

User remains ultimate authority at phase boundaries and other high-risk stop points.

---

## Continuity Override (USER 2026-08-17)

USER directed (and architect registered) that under **PIPELINE**:

| Former ceremonial gate | New posture |
|------------------------|-------------|
| `TRAVELCORE_TASK_CONFIRM` for phase Gates | **Not required** for routine Gate execution after product tasks are ACCEPTED |
| `TRAVELCORE_PHASE_CONFIRM` for next phase | **Not required** for starting the next phase PLAN after Gate ACCEPT |

**Still STOP / `HUMAN_CONFIRM_NEEDED` when:**

- an architectural choice is genuinely required
- multiple valid paths exist and user preference is needed
- source-of-truth conflict exists
- unsafe/unresolved repository state exists
- implementation would silently resolve an unlocked decision
- USER issues pause / HUMAN mode / explicit stop

Binding machine policy: [`pipeline-runtime-policy.json`](pipeline-runtime-policy.json)  
(`phaseHumanGateRequired: false`, `criticalTaskHumanGateRequired: false`, `autoContinueAfterTaskAccept: true`, `autoStartNextPhasePlanAfterGateAccept: true`)

Tokens may still be sent by USER and still count when present; they are simply **not blockers** for routine Gate/next-phase continuity under this override.

---

## 1. Canonical Breakpoint

```text
HUMAN_CONFIRM_NEEDED
```

When triggered:

```text
Pipeline = STOPPED
```

- No new Cursor task may be issued automatically
- No next implementation task may start
- Remains until explicit USER action resolves it

### Minimum Breakpoint Report

```text
HUMAN_CONFIRM_NEEDED

Reason:
...

Current Task:
...

Current State:
...

Decision Required:
...

Recommended Safe Default:
STOP

Pipeline:
STOPPED
```

---

## 2. Conditions That Trigger `HUMAN_CONFIRM_NEEDED`

At minimum:

- Roadmap phase transition
- CRITICAL engineering task
- Destructive production-data action
- Irreversible external action
- Meaningful architecture decision requiring Product Owner involvement
- Accepted ADR conflict that cannot be safely resolved
- Repository history divergence requiring a human choice
- Credible secret / security incident
- Unexpected scope expansion with material product impact
- Ambiguous user-owned working-tree changes
- Repeated failure where automatic retry is unsafe
- Task result materially inconsistent with repository state
- Implementation deviation that changes product behavior beyond approved scope
- Required quality gate unavailable where continuing is unsafe
- Insufficient architect confidence to safely continue
- Any situation where continuing automatically creates disproportionate risk

---

## 3. Phase Human Gate

Every roadmap phase transition requires explicit **USER** confirmation.

Pattern:

```text
Pn => COMPLETE
Pn+1 => NOT_STARTED
READY_AWAITING_HUMAN_CONFIRMATION
STOP
```

Required USER token:

```text
TRAVELCORE_PHASE_CONFIRM: Pxx
```

Rules:

- Only USER-authored messages count
- Assistant/ChatGPT text containing the token does **not** count
- Future phase confirmations cannot be pre-authorized
- Each transition (`P01→P02`, …, `P28→P29`) needs its own confirmation

---

## 4. CRITICAL Task Human Gate

CRITICAL tasks require explicit USER confirmation.

Canonical token:

```text
TRAVELCORE_TASK_CONFIRM: <Task-ID>
```

NON_EXECUTABLE_EXAMPLE:

```text
TRAVELCORE_TASK_CONFIRM: TC-P20-T005
```

Prior generic approvals do not count.

---

## 5. Automation Pause / Resume

USER control:

```text
TRAVELCORE_AUTOMATION_PAUSE
```

Effect: no new automatic task may execute.

Resume:

```text
TRAVELCORE_AUTOMATION_RESUME
```

Rules:

- Only USER-authored resume is valid
- Resume does **not** bypass phase or CRITICAL-task confirmation
- Clear natural-language USER requests to stop/pause also override automation

---

## 6. Three Stop Types (Human View)

| Kind | Signal | Who clears it |
|------|--------|---------------|
| Architect review | `AWAITING_ARCHITECT_REVIEW` | ChatGPT Chief Architect (usually) |
| Human breakpoint | `HUMAN_CONFIRM_NEEDED` | USER |
| Phase boundary | `READY_AWAITING_HUMAN_CONFIRMATION` | USER via `TRAVELCORE_PHASE_CONFIRM` |

---

## 7. Authority Boundaries

| Actor | May authorize |
|-------|---------------|
| USER | Phase transitions · CRITICAL tasks · pause/resume · irreversible risk acceptance |
| ChatGPT | Task envelopes · architect acceptance of results · Proposed→Accepted ADR **recommendation** (persisted via explicit acceptance task) |
| Cursor | Neither phase confirm nor ADR self-acceptance |

Cursor cannot invent consent from chat memory after recovery.
