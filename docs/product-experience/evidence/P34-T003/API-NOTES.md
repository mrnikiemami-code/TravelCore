# TC-P34-T003 — API Notes (Sandbox Payment Provider)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P34-T003` |
| Nature | Non-production Sandbox `IPaymentProviderGateway` (Option B) |
| ProviderKey | `sandbox` |
| Production flag | `NamedProductionAdapterImplemented` remains **`false`** |

## Enable locally (Development)

In `src/backend/TravelCore.Api/appsettings.Development.json` (placeholder secret only):

```json
"Payment": {
  "Provider": { "DefaultProviderKey": "sandbox" },
  "Sandbox": {
    "Enabled": true,
    "HmacSecret": "DEV-ONLY-CHANGE-ME-sandbox-hmac-secret"
  }
}
```

Requirements:

- `ASPNETCORE_ENVIRONMENT` ∈ { `Development`, `Local`, `Staging` }
- `Payment:Sandbox:Enabled=true`
- Production: registration is impossible even if Enabled=true (fail-closed)

## Endpoints

| Method | Path | Role |
|--------|------|------|
| GET | `/api/payment/providers/sandbox/outcome` | NON-PRODUCTION outcome chooser UI (browser return ≠ success) |
| POST | `/api/payment/providers/sandbox/outcome` | Posts HMAC-signed callback to existing callback route |
| POST | `/api/payment/providers/sandbox/callback` | Existing Payment callback processor |

HMAC header: `X-TravelCore-Sandbox-Signature` = hex(HMAC-SHA256(secret, body))

## Boundary matrix (API evidence)

| Case | Expectation | Evidence |
|------|-------------|----------|
| Initiate (sandbox enabled, non-prod) | Redirect to outcome page; Attempt Initiated | Unit: `Sandbox_Initiate_Returns_Labeled_Outcome_Redirect` |
| Verified success callback | `IsVerified` + Succeeded outcome | Unit: `Sandbox_Verified_Success_Callback_Requires_Valid_Hmac` |
| Failed outcome | Failed verification outcome | Same HMAC path with `outcome=Failed` |
| Cancelled outcome | Maps to Failed (no new domain enum) | Unit: `Cancelled_Outcome_Maps_To_Failed_Without_New_Domain_Enum` |
| Tampered / unsigned callback | Unverified; no success | Unit: `Tampered_Or_Unsigned_Callback_Is_Unverified` |
| Duplicate verified success | Existing PaymentCallbackProcessor idempotency | Reuses P20 processor (no sandbox bypass) |
| Browser return alone | GET outcome page does not mutate Payment | Outcome page copy + `BrowserReturn != PaymentSuccess` |
| Confirm boundary | Sandbox never calls Booking Confirm | Adapter posts Payment callback only |
| Production host | Gate denies registration | Arch/Unit: `Production_Cannot_Register_Sandbox` |
| NamedProductionAdapterImplemented | Stays false | Arch/Unit + Contracts source |

## Tests run

```text
TravelCore.Modules.Payment.UnitTests --filter-class *PaymentSandboxProviderTests
  → Passed 16/16

TravelCore.ArchitectureTests --filter-class *PaymentSandboxProviderGuardrailTests
  → Passed 5/5

TravelCore.ArchitectureTests --filter-method *Payment_T003_ProviderNeutral_Trust_Boundary
  → Passed 1/1

TravelCore.ArchitectureTests --filter-method *Payment_T004_Idempotency_Is_Database_Backed
  → Passed 1/1
```

## Ownership preserved

`sandbox != production provider` · Payment owns adapter · Booking Confirm untouched · secrets not committed (Development placeholder only)
