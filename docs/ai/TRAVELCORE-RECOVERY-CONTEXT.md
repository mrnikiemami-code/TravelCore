# TravelCore Recovery Context

**P39 — Multi-Agency Commercial Finance Foundation** (`TC-P39-T006` Cursor **PASS** · Contracts + persistence skeleton)

## Completed (recent)

- P39-T001 ACCEPTED — Commission/Settlement/Payout boundary plan
- P39-T002 ACCEPTED — Commercial Finance domain vocabulary
- P39-T003 ACCEPTED — Commercial Obligation lifecycle + evidence boundaries
- P39-T004 ACCEPTED — Business decision intake (Q1–Q38)
- P39-T005 ACCEPTED — Locked finance policy decisions + readiness matrix
- P39-T006: Commercial Finance module skeleton — schema `commercial_finance`, entities, lifecycle guards, Access permissions, admin read endpoints

## Current Authorized Work

**None** — WAITING for Architect after T006 RESULT. Next expected: business-rule resolution or engine envelopes when authorized.

## Locked business policy (summary)

```text
Commission: AGENCY_SPECIFIC_WITH_OFFER_OVERRIDE · base PAID_AMOUNT · trigger PAYMENT_SUCCEEDED
Refund: PRE_SETTLEMENT_CANCEL · PROPORTIONAL_PARTIAL_REFUND · POST_SETTLEMENT_REVERSAL
Settlement: AGENCY_CONFIGURABLE_CADENCE · ADMIN_APPROVAL · OFFSET_FUTURE_PAYABLES
Currency: SOURCE_OBLIGATION + SETTLEMENT_CONVERSION · CONFIGURABLE_FX
Tax/Invoice: MARKET_CONFIGURABLE (Iran/UAE rules still UNKNOWN)
Payout: SEMI_AUTOMATED_ADMIN_APPROVED
Market: SHARED_CORE_WITH_MARKET_SPECIFIC_POLICIES
```

## Revision

| Date | Change |
|------|--------|
| 2026-08-22 | Sync after TC-P39-T006 Commercial Finance contracts + persistence skeleton |
| 2026-08-22 | Sync after TC-P39-T005 Commercial Finance decisions locked |
| 2026-08-22 | Sync after TC-P39-T004 Commercial Finance business decision intake |
| 2026-08-22 | Sync after TC-P39-T003 Commercial Obligation lifecycle foundation |
