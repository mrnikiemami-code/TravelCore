# Agency Presentation (P03 T011)

TravelCore **Agency Panel** is a presentation/capability surface only.

## Ownership

| Concern | Owner |
|---------|--------|
| Authentication | Identity (HttpOnly cookie) |
| Authorization | Access (`agency.panel.open`) |
| Acting business party | Party (`Kind = Agency`) |
| Tour / Pricing / Booking / Payment | **Not owned here** (future modules) |

## API

`GET /api/agency/panel/capabilities`

- `401` unauthenticated
- `403` authenticated without Access permission
- `409` missing/non-Agency associated Party
- `200` commerce-free capability stub

## Non-goals

- No marketplace
- No Tour/Booking/Payment implementation
- No copying domain services into Agency Panel
