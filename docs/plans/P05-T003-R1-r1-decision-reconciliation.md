# TC-P05-T003-R1 — Reconcile P05 R1 Decision State

| Field | Value |
|-------|--------|
| Task-ID | `TC-P05-T003-R1` |
| Phase | P05 |
| Status | AWAITING_ARCHITECT_REVIEW |
| Baseline | `7226451` |
| Product code changed | **NO** |

## Architect decision (authoritative)

**P05-R1 = RESOLVED**

- `DestinationTranslation.Slug` = authoritative **current** localized Destination slug
- Destination owns and mutates it
- SEO does **not** own or write `DestinationTranslation.Slug`
- SEO owns: SeoRoute binding · historical public path records · path reservation · redirect candidate/history mechanics · eventual redirect resolution
- Historical path ≠ current Destination slug
- Redirect history ≠ Destination domain history
- No cross-schema writes
- Preferred redirect-chain: `A → C`, `B → C` (not `A → B → C`)

## Implementation verification (T003)

| Check | Result |
|-------|--------|
| Destination.Translation.Slug remains Destination-owned | PASS (SEO comments/contracts; no Destination schema writes) |
| `seo_path_history` SEO-owned | PASS |
| `seo_path_reservations` SEO-owned | PASS |
| `seo_redirect_candidates` SEO-owned (Pending; T004 engine) | PASS |
| SEO writes destination schema | **NO** |
| T004 still owns redirect engine | YES |

**Implementation-compatible:** YES — no product repair required.

## Remediation

Docs/state only: mark R1 RESOLVED consistently; keep R2 UNRESOLVED; keep T003 AWAITING_ARCHITECT_REVIEW; T004 NOT_STARTED.

## Recommendation

After this ledger reconciliation, architect may finally accept `TC-P05-T003`.
