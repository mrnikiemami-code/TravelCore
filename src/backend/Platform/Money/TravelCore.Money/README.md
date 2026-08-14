# TravelCore.Money

Money and `CurrencyCode` primitives (ADR 0003).

- `Money` = `decimal` amount + explicit `CurrencyCode`
- Same-currency add/subtract only; cross-currency fails explicitly
- No FX, Pricing, Toman helpers, or persistence mapping in this foundation
