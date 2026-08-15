# `lib/api` — Frontend API / read-model boundary (T009)

Server-first access to **application/API contracts** and **page/workflow view models**.

## Layout

| Path | Role |
|------|------|
| `config.ts` | Env-based API base URL (`TRAVELCORE_API_BASE_URL` / `API_BASE_URL`) |
| `client.ts` | `apiGetJson` — server `fetch` + Problem Details aware failures |
| `result.ts` | `ApiResult` helpers (`ok` / fail kinds) |
| `read-models.ts` | Page / Workflow view-model brands & factories |
| `fixtures/` | Minimal non-product fixtures for boundary proof |

## Rules

- Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model
- No DbContext / EF / SQL / backend entity imports
- No OpenAPI codegen in P02 by default
- No pricing/booking/payment authority in this layer
- Cache/revalidate is **caller-explicit** via `ServerFetchOptions`
- Correlation: optional `X-Correlation-ID` request/response (P01 header)

## Cross-domain readiness (T010)

`WorkflowViewModel` may compose multiple module-originated read models for one
user goal without merging domain ownership. Coordination stays in explicit
application/API contracts — not UI→DB joins.
