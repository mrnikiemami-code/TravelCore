# `lib/formatting`

Presentation formatting helpers (display-only).

## Money (`money.ts`)

- Formats `MoneyView` amounts with `Intl` / locale **digits only**
- Explicit IRR ↔ Toman **display denomination** (`1 Toman = 10 IRR`) when caller sets `irrDisplayUnit: "Toman"`
- **No** FX rates, **no** cross-currency arithmetic, **no** locale→currency selection

Authoritative money/time calculations remain outside UI.
