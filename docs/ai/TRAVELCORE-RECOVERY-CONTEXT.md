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

**P35 — Production Payment Provider Readiness** (`TC-P35-T006` Cursor **PASS** · **BLOCKED_ON_PROVIDER_ACCOUNT_FACTS**)

## Completed (recent)

- P35-T005 ACCEPTED — worksheet; user decisions recorded
- P35-T006: Provider-specific design briefs (Behpardakht · Zarinpal · Stripe) · no adapters · no Iran final pick · no core redesign

## Current Authorized Work

**None** — WAITING for Architect after T006 RESULT. Do not implement adapters.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P35-T006 provider-specific design |
| 2026-08-21 | Sync after TC-P35-T005 provider selection worksheet |
| 2026-08-21 | Sync after TC-P35-T004 Iran/UAE provider research |
| 2026-08-21 | Sync after TC-P33-GATE review |
