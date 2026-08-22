# P38-T014 — AgencyOffer Governance Operations Refinement

| Field | Value |
|-------|--------|
| Task | `TC-P38-T014` |
| Phase | P38 — Multi-Agency Commerce |
| Slice | Commerce Depth — governance ops visibility / search |
| Status | Cursor PASS (awaiting Architect) |

## Governance operations summary

- Admin list query: `GET /api/agency-marketplace/moderation/offers?publicationStatus={status}&take=`
- Allowed statuses: Submitted (pending) · Approved · Rejected · Suspended · Retired
- Legacy alias retained: `GET .../pending` → Submitted
- Queue item enrichment: `lastDecisionKind` · `lastDecisionAt` · `hasGovernanceHistory`
- Admin FE: status filter + current state + last decision + history availability on `/admin/agencies/offers`

## Authorization assessment

- List / pending / policy-evaluation: `Offers.Read`
- Mutate + governance-history: `Offers.Moderate`
- Self-moderation denial unchanged
- No financial fields exposed

## Compatibility assessment

- Public marketplace / agency catalog / booking flow untouched
- No fake metrics · revenue · commission · settlement amounts

## Tests

- Unit: ops status parse allow/deny (`AgencyOfferGovernanceTests`)
- Host: Approved/Suspended filters · lastDecision · bad Published filter · history regression
- Architecture: `publicationStatus` + `ListOffersAsync` guardrail

## Explicitly out of scope

- Commission · Settlement · Payout · Financial reporting
