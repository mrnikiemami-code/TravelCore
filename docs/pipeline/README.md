# Pipeline Automation V2 — Conventions

| Field | Value |
|-------|--------|
| Status | POC conventions (docs only) |
| Decision record | [`TC-PIPELINE-AUTOMATION-V2-POC-DECISION.md`](./TC-PIPELINE-AUTOMATION-V2-POC-DECISION.md) |
| Persistent execute envelope | [`../plans/TC-PIPELINE-AUTOMATION-V2-POC-task-envelope.md`](../plans/TC-PIPELINE-AUTOMATION-V2-POC-task-envelope.md) |
| Product code | Not in scope |

## Canonical paths

| Role | Path |
|------|------|
| **Task inbox** | `docs/pipeline/inbox/` |
| **Results** | `docs/pipeline/results/` |
| **Durable plan envelopes** (anti-truncation SoT) | `docs/plans/*-task-envelope.md` (unchanged) |

## Flow (target)

```text
Architect Task
        |
        v
docs/pipeline/inbox/<Task-ID>.task.md
        |
        v
Cursor Automation / authorized Agent
        |
        v
docs/pipeline/results/<Task-ID>.result.md
        |
        v
Architect review (ACCEPT ≠ Cursor PASS)
```

## Hard rules

1. **No Envelope = No Execution**
2. **Cursor PASS ≠ Architect ACCEPT**
3. Do not invent the next task after RESULT
4. Do not scrape ChatGPT UI as the primary trigger design
5. Illustrative samples use `__EXAMPLE` / `NON_EXECUTABLE_EXAMPLE` markers only
6. No large orchestration platform in this POC

See the decision record for detection, triggers, recovery, and replay protection.
