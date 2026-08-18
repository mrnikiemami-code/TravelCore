# TC-P20-T006 Preflight Checklist (Blocked Until T005 Acceptance)

This checklist is preparation-only.

Do not execute `TC-P20-T006` implementation before architect acceptance of `TC-P20-T005`.

## Preconditions

- [ ] Architect review completed for `TC-P20-T005`
- [ ] Architect acceptance explicitly recorded
- [ ] `docs/PROJECT-STATE.md` updated from T005 awaiting-review to accepted
- [ ] `docs/ROADMAP.md` current-next-task advanced to `TC-P20-T006`
- [ ] `docs/plans/P20-implementation-plan.md` still keeps `P20-R6` OPEN (until lock)

## T006 Scope Guardrails (from current SoT)

- [ ] Refund/cancellation/compensation remains Payment boundary topic
- [ ] No booking status rewrites by Payment module
- [ ] No settlement/accounting/agency-ledger expansion
- [ ] No new provider selection/routing engine invention
- [ ] No fake success/reversal semantics in public UX

## Ready-to-start Signal

Start T006 only when all preconditions are checked and an explicit architect envelope for `TC-P20-T006` exists.

