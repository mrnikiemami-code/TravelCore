# Human and Pipeline Operating Modes

Canonical entry: [`TRAVELCORE-PIPELINE-PROTOCOL.md`](TRAVELCORE-PIPELINE-PROTOCOL.md)

Architecture companion: [`../architecture/17-human-and-pipeline-operating-modes.md`](../architecture/17-human-and-pipeline-operating-modes.md)

**Status:** Accepted (ADR 0014 via TC-GOV-T002A). Binding in `AGENTS.md` / Recovery.

---

## Modes

| Mode | Meaning |
|------|---------|
| `HUMAN` | Default. No auto poll / discovery / ChatGPT→Cursor execution. User drives Cursor. ADR 0013 governance still applies to any explicit task work. |
| `PIPELINE` | USER opt-in. Passiveive watch of supplied ChatGPT conversation; execute only latest valid unexecuted task envelope; report; STOP. |

Tokens:

```text
TRAVELCORE_MODE: HUMAN
TRAVELCORE_MODE: PIPELINE
```

Clear Persian USER phrases also count when unambiguous («برو روی مد Human» / «برو روی مد Pipeline»).

Only USER may activate PIPELINE. ChatGPT cannot activate silently.

---

## Polling (PIPELINE only)

- Base: **20 seconds**
- Jitter: **±3 seconds** (~17–23s)
- Passive detection only
- After **3 consecutive** watch failures → `HUMAN_CONFIRM_NEEDED` / `CHAT_WATCH_UNAVAILABLE`

---

## Chat Context Limit

Observable continuity failure → `HUMAN_CONFIRM_NEEDED` / `CHAT_CONTEXT_LIMIT`.

No auto conversation switch. No Recovery-then-continue. Default after stop/recovery: **HUMAN**.

---

## Single Active Cursor

One active Cursor execution agent. No multi-Cursor leasing without a future ADR.

---

## Safety (never overridden by PIPELINE)

- No ADR self-acceptance
- No architecture rewrite to match chat
- No skipping quality gates / phase / CRITICAL gates
- No force push / destructive production actions without required human authority
- No inventing next Task-ID
