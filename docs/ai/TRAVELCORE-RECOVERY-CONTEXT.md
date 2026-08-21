# TravelCore Recovery Context

| Field | Value |
|-------|--------|
| Document | `docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md` |
| Purpose | Fast durable position snapshot for new ChatGPT / Cursor sessions |
| Authority | Derived from repository SoT — update after gates / accepted tasks |
| Companion | [`../prompts/START-HERE-IF-CHATGPT-IS-LOST.md`](../prompts/START-HERE-IF-CHATGPT-IS-LOST.md) |
| Controller | [`TRAVELCORE-PIPELINE-CONTROLLER.md`](TRAVELCORE-PIPELINE-CONTROLLER.md) |

This file is a **fast recovery aid**. If it conflicts with `PROJECT-STATE.md` / accepted ADRs / Git evidence, those win — report `RECOVERY_CONFLICT` / `SOURCE_OF_TRUTH_CONFLICT`.

---

# Current Project Position

## Identity

| Field | Value |
|-------|--------|
| Project | TravelCore |
| Canonical repository | `mrnikami-code/TravelCore` |
| Architecture | Modular Monolith |
| Backend | .NET 10 / ASP.NET Core 10 Minimal API |
| Frontend | Next.js 16 / React 19 / TypeScript |

## Current Phase

**P35 — Production Payment Provider Readiness** (`TC-P35-T010` Cursor **PASS** · Zarinpal design lock awaiting Architect)

## Completed (recent)

- P35-T009 ACCEPTED — Behpardakht design lock
- P35-T010: Zarinpal adapter design lock · refund CBI temporary disable · inbox audit (48 tracked)

## Current Authorized Work

**None** — WAITING for Architect after T010 RESULT. No Iran final pick. No Zarinpal code.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P35-T010 Zarinpal adapter design lock |
| 2026-08-21 | Sync after TC-P35-T009 Behpardakht Mellat adapter design lock |
| 2026-08-21 | Sync after TC-P35-T008 Stripe test-mode adapter |
| 2026-08-21 | Sync after TC-P33-GATE review |
