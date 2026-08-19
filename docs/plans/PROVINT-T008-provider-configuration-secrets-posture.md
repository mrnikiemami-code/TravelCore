# PROVINT-T008 — Provider Configuration & Secrets Posture

**Status:** Operations / integration guidance (manual verification)

---

## Configuration principles

1. **Secrets never in repository** — API keys, merchant IDs, webhook secrets live in secure host configuration only.
2. **Module-owned registration** — Payment, HotelBooking, and Flight each register their own adapters; Evolution does not own vendor SDKs.
3. **Disabled-by-default** — empty provider/source lists and `Enabled=false` descriptors are valid production posture until architect lock.
4. **Named provider = NONE** — boundary constants remain NONE until explicit architect acceptance + ADR when required.

## Host configuration sections (when adapters exist)

| Module | Typical section | Notes |
|--------|-----------------|-------|
| Payment | `Payment:Providers:*` | ProviderKey → enabled + capability flags |
| HotelBooking | `HotelBooking:Sources:*` | SourceKey → capabilities |
| Flight | `Flight:Sources:*` | SourceKey → capabilities |

## Pre-adapter launch checklist

- [ ] No vendor SDK packages in product projects (architecture guardrails PASS)
- [ ] `PaymentProviderTrustBoundary.NamedProviderSelected = NONE`
- [ ] `HotelSourceReadinessBoundary.NamedHotelSupplier = NONE`
- [ ] `FlightOwnershipBoundary.NamedFlightSupplier = NONE`
- [ ] Sandbox credentials isolated from production secrets store
- [ ] Callback URLs registered per environment (Payment module)

## Out of scope for repository CI

- Live sandbox credential wiring (PROVINT-T015)
- Named vendor selection (PROVINT-T014)
