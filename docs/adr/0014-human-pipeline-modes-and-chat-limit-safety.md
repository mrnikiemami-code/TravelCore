# ADR 0014 — Human/Pipeline Operating Modes and Chat-Limit Safety

- **Status:** Proposed
- **Date:** 2026-08-11
- **Task:** TC-GOV-T002
- **Related:**
  - [`../ai/TRAVELCORE-PIPELINE-PROTOCOL.md`](../ai/TRAVELCORE-PIPELINE-PROTOCOL.md)
  - [`../ai/04-human-and-pipeline-modes.md`](../ai/04-human-and-pipeline-modes.md)
  - [`../architecture/17-human-and-pipeline-operating-modes.md`](../architecture/17-human-and-pipeline-operating-modes.md)
  - [`../ai/pipeline-runtime-policy.json`](../ai/pipeline-runtime-policy.json)
  - ADR 0013 (Accepted controlled handoff)

---

## Context

ADR 0013 locks safe ChatGPT↔Cursor handoff (envelopes, one task, architect barrier, human phase/CRITICAL gates, ledger, SoT). Operators still need:

- a default interactive mode without background automation
- an explicit USER opt-in for passive conversation watching
- a mandatory stop when ChatGPT chat/context continuity fails
- no silent auto-recovery that resumes development after chat loss

Without this, agents may keep polling forever, invent continuation after context limits, or treat prior PIPELINE activation as durable across chat resets.

---

## Decision

1. **Two modes:** `HUMAN` and `PIPELINE`.
2. **`HUMAN` is default.**
3. **`PIPELINE` is USER opt-in** via `TRAVELCORE_MODE: PIPELINE` (or clear USER natural-language equivalent). ChatGPT cannot activate silently.
4. **`HUMAN` immediately exits** automatic polling, discovery, and auto-execution.
5. **Passive polling** when PIPELINE is active: base **20 seconds** with **±3 seconds** jitter; polling is detection only.
6. **One active Cursor** assumption; no multi-Cursor leasing without a future ADR.
7. **Phase gates and CRITICAL gates from ADR 0013 are preserved**; PIPELINE never bypasses them.
8. **`HUMAN_CONFIRM_NEEDED` remains the global breakpoint.**
9. **`CHAT_CONTEXT_LIMIT` is a mandatory STOP** (`HUMAN_CONFIRM_NEEDED`); automatic continuation is forbidden.
10. **No automatic recovery/continuation after chat limit** (no auto new-chat selection, no Recovery-then-continue, no inferred next task).
11. **Recovery defaults to `HUMAN`.** Prior PIPELINE state must not be inferred from a lost chat.
12. **Private runtime state is not committed** (URLs, cookies, tokens, personal paths). Stable config only in `pipeline-runtime-policy.json`.

Activation in `AGENTS.md` / Recovery requires a later acceptance task after Chief Architect review.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| Always-on automatic polling | مزاحم؛ بدون اختیار صریح USER |
| Auto-resume after chat limit via Recovery | خطر اجرای نابینا و جعل رضایت |
| Multi-Cursor session leasing now | پیچیدگی زودرس؛ خارج از فرض تک‌عامل |
| Soften phase/CRITICAL gates in PIPELINE | نقض ADR 0013 |

---

## Consequences

### مثبت

- کنترل روشن USER روی اتوماسیون
- توقف اجباری در مرز محدودیت چت
- تفکیک transport automation از معماری Accepted

### منفی / هزینه

- سربار توکن‌های mode
- تا Accepted شدن، دو لایهٔ «مستند» و «فعال در AGENTS»

---

## Migration / Impact

- Documentation under `docs/ai`, `docs/architecture`, `docs/adr`, `PROJECT-STATE`
- No application code impact
- Does not change ADR 0001–0013 statuses
- Do not activate in `AGENTS.md` until Accepted + explicit activation task

---

## Status Note

**Proposed** via TC-GOV-T002. Cursor must **not** mark this ADR Accepted.
