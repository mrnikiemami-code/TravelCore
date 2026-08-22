# P38-T013 — AgencyOffer Governance Audit Visibility Foundation

| Field | Value |
|-------|--------|
| Task | `TC-P38-T013` |
| Phase | P38 — Multi-Agency Commerce |
| Slice | Commerce Depth — governance audit visibility |
| Status | Cursor PASS (awaiting Architect) |

## Audit foundation summary

- Entity: `AgencyOfferGovernanceEvent` (operational history only)
- Table: `agency_marketplace.agency_offer_governance_events` (migration `20260822040000_P38AgencyOfferGovernanceAudit`)
- Events: Submitted · Approved · Rejected · Published · Unpublished · Suspended · Retired · PolicyDenied
- Writers: Agency panel lifecycle + Admin governance mutations (+ PolicyDenied before throw)
- Admin API: `GET /api/agency-marketplace/moderation/offers/{id}/governance-history` (`Offers.Moderate`)
- Admin FE: Governance history panel on `/admin/agencies/offers`

## Ownership assessment

- Audit belongs to AgencyMarketplace operational governance
- **Not** accounting ledger / commission history / settlement history / payout ledger

## Authorization assessment

- History requires `Offers.Moderate` (Admin) — Agency role Forbidden
- Existing Moderate / self-moderation boundaries unchanged

## Compatibility assessment

- Existing offers continue; public listing / booking untouched
- No fake metrics / fake revenue in UI

## Tests

- Unit: `AgencyOfferGovernanceAuditTests`
- Host: history after submit→approve→publish→suspend · Agency Forbidden
- Architecture: `/governance-history` guardrail
