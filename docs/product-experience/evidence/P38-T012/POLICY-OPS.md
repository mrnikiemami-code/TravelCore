# P38-T012 — AgencyOffer Policy Operations Foundation

| Field | Value |
|-------|--------|
| Task | `TC-P38-T012` |
| Phase | P38 — Multi-Agency Commerce |
| Slice | Commerce Depth — operational policy visibility |
| Status | Cursor PASS (awaiting Architect) |

## Policy operations summary

- `EvaluateDetailedAsync` returns aggregate + all hook decisions (Commercial/Content/Channel/Publication)
- Admin read API: `GET /api/agency-marketplace/moderation/offers/{id}/policy-evaluation`
- Governance mutations still evaluate policies; Deny returns 409 with `policyCode` / `policyName` / `policyReason`
- Admin FE: Evaluate policies control on `/admin/agencies/offers` — shows codes/reasons, **no fake metrics**

## Ownership assessment

- AgencyMarketplace owns offer governance policies
- Pricing / Booking / Payment ownership unchanged
- Policy ≠ Commission

## Compatibility assessment

- Default Allow → approved offers / public marketplace / agency ops unchanged
- No financial calculations introduced

## Tests

- Detailed evaluation unit test
- Existing policy allow/deny + governance host regressions
- Architecture guardrail includes `/policy-evaluation`
