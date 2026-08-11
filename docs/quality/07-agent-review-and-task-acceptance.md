# Agent Review and Task Acceptance

منبع: [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md) · [`../architecture/09-ai-development-workflow.md`](../architecture/09-ai-development-workflow.md)
ADR: [`../adr/0011-evidence-based-task-acceptance.md`](../adr/0011-evidence-based-task-acceptance.md) (Accepted)

---

## 1. Cursor Self-Review (Mandatory Before Completion Claim)

Before reporting completion / committing, Cursor reviews its own diff for:

- scope correctness
- unexpected files
- architecture violations
- missing applicable tests
- state documentation consistency
- `git diff --check`

Do not immediately commit after writing without reviewing the diff.

---

## 2. Hermes Review (Risk-Based)

Hermes may act as independent reviewer/auditor. Especially valuable for:

large changes · security-sensitive · cross-module · data migrations · architecture-sensitive · complex business logic.

Do **not** require Hermes for every trivial Task. Policy is risk-based (see DoD risk levels).

---

## 3. Architect Review

Architecture-affecting work requires Chief Architect review where workflow says so.

**Cursor cannot:**

- self-accept an ADR
- convert Proposed → Accepted without architect instruction
- self-declare architectural change accepted

---

## 4. Accepted-Document Integrity

If modifying an accepted architecture document:

classify SAFE EXTENSION vs ACCEPTED ARCHITECTURE MODIFICATION vs UNRELATED.

Silent rewrite of accepted rules is forbidden. Rule changes need architect review / ADR when meaningful.

---

## 5. Final Status Reporting

| Status | When |
|--------|------|
| **PASS** | All applicable gates PASS (or valid N/A); required push succeeded; tree clean as expected; no unresolved architecture concern |
| **PARTIAL** | Scoped work done but a non-fundamental requested verification/action incomplete — explain remainder |
| **BLOCKED** | Safe progress needs architect decision · missing access · history divergence · critical env · unresolved migration/data risk · security conflict |

Do **not** report PASS if: tests failed · required test not run · push failed when required · unexpectedly dirty tree · unresolved architecture concern · required migration unvalidated.

Do not work around a blocker by silently changing scope.

---

## 6. Gate State Discipline

- Unexecuted required gate ≠ PASS
- Environment prevents required gate → BLOCKED
- Credible non-need → NOT_APPLICABLE with reason

---

## 7. Multi-Machine / Git Integrity

Before Task: discover repo · verify branch · verify tree · fetch · ff-only sync when appropriate.

After commit when required: push (no force-push in normal workflow).

Do not casually rewrite shared `main` history. Do not discard user changes automatically. Unexpected uncommitted files must be reported.

Working tree should normally be clean after completion commit.

---

## 8. Commit Quality

Scoped commits. Do not mix unrelated refactor · personal formatting · dependency upgrades · architecture changes into a narrow feature task unless Task scope says so.

Secrets never committed.

---

## 9. Documentation / State

Do not update architecture docs to describe aspirational code as if implemented.
Do not leave PROJECT-STATE claiming acceptance before architect acceptance.

Comments explain **WHY**; identifiers English; Persian docs/comments OK; UTF-8 readable.

---

## 10. TODO Policy

TODOs must be actionable with context/task reference when possible. Do not use TODO to bypass correctness. Quality debt needs ownership.

---

## 11. CI Once Exists

Local PASS is valuable; required CI gates must also pass before final acceptance/merge per workflow. No «works only on my machine» acceptance.

CI design staged/risk-based later — not implemented in this Task.
