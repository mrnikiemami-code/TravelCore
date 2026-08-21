# TC-P34-T004 — API Notes (Tour UX ↔ Sandbox)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P34-T004` |
| Nature | Wire Tour public booking/payment UX to non-production Sandbox (T003) |
| ProviderKey | `sandbox` |
| Production flag | `NamedProductionAdapterImplemented` remains **`false`** |

## Enable locally (Development)

1. PostgreSQL: `Host=localhost;Port=5432;Database=TravelCore;Username=admin;Password=123456`
2. Apply **existing** Payment EF migrations if `payment` schema missing:
   ```text
   dotnet ef database update \
     --project src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure \
     --connection "Host=localhost;Port=5432;Database=TravelCore;Username=admin;Password=123456"
   ```
3. `src/backend/TravelCore.Api/appsettings.Development.json`:
   ```json
   "Payment": {
     "Provider": { "DefaultProviderKey": "sandbox" },
     "Sandbox": {
       "Enabled": true,
       "HmacSecret": "DEV-ONLY-CHANGE-ME-sandbox-hmac-secret"
     }
   }
   ```
4. API: `ASPNETCORE_ENVIRONMENT=Development`, `ConnectionStrings__TravelCore=…`, listen `:5275`
5. Next: `TRAVELCORE_API_BASE_URL=http://localhost:5275` on `:3000`

## Availability detection (no hardcode)

Public payment compose/read returns `safeAction`:

| `safeAction` | UX |
|--------------|-----|
| `Initiate` / `Retry` | Show labeled Sandbox CTA |
| `Unavailable` | Option A honest stop (no pay CTA) |
| `Wait` | Waiting copy; no new initiate |
| `Succeeded` | Server payment success; booking status from Booking read |

## Live E2E results (2026-08-21 local)

Demo departure: `demofeed-tour-teh-1` / `01a02414-65a9-7d3d-90f4-9b6179d3f0db`

| Flow | Booking | Payment | Booking after Confirm path | Notes |
|------|---------|---------|----------------------------|-------|
| Success | `01a02591-83fc-7a49-8f23-518f33956783` | `Succeeded` · `safeAction=Succeeded` | `status=Confirmed` after PaymentSuccess outbox (~1 min) | ConfirmIfEligible ran; inbox row present; no recovery issue |
| Failure | `01a02594-a368-7710-a3e7-21ea2c09f26c` | `Pending` · attempt `Failed` · `safeAction=Retry` | Stays Pending | Sandbox outcome `Failed` |
| Cancel | `01a02594-a38d-7aef-8930-703ab533e117` | `Pending` · attempt `Failed` · `safeAction=Retry` | Stays Pending | Cancelled maps to Failed (no new domain enum) |

### API evidence samples

**GET payment (Initiate available):**

```json
{"bookingStatus":"Pending","paymentStatus":"Pending","providerInitiationPossible":true,"safeAction":"Initiate"}
```

**POST initiation → redirect:**

```text
redirectUri=http://localhost:5275/api/payment/providers/sandbox/outcome?...
safeAction=Wait · latestAttemptStatus=Initiated
```

**After verified Success + outbox dispatch:**

```json
{"bookingStatus":"Confirmed","paymentStatus":"Succeeded","safeAction":"Succeeded","bookingConfirmed":false}
```

Note: public `bookingConfirmed` / `confirmed` boolean remains hardcoded `false` in `PublicBookingMapper` (pre-existing P20 surface). UI uses **`bookingStatus` / `status` string** for truth (`Confirmed`).

## Frontend changes

- Restored `PublicBookingPaymentView` read/initiate actions
- Status view loads payment read; CTA only when `Initiate`/`Retry`
- FA/EN/AR copy: «Sandbox payment — non-production»
- Browser return route still states return ≠ success

## Ownership preserved

No Payment bypass · no fake Confirm · sandbox ≠ production · `NamedProductionAdapterImplemented` untouched
