# Money and Currency Foundation

وضعیت: Active (`TC-P01-T010`)

فیزیکی:

```text
src/backend/Platform/Money/TravelCore.Money/
```

## Authority

ADR 0003 remains authoritative.

## Model

`Money` = `decimal` Amount + explicit `CurrencyCode`.

- Currency is always explicit (no universal implicit default).
- Amount uses `decimal` — never `float` / `double`.
- Money may be negative; business rules that forbid negatives live outside this primitive.
- Money does **not** round automatically by currency; no minor-unit / decimal-place registry in this foundation.
- Same-currency add/subtract may proceed; different currencies fail explicitly.
- No implicit FX / exchange-rate model.
- Money ≠ Pricing (no PriceList, Quote, discount, tax, commission, BuyBox, etc.).

## CurrencyCode

- Immutable identity, not a complete currency catalog.
- Canonicalization: trim, invariant uppercase, length 3–12, ASCII letters A–Z only.
- Extensible string-backed primitive — **not** a closed enum of known currencies.
- Examples that must be representable: `IRR`, `USD`, `EUR`, `USDT`.

## IRR / Toman

- **IRR** is the canonical stored/business currency for Iranian Rial amounts.
- **Toman** is a presentation/input denomination: `1 Toman = 10 IRR`.
- Do **not** use `TOMAN` as a canonical `CurrencyCode`.
- No `TomanMoney` type; no automatic Toman conversion inside Money arithmetic.
- Future Toman conversion belongs at an explicit presentation/input boundary.

## Non-goals (deferred)

- Custom System.Text.Json Money contract (wait for real API DTOs)
- EF Core / Npgsql / ValueConverter / migrations (persistence later; ADR 0003 still governs future `numeric` precision)
- Currency metadata (names, symbols, minor digits) — ReferenceData / presentation later
- Exchange rates, Pricing engines, formatting / localization

## Host impact

T010 does not wire Money into `TravelCore.Api`. The library is a pure technical primitive for future Domain / Application consumers.
