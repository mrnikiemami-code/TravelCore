# P38-T010 — Admin AgencyOffer Governance Foundation

| Field | Value |
|-------|--------|
| Task | `TC-P38-T010` |
| Phase | P38 — Multi-Agency Commerce |
| Slice | Commerce Depth — Admin approval / access / policy stubs |
| Status | Cursor PASS (awaiting Architect) |

## Admin governance summary

- New Admin surface: `/api/agency-marketplace/moderation/offers`
  - `GET /pending` — Submitted queue (`Offers.Read`)
  - `POST /{id}/approve|reject|suspend` — (`Offers.Moderate`)
- Legacy panel `/approve|/reject` now route through the same governance service (self-guard included).
- Agency-owned `POST /offers/{id}/suspend` remains Write for operator unlist; Admin suspend is Moderate.
- Admin FE: `/[locale]/admin/agencies/offers` (UGC-moderation patterned island).

## Lifecycle assessment

| Actor | Creates / Submits | Approve / Reject | Publish (post-approve) | Suspend Published | Public consume |
|-------|-------------------|------------------|------------------------|-------------------|----------------|
| Agency (Write) | Yes | Forbidden (no Moderate + self-guard) | Yes (owned) | Yes (owned Write) | No |
| Admin (Moderate) | N/A | Yes | N/A (agency publish) | Yes (moderation path) | No |
| Public | No | No | No | No | Published + Listed + Active + Public only |

Ownership locked:

```text
Agency ≠ Admin
Admin Approval ≠ Agency Ownership
Published ≠ Automatically Booked
```

## Access boundary summary

- `agency.marketplace.offers.moderate` remains Admin-baseline only (not Agency Presentation baseline).
- Display name updated: review / approve / reject / suspend.
- Self-moderation denial: if the authenticated account resolves to an AgencyProfile that owns the offer, governance returns Forbid — even when Moderate is granted.
- Unauthorized / Agency-role callers receive 403 on moderation endpoints.

## Policy foundation

Extension-point interfaces (Allow stubs only — no money):

- `IAgencyOfferCommercialPolicy`
- `IAgencyOfferContentPolicy`
- `IAgencyOfferChannelPolicy`

`AgencyOfferPolicyContext` intentionally excludes Commission / Settlement / Payout fields.

## Compatibility

- Public eligibility still requires `Published` (+ Listed / Active / Public).
- Agency catalog ops (T007) unchanged for create / submit / publish / retire.
- Booking / Quote / Payment untouched.
- No commission, settlement, payout, or financial rules implemented.

## Tests

- Unit: `AgencyOfferGovernanceTests` (self-guard, suspend, policy stubs)
- Host: `AgencyOfferGovernanceAccessTests` (Agency 403; Admin queue/approve/suspend; self-approve Forbid)
- Architecture: Admin endpoints guardrail (moderate + no commission tokens)

## Explicit non-goals (honored)

- Commission
- Settlement
- Payout
- Financial reconciliation / commercial rules engines
