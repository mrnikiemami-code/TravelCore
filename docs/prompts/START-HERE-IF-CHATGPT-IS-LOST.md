# اگر چت ChatGPT از دست رفت، این فایل را به Cursor بده

اگر گفتگوی معمار ChatGPT محدود شد، از دست رفت یا مجبور شدیم در یک Chat جدید ادامه دهیم، در Cursor فقط بگویید:

`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md را اجرا کن`

Cursor نباید پروژه را ادامه دهد؛ فقط باید وضعیت فعلی Repository را بازیابی کرده و Recovery Packet تولید کند.

---

## نحوه استفاده

اگر ChatGPT محدود شد یا مجبور شدید Chat جدید باز کنید:

در Cursor فقط بنویسید:

`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md را اجرا کن`

سپس کل خروجی `TRAVELCORE — CHATGPT ARCHITECT RECOVERY PACKET`
را در ChatGPT جدید قرار دهید.

در Chat جدید، قبل از ادامه پروژه، معمار باید وضعیت بازیابی‌شده را با شما تطبیق دهد.

---

# TC-P00-T001C — Emergency ChatGPT Architect Recovery Prompt

## Recovery Role

The previous ChatGPT architect conversation is unavailable or cannot be relied upon.

Your job is **NOT** to continue development.

Your job is **NOT** to implement Current Next Task.

Your job is **NOT** to redesign the architecture.

Your **ONLY** job is to reconstruct the current authoritative TravelCore state from the repository and prepare a complete handoff for a new ChatGPT architect conversation.

### Repository discovery (mandatory — no machine-specific path)

TravelCore is developed from **multiple machines**. Do **not** assume a hardcoded local path such as `C:\Users\...` or `D:\Users\...` as the project identity.

At execution time, discover the repository root dynamically:

```powershell
git rev-parse --show-toplevel
```

Then verify identity against the canonical GitHub repository:

```text
mrnikiemami-code/TravelCore
```

Expected HTTPS remote:

```text
https://github.com/mrnikiemami-code/TravelCore.git
```

Also inspect:

```powershell
git remote -v
git branch --show-current
```

If `origin` clearly points to a different GitHub repository, report:

```text
SOURCE OF TRUTH CONFLICT — unexpected Git remote.
```

Do not continue development. Do not “fix” remotes during recovery (recovery remains read-only).

The repository (files + Git history + `origin` identity) is the source of truth.

A local filesystem path may be reported as environment information only; it is **not** the canonical project identity.

Do not depend on Cursor conversation memory.

Do not guess missing information.

---

## CRITICAL: FUTURE-PROOF DYNAMIC DISCOVERY

This recovery prompt must remain valid in any phase (P00, P12, P25, …) and on any developer machine.

Do **NOT** hardcode assumptions that the project is still in P00.

Do **NOT** hardcode a mandatory local Windows path.

At execution time, dynamically discover from the repository:

- repository root via `git rev-parse --show-toplevel`
- Git remote / canonical repository identity
- Current Phase
- Last Accepted Task
- Last Accepted Commit
- Current Next Task
- current architecture documents
- accepted ADRs
- current module boundaries
- current Git state

Use whatever documents exist **now**. Do not invent missing ones.

---

## READ AUTHORITATIVE SOURCES

At execution time, first discover and read what exists:

1. `AGENTS.md`
2. `docs/PROJECT-STATE.md`
3. `docs/ROADMAP.md` (if present)
4. `docs/architecture/15-future-architecture-transition-map.md` (if present)
5. `docs/architecture/16-agent-handoff-and-phase-gates.md` (if present)
6. `docs/ai/01-chatgpt-cursor-handoff-protocol.md` · `docs/ai/02-execution-state-machine.md` · `docs/ai/03-human-confirmation-gates.md` (if present)
7. `docs/architecture/**`
8. `docs/domain/**`
9. `docs/adr/**`
10. `docs/prompts/**`
11. Later architecture-related docs **if directories exist**, for example:
   - `docs/ui/**`
   - `docs/i18n/**`
   - `docs/seo/**`
   - `docs/data/**`
   - `docs/security/**`
   - `docs/testing/**`

Do not assume all these directories already exist.

Discover the repository state dynamically.

When handoff protocol docs exist (ADR 0013 Accepted), recovery **must** also reconstruct from durable repo evidence:

- Pipeline State (`ACTIVE` / `STOPPED — HUMAN_CONFIRM_NEEDED` / …)
- Current Task State
- Current Phase Ledger (cumulative for current phase)
- Human Confirmation State
- Phase Boundary State (`READY_AWAITING_HUMAN_CONFIRMATION` when applicable)

Recovery remains **READ-ONLY**.

Recovery must **never fabricate**:

- phase confirmation (`TRAVELCORE_PHASE_CONFIRM`)
- critical-task confirmation (`TRAVELCORE_TASK_CONFIRM`)
- architect acceptance

