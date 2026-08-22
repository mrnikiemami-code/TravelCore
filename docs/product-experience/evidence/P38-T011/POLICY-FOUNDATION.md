# P38-T011 — AgencyOffer Policy Foundation

| Field | Value |
|-------|--------|
| Task | `TC-P38-T011` |
| Phase | P38 — Multi-Agency Commerce |
| Slice | Commerce Depth — Policy evaluation foundation |
| Status | Cursor PASS (awaiting Architect) |

## Policy foundation summary

- Decision model: `AgencyOfferPolicyDecision` (`Allow`/`Deny` + `Code` + `Reason` + `PolicyName`)
- Context: `AgencyOfferPolicyContext` (offer/agency/tour/channel/publication/visibility/status — **no money fields**)
- Hooks: Commercial · Content · Channel · Publication (default Allow stubs)
- Composite: `IAgencyOfferPolicyEvaluator` / `AgencyOfferPolicyEvaluator` — first Deny wins
- Governance mutations evaluate policy **after** authorization + self-moderation checks
- Deny → `AgencyOfferPolicyDeniedException` → HTTP 409 (does not bypass Moderate auth)

## Ownership assessment

| Concern | Owner |
|---------|--------|
| AgencyOffer policies | AgencyMarketplace governance |
| Pricing | Pricing module (unchanged) |
| Booking | Booking module (unchanged) |
| Commission / Settlement / Payout | **Not implemented** |

## Authorization assessment

- Moderate permission still required for Admin approve/reject/suspend
- Policy Deny ≠ authorization failure (403 vs 409)
- Self-moderation Forbid still precedes policy evaluation

## Compatibility assessment

- Default policies Allow → existing Admin governance / public Published eligibility unchanged
- No FE changes required for this foundation slice
- Booking / Quote / Payment untouched

## Tests

- `AgencyOfferPolicyFoundationTests` — allow composite · deny first-hook · exception payload
- Existing governance / public eligibility / host access regressions remain green

## Explicit non-goals (honored)

- Commission percentage
- Settlement / Payout / Revenue share rules
