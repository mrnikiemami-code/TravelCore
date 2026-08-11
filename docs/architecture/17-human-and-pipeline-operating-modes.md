# Human and Pipeline Operating Modes

این سند **نمای معماری** برای دو حالت اجرای TravelCore است.

Canonical entry: [`../ai/TRAVELCORE-PIPELINE-PROTOCOL.md`](../ai/TRAVELCORE-PIPELINE-PROTOCOL.md)

Detail: [`../ai/04-human-and-pipeline-modes.md`](../ai/04-human-and-pipeline-modes.md)

ADR: [`../adr/0014-human-pipeline-modes-and-chat-limit-safety.md`](../adr/0014-human-pipeline-modes-and-chat-limit-safety.md) (**Proposed**)

**قانون:** تا Accepted شدن ADR 0014، این قواعد در `AGENTS.md` / Recovery به‌عنوان کانال اجباری فعال نمی‌شوند. ADR 0013 همچنان مرجع handoff پذیرفته‌شده است.

---

## Why Two Modes?

ADR 0013 defines safe one-task handoff. Product Owner also needs:

1. Normal interactive Cursor use without background polling (**HUMAN**)
2. Optional automatic watch of an architect ChatGPT conversation (**PIPELINE**)
3. Hard stop when chat continuity / context limits make automation unsafe

---

## Mode Model

```text
HUMAN (default)
  ↔ USER token / clear instruction
PIPELINE (opt-in)
```

PIPELINE → HUMAN immediately ends automatic polling, discovery, and auto-execution.

Mode switches do not rewrite task/phase progress.

---

## Pipeline Watch

Passive 20s ±3s polling when PIPELINE is active and conversation access is reliable.

Polling ≠ permission to execute historical content.

---

## Chat-Limit Safety

`CHAT_CONTEXT_LIMIT` is a mandatory `HUMAN_CONFIRM_NEEDED` stop.

No automatic resume. Recovery defaults to HUMAN. USER must re-opt into PIPELINE after review.

---

## Relationship to ADR 0013

ADR 0014 **extends** transport/runtime control. It does **not** weaken:

- repository source of truth
- one task at a time
- architect review barrier
- phase / CRITICAL gates
- HUMAN_CONFIRM_NEEDED
- replay protection
- non-executable historical chat