If recovery finds `READY_AWAITING_HUMAN_CONFIRMATION` / `HUMAN_CONFIRM_NEEDED`, it must **preserve the stop** and report the required USER token. Do not auto-start the next phase or invent consent from chat memory.

---

## SOURCE OF TRUTH PRIORITY

Use the repository's documented Source of Truth rules (typically in `docs/PROJECT-STATE.md`).

At minimum respect:

1. Accepted ADRs
2. `AGENTS.md`
3. current accepted architecture / domain / UI / i18n / SEO / data documentation
4. `PROJECT-STATE.md`
5. `ROADMAP.md` for execution ordering
6. current accepted task specification
7. implementation / code
8. historical prompts / chat discussions

If the repository defines a newer accepted Source of Truth order, use that newer rule.

Never allow an old prompt to override a later accepted ADR.

If documents conflict, do **not** silently choose. Report:

```text
SOURCE OF TRUTH CONFLICT
```

Then list Source A, Source B, Conflict, and Recommended architect action: Review before continuing.

Cursor must not resolve architectural conflicts autonomously.

---

## INSPECT ACCEPTED ADRs

Inspect all ADR files under `docs/adr/` (if any besides process README).

Distinguish statuses:

- Accepted
- Proposed
- Rejected
- Superseded

Only **Accepted** ADRs represent active decisions.

If an ADR is Superseded, report the newer authoritative ADR instead.

For each relevant Accepted ADR report:

- ADR id/title
- decision
- architectural impact

If only the ADR process README exists and no decision ADRs yet, say so clearly.

---

## INSPECT PROJECT STATE

Read `docs/PROJECT-STATE.md` and extract:

- Current Phase
- Last Accepted Task
- Last Accepted Commit
- Current Next Task
- Known blockers
- Environment notes
- Current locked decisions
- Pipeline State / Agent Handoff Pipeline (if present)
- Phase Transition State / Human Confirmation State (if present)
- Current Phase Ledger signals for governance + product tasks (if present)

**Do not modify** `PROJECT-STATE.md` during recovery.

---

## INSPECT ROADMAP

If `docs/ROADMAP.md` exists, read it and extract:

- phase sequence
- current phase
- immediately upcoming phases
- phase dependencies
- completion gates

Do not dump the entire roadmap. Provide enough context for the new architect to continue safely.

---

## INSPECT FUTURE ARCHITECTURE TRANSITION MAP

If `docs/architecture/15-future-architecture-transition-map.md` exists, read it.

For concerns relevant to the Current Phase / Current Next Task / next dependent phases, report where useful:

- Current State
- Target State
- Transition Trigger
- Target Phase
- Preserved Invariants

This map supplements ROADMAP. It does **not** authorize starting future work.

Recovery remains READ-ONLY and must NOT automatically begin implementation.

---

## INSPECT GIT (READ-ONLY)

Run only read-only commands, for example:

```powershell
git status --short
git branch --show-current
git log --oneline --decorate -30
git remote -v
git log -1 --oneline
```

If useful:

```powershell
git status
```

Determine:

- current branch
- HEAD
- clean/dirty working tree
- untracked files
- unfinished changes
- possible local-only commits
- possible incomplete task

### Forbidden during recovery

Do **NOT** run:

- `git reset`
- `git clean`
- `git checkout`
- `git restore`
- `git stash`
- `git commit`
- `git push`

Recovery is **READ-ONLY**.

---

## DETECT INCOMPLETE WORK

Look for evidence that a task was started but not accepted, for example:

- dirty working tree
- prompt exists but PROJECT-STATE still points to a previous task
- commits exist beyond Last Accepted Commit
- partial docs/code
- TODOs indicating unfinished current task

If detected, clearly report:

```text
POTENTIAL UNFINISHED TASK
```

Do **NOT** complete it.

Do **NOT** discard it.

---

## GENERATE RECOVERY PACKET

Output a self-contained report titled exactly:

```text
# TRAVELCORE — CHATGPT ARCHITECT RECOVERY PACKET
```

It must be sufficient for a new ChatGPT conversation to understand where TravelCore currently stands without access to the previous chat.

Include these sections.

### A. Project Identity

Project · Canonical GitHub repository (`owner/name`) · `origin` URL · Local path (environment only; not canonical identity) · Branch · Architecture style · Backend stack · Frontend stack · Data/platform stack

### B. Current Position

Current Phase · Phase Status · Last Accepted Task · Last Accepted Commit · Current Next Task · Purpose of Current Next Task

This is the most important section.

### C. Accepted Work History

Important accepted tasks in chronological order.

For each: Task ID · Purpose · Status · Commit (if available)

Avoid dumping every trivial commit.

### D. Locked Architectural Decisions

Summarize CURRENT accepted decisions required to continue safely.

