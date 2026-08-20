# TC-PIPELINE-AUTOMATION-V2-POC — Decision Record

| Field | Value |
|-------|--------|
| Task-ID | `TC-PIPELINE-AUTOMATION-V2-POC` |
| Phase | Post-P30 — Pipeline Automation |
| Baseline at execute | `ba714e5` (`origin/main`) |
| Envelope | `docs/plans/TC-PIPELINE-AUTOMATION-V2-POC-task-envelope.md` |
| Scope | Docs / conventions only — **no** product, frontend, backend, or orchestration platform |
| Prior research | `TC-CURSOR-CAPABILITY-REVIEW-001` → prefer **repo task queue + Cursor trigger** |

This document satisfies POC evaluation §2.1–§2.6.

---

## 2.1 Repository task inbox location

### Decision

**Canonical inbox:** `docs/pipeline/inbox/`

### Naming

| Artifact | Convention |
|----------|------------|
| Authorized runnable task | `docs/pipeline/inbox/<Task-ID>.task.md` |
| Claimed / in-flight (optional rename) | `docs/pipeline/inbox/<Task-ID>.executing.md` |
| Failed detection / blocked | `docs/pipeline/inbox/<Task-ID>.blocked.md` (optional) |

`<Task-ID>` matches the envelope `Task-ID:` field exactly (e.g. `TC-P30-T006`).

### Where authorized envelopes land

1. **Runnable queue (this POC):** Architect (or thin bridge) places a complete `TRAVELCORE_CURSOR_TASK_V1` body into  
   `docs/pipeline/inbox/<Task-ID>.task.md` and commits/pushes to `main` (or opens a PR that merges to `main`).
2. **Durable anti-truncation copy (existing practice):** Long envelopes may also live under  
   `docs/plans/<Task-ID>-task-envelope.md`.  
   - Plans file = persistence / SoT copy  
   - Inbox file = **signal that execution is requested now**

### Relationship: inbox vs `docs/plans/*-task-envelope.md`

| Location | Role |
|----------|------|
| `docs/plans/*-task-envelope.md` | Persistent full text; survives ChatGPT truncation; may describe future work |
| `docs/pipeline/inbox/*.task.md` | **Only** place Automations/workers treat as “ready to execute” |

Presence of a plans envelope alone does **not** mean execute. Inbox drop (or an execute cycle that explicitly cites the plans file) is required.

### What is NOT an inbox

- ChatGPT paste alone (unless also written to inbox or explicitly cited in a live authorized cycle)
- Historical chat quotes
- Fenced examples inside markdown docs
- `docs/plans/` files without a matching inbox item or live execute authorization
- RESULT files, recovery docs, ADRs

---

## 2.2 Authorized task detection

A worker / Automation / Agent may treat an inbox file as executable **only if all** hold:

| Check | Rule |
|-------|------|
| Markers | Exact `BEGIN_TRAVELCORE_CURSOR_TASK_V1` … `END_TRAVELCORE_CURSOR_TASK_V1` (unsuffixed) |
| Version | `Protocol-Version: 1` |
| Completeness | Untruncated; both markers present; required fields readable |
| Auto-Execute | `YES` when USER PIPELINE mode applies |
| Uniqueness | Latest complete **unexecuted** envelope for that `Task-ID` |
| Non-example | Not `__EXAMPLE`, not labeled `NON_EXECUTABLE_EXAMPLE`, not quoted-as-history only |
| Recovery / SoT | Pass controller recovery checks; no `SOURCE_OF_TRUTH_CONFLICT` |
| Path | File is under `docs/pipeline/inbox/` and matches `*.task.md` (or live cycle cites plans envelope explicitly) |

### Explicit non-detection

- Chat history without repo persistence
- Partial / truncated envelopes
- Already PASS or Architect-ACCEPT for same `Task-ID` (replay)
- Sample files under `docs/pipeline/**/samples/`
- Envelope-create Task-IDs that only author files (unless their own execute cycle is authorized)

---

## 2.3 Cursor execution trigger options

| Option | Mechanism | Fit for TravelCore POC |
|--------|-----------|------------------------|
| **A. Cursor Automations** | Push / path filter on `docs/pipeline/inbox/**`, or webhook after commit | **Recommended primary** — native schedule/events; no custom daemon |
| **B. Cloud Agents API / SDK** | `POST /v1/agents` (+ follow-up runs) from thin CI/webhook | Good if Automation UI unavailable; slightly more glue |
| **C. Local CLI watcher** | Watch folder → `agent -p` | Works offline; extra process; laptop must be on — **not primary** |
| **D. Manual IDE paste** | Human pastes envelope into Cursor Agent | **Required fallback** / bootstrap; current reliable path |

### Recommendation (ONE primary)

**Primary: A — Cursor Automations** triggered by push (or webhook) when files change under `docs/pipeline/inbox/`.

Rationale:

