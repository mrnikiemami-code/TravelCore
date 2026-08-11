# ADR 0013 — Controlled Agent Handoff and Human-Gated Phase Transitions

- **Status:** Accepted
- **Date:** 2026-08-11
- **Task:** TC-GOV-T001 / TC-GOV-T001A
- **Related:**
  - [`../architecture/16-agent-handoff-and-phase-gates.md`](../architecture/16-agent-handoff-and-phase-gates.md)
  - [`../ai/01-chatgpt-cursor-handoff-protocol.md`](../ai/01-chatgpt-cursor-handoff-protocol.md)
  - [`../ai/02-execution-state-machine.md`](../ai/02-execution-state-machine.md)
  - [`../ai/03-human-confirmation-gates.md`](../ai/03-human-confirmation-gates.md)
  - [`../architecture/09-ai-development-workflow.md`](../architecture/09-ai-development-workflow.md)
  - ADR 0011 (evidence-based acceptance)

---

## Context

TravelCore already has an AI development workflow (Architect → scoped Cursor task → evidence → review). Manual copy/paste of every prompt is friction, but giving Cursor unrestricted ability to treat the entire ChatGPT conversation as executable is unsafe:

- Historical P00 prompts remain visible
- Examples and explanations look like instructions
- Cursor PASS can be mistaken for architectural acceptance
- Phase transitions and destructive work need Product Owner authority
- Chat memory is not a durable source of truth

A controlled machine-readable handoff is needed **without** transferring architectural authority to automation.

---

## Decision

1. **Explicit machine-readable handoff is allowed** via `TRAVELCORE_CURSOR_TASK_V1` / `TRAVELCORE_CURSOR_RESULT_V1` envelopes with exact BEGIN/END markers.
2. **Chat history is non-executable by default.** Only a valid current envelope may execute.
3. **Latest complete unexecuted task only** — no historical replay scan.
4. **One task at a time** — execute, report, STOP; no recursive continuation.
5. **Architect review barrier** — Cursor PASS → `AWAITING_ARCHITECT_REVIEW`; Cursor cannot self-accept architecture.
6. **Human-gated roadmap phase transitions** via USER-only `TRAVELCORE_PHASE_CONFIRM: Pxx`.
7. **Human-gated CRITICAL tasks** via USER-only `TRAVELCORE_TASK_CONFIRM: <Task-ID>`.
8. **`HUMAN_CONFIRM_NEEDED` breakpoint** stops the pipeline for high-risk / low-confidence situations.
9. **Repository source-of-truth protection** — Accepted ADRs / AGENTS / architecture docs override chat; conflicts → `SOURCE_OF_TRUTH_CONFLICT` / BLOCKED.
10. **Replay protection** — completed Task-IDs must not re-execute (`REPLAY_BLOCKED`).
11. **Cumulative human-visible progress ledger** derived from durable repository evidence; chat memory alone is not authoritative; ledger/repo conflict → `STATE_LEDGER_CONFLICT`.
12. **Cursor cannot self-accept ADRs** or invent the next Task-ID.
13. **Activation is gated:** documenting this decision does not activate the automatic pipeline until this ADR is Accepted and an explicit activation/update task updates `AGENTS.md` / recovery as required.

Until Accepted, operational pipeline label:

```text
NOT_ACTIVE_UNTIL_ADR_0013_ACCEPTED
```

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| ادامهٔ فقط copy/paste دستی | اصطکاک بالا؛ خطای انسانی در handoff |
| اجرای آزاد هر محتوای صفحهٔ ChatGPT | خطر replay و دستورات تاریخی |
| اتوماسیون کامل Phase-to-Phase بدون تأیید User | حذف اختیار Product Owner در مرزهای پرریسک |
| Cursor خود ADR را Accepted کند | نقض تفکیک Architect / Implementer |
| Ledger فقط از حافظهٔ چت | غیرقابل حسابرسی؛ تعارض با ریپو |

---

## Consequences

### مثبت

- کانال حمل‌ونقل ایمن‌تر پس از پذیرش
- مرز روشن انسان / معمار / Cursor
- Progress قابل‌مشاهده برای Product Owner
- حفاظت از ADRهای Accepted و تاریخچهٔ Git

### منفی / هزینه

- سربار قالب Task/Result
- توقف‌های بیشتر در مرز Phase و CRITICAL
- تا Accepted شدن، کانال خودکار نباید فرض شود

---

## Migration / Impact

- Documentation-only under `docs/architecture`, `docs/ai`, `docs/adr`, `PROJECT-STATE`
- No application code impact
- No change to ADR 0001–0012 statuses
- Activated in `AGENTS.md` and recovery via TC-GOV-T001A

---

## Status Note

**Accepted** via TC-GOV-T001A after Chief Architect review of TC-GOV-T001. Cursor still cannot self-accept future ADRs; acceptance requires architect review + explicit acceptance/state-sync task.