Where defined, consider: Modular Monolith · module boundaries · dependency direction · database · EF Core · Dapper · PostgreSQL · Redis · S3 · IDs · Money · Pricing · Mixed Currency · Date/Time · i18n · RTL/LTR · bidi · UI · mobile · SEO · accessibility · events · Outbox · security · observability · testing · AI governance

Where something has not been decided, explicitly state:

```text
NOT YET DECIDED
```

Never invent the missing decision.

### E. Module Map and Ownership

If module-boundary documentation exists, summarize for every currently approved module:

Module · Purpose · Owns · Does Not Own · Important Dependencies · Important Contracts

If detailed module mapping has not yet happened, state that clearly.

### F. Critical Domain Distinctions

Extract all important accepted distinctions from repository documents (authority), for example:

- TourProduct != TourDeparture
- Hotel Catalog != Hotel Booking
- Price != Quote != Payment
- PassengerCategory != Occupancy
- Locale != Currency != Calendar != Timezone
- Domain Model != Persistence Model != API Contract != Page View Model

### G. Product Direction

Briefly summarize what TravelCore is, principal business areas, relevant page archetypes, important reference platforms.

No marketing language.

### H. Roadmap Position

Summarize roadmap with statuses: COMPLETE · IN_PROGRESS · PLANNED · BLOCKED

Emphasize: current phase · current next task · next 2–3 major dependent phases

If the Future Architecture Transition Map exists, add a short subsection for the most relevant upcoming transitions (Current State · Target State · Trigger/Phase · Invariants). Do not dump the entire map.

### I. Cross-Cutting Requirements

Summarize accepted rules for Multilingual · RTL/LTR · Bidi · Mobile · SEO · Accessibility · Security · Testing · Performance · Observability

### J. Agent Governance

Explain current rules for ChatGPT Architect · Cursor · Hermes · Automated Tests

Include:

- One Task → One Writer
- Cursor does not autonomously make architectural changes
- Architecture changes require architect decision / ADR when applicable

### K. Accepted ADRs

List active Accepted ADRs and impact.

Exclude rejected decisions.

Indicate superseded decisions correctly.

### L. Environment and Known Issues

Only currently relevant items: SDK/tool versions · database/tool state · NuGet connectivity · GitHub state · temporary workarounds · current blockers

Do not clutter with fully resolved historical problems unless they still matter.

### M. Git / Working Tree State

Report exactly:

```text
Branch:
HEAD:
Working Tree: CLEAN / DIRTY
Uncommitted:
Remote:
Potential unfinished work: YES / NO / UNKNOWN
```

### N. Architectural Concerns / Conflicts

Only genuine unresolved concerns documented or detected.

If none: `None.`

### O. Files the New Architect Must Read

Prioritized SHORT list.

Always include:

- `AGENTS.md`
- `docs/PROJECT-STATE.md`

Also include ROADMAP (if present), `docs/architecture/15-future-architecture-transition-map.md` (if present), and documents relevant to Current Next Task.

Do not dump every file.

---

## CREATE READY-TO-COPY NEW CHAT PROMPT

At the end of the packet, produce this section first (or immediately after the packet title flow as specified):

```text
# COPY THIS INTO A NEW CHATGPT CHAT
```

The copyable text must say:

You are the senior/chief software architect for my TravelCore project.

The previous architect conversation is unavailable or cannot be relied upon.

Do NOT redesign TravelCore from scratch.

The repository is the source of truth.

I am providing a Recovery Packet reconstructed from the current repository.

Your role is primarily:

- architecture analysis
- domain modeling
- specification
- architectural review
- generating precise scoped implementation prompts for Cursor

Cursor is the primary implementation agent.

Hermes may act as an independent reviewer.

Before continuing:

1. Read the entire Recovery Packet.
2. Identify Current Phase.
3. Identify Last Accepted Task.
4. Identify Current Next Task.
5. Respect all accepted ADRs and locked architecture.
6. Detect any source-of-truth conflict or unfinished working-tree state.
7. Do NOT produce the next Cursor task yet.

First reply to me with:

- current project position
- key locked decisions
- current next task
- detected conflict/blocker/unfinished work, if any

Then wait for my confirmation before continuing.

After that ready-to-copy text, provide the full Recovery Packet.

---

## OPTIONAL RECOVERY FILE

In addition to terminal/chat output, Cursor MAY create a temporary recovery packet file at repository root **only if the user explicitly requests it** at execution time.

Default behavior:

- READ-ONLY
- NO FILE CHANGES

Do not automatically commit a generated Recovery Packet; it is a transient recovery artifact.

Permanent state already belongs in repository documentation.

---

## VERY IMPORTANT FINAL RULE

Recovery means **RECOVERY ONLY**.

Cursor must NOT:

- begin Current Next Task
- produce code
- update architecture
- fix issues
- commit anything
- push anything
- modify PROJECT-STATE
- modify ROADMAP

Stop after generating the Recovery Packet.