- Matches capability-review “Option B” architecture (repo queue + Cursor), with Automations as the Cursor-native trigger
- Smallest unattended path without building an orchestrator
- Inbox path filter keeps noise low
- Keeps Architect acceptance human; automation only starts authorized work

**Fallback: D** until Automation is configured; still write tasks to inbox so the queue SoT stays valid.

**Defer: B** as alternate if Automations cannot watch the path; **C** only as emergency local bridge.

### What we will NOT build (triggers)

- ChatGPT DOM scraper as primary trigger
- Always-on local poller platform
- Multi-agent mesh / custom workflow engine

---

## 2.4 Result storage convention

### Decision

**Canonical results:** `docs/pipeline/results/`

### Naming

| Artifact | Convention |
|----------|------------|
| Cursor RESULT | `docs/pipeline/results/<Task-ID>.result.md` |

File body must include exact:

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
…
END_TRAVELCORE_CURSOR_RESULT_V1
```

### Commit / PR expectations (minimal)

1. After execute: commit RESULT file (and any allowed task artifacts) with the task’s required commit message.
2. Push `origin/main` (or PR → main per team norm).
3. Optional: PR/issue comment linking the RESULT path — **not required** for POC if RESULT file is on `main`.
4. Do not duplicate RESULT only in chat without repo write when using inbox workflow.

### How Architect reads acceptance state

| Signal | Meaning |
|--------|---------|
| `docs/pipeline/results/<Task-ID>.result.md` with `Status: PASS` | Cursor claims done → **AWAITING_ARCHITECT_REVIEW** |
| Architect reply / acceptance note in channel **and** recovery/PROJECT-STATE update when required | **ACCEPT** (human) |
| Cursor PASS alone | **Not** ACCEPT |

Inbox item should leave `*.task.md` state (rename to `.executing.md` then remove or move aside once RESULT is committed) so detectors do not re-fire.

---

## 2.5 Recovery compatibility

| Protocol rule | Inbox/result touch-point |
|---------------|--------------------------|
| `TRAVELCORE-RECOVERY-CONTEXT.md` after accepted work | After Architect ACCEPT, recovery may note last accepted Task-ID; inbox/results paths are stable pointers (see README) |
| PROJECT-STATE / ROADMAP win on conflict | Inbox task must not override Accepted ADRs; Agent reports `SOURCE_OF_TRUTH_CONFLICT` |
| No Envelope = No Execution | Empty inbox / no live cite → no run |
| Cursor PASS ≠ Architect ACCEPT | RESULT file ≠ acceptance |
| After RESULT: wait; do not invent next task | Worker stops; next run only on new inbox item or new authorized envelope |

**POC choice:** Do **not** broad-rewrite `docs/ai/*` protocol family. Point recovery readers at `docs/pipeline/README.md` when discussing automation V2. Optional one-line recovery pointer may be added in a later authorized docs task if Architect requests.

---

## 2.6 Replay protection

Mandatory guards:

1. **Task-ID ledger:** If `docs/pipeline/results/<Task-ID>.result.md` exists with prior Cursor `PASS` / Architect `ACCEPT`, do **not** re-execute that Task-ID.
2. **No duplicate commits** for a replayed Task-ID (`REPLAY_BLOCKED`).
3. **Inbox lifecycle:**  
   `received (*.task.md)` → `claimed/executing (*.executing.md)` → `completed` (RESULT written; inbox file removed or archived) / `failed|blocked`.
4. **Idempotent Automation:** If the same commit retriggers Automation, detector must no-op when RESULT already exists or status is `.executing.md` with active claim.
5. **Samples:** Only `__EXAMPLE` / `NON_EXECUTABLE_EXAMPLE` markers in illustrative files (see `samples/`).

Claim stamp (recommended minimal frontmatter or first lines in executing file):

```text
Claimed-At: <ISO-8601>
Claimed-By: cursor-automation|local-agent|<id>
Commit: <sha that triggered>
```

---

## What we will NOT build

- Large orchestration system / workflow platform
- Product, frontend, or backend changes for this POC
- Primary design based on ChatGPT UI scraping
- Replacing Architect acceptance with bots
- Multi-repo automation
- Production billing/hardening beyond these notes
- DEMOFEED / TC-P30-T006 / P30 UI work under this Task-ID

---

## POC deliverable map

| Deliverable | Location |
|-------------|----------|
| Decision record §2.1–§2.6 | This file |
| Inbox + results paths | `docs/pipeline/inbox/`, `docs/pipeline/results/` |
| Conventions README | `docs/pipeline/README.md` |
| Non-executable sample task | `docs/pipeline/samples/NON-EXECUTABLE-sample.task.md` |
| Non-executable sample result | `docs/pipeline/samples/NON-EXECUTABLE-sample.result.md` |

---

## Cumulative note (phase)

Post-P30 automation: research PASS (`TC-CURSOR-CAPABILITY-REVIEW-001`) → envelope-create PASS → this POC conventions PASS (Cursor) → **AWAITING_ARCHITECT_REVIEW**.
