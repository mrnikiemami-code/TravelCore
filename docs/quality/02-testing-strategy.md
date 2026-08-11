# Testing Strategy — TravelCore

منبع: [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md)

Exact frameworks/packages deferred to P01/P02.

---

## 1. Portfolio (Not Rigid Pyramid Numbers)

Direction:

- many fast focused tests  
- fewer integration tests  
- small number of high-value E2E journeys  
- architecture + contract tests supplement  

Choose test type by **risk**, not by habit.

---

## 2. Unit Tests

**Protect:** domain rules · value objects · pure application logic · policies · state transitions · edge cases.

**Traits:** fast · isolated · deterministic.

Do **not** write tests that merely mirror implementation line-by-line.

Prefer:

```text
Given business state
When operation occurs
Then observable rule holds
```

over mocking every private collaborator / asserting call order / private shape.

Tests should permit safe refactoring.

### Domain rule examples (future)

- Booking cannot accept expired Quote  
- TourDeparture capacity stays valid  
- Money components preserve currency  
- Translation publication requires required localized content  

---

## 3. Integration Tests

Validate real boundaries: PostgreSQL persistence · EF mappings · handlers · HTTP/API pipeline · serialization · module contracts · Outbox · authn/authz integration.

Do **not** mock the component under verification.

### Database

Claims about PostgreSQL behavior require PostgreSQL-compatible execution.  
EF Core InMemory is **not** relational proof.  
SQLite differs (types, constraints, collation, concurrency, JSON, timezone) — not authoritative for PG-specific behavior.

Disposable PostgreSQL (containers / ephemeral DBs) is the intended direction. Tooling deferred.

---

## 4. Contract Tests

Protect semantic compatibility between frontend/backend, modules, and external providers where breakage risk exists.

Do not overuse snapshots as sole contract validation. Changed snapshots require interpretation — not auto-correctness.

---

## 5. Frontend Component & Interaction

**Component:** states · semantics · user-visible conditions · a11y interaction — not every Tailwind class.

**Interaction:** user-observable behavior for PassengerPicker · DateRangePicker · filters · Dialog/Sheet · booking initiation.

---

## 6. E2E

Protect critical journeys (localized nav · tour discovery/detail · booking initiation · auth · payment later). Do not cover every minor branch with expensive E2E.

### Journey examples (future)

Discover Destination → Tour → price → booking start · Search → filter → result · Locale change → equivalent localized route.

---

## 7. Failure Paths

Required quality includes: provider timeout · payment failure · expired Quote · DB conflict · missing translation · partial page composition failure · invalid authorization.

Happy-path-only is incomplete.

---

## 8. Determinism

Avoid uncontrolled: current clock · randomness · network · execution order · machine locale/timezone — unless intentionally under test.

### Time

TravelCore is time-heavy. Control Instant / LocalDate / timezone / expiry / departure / quote validity. Do not scatter uncontrollable `DateTime.Now` in domain/tests. Clock abstraction implementation later.

### Randomness

Use seeds / controlled generators when randomness is needed.

---

## 9. Isolation & Order Independence

Tests must not depend on execution order. One test must not secretly prepare global state for another.

Integration tests isolate DB / filesystem / network / config where practical. Parallelism depends on isolation.

---

## 10. Flaky Tests

Flaky tests are defects. Do **not** normalize «rerun until green» as acceptance. Investigate · fix · or quarantine with tracked justification. Quarantine mechanics deferred.

---

## 11. Concurrency & Idempotency

Where risk exists: capacity · payment idempotency · slug uniqueness · inventory · optimistic concurrency. Sequential unit tests alone are insufficient for concurrency invariants.

Idempotent commands/events: test duplicate delivery / retry (Outbox, Inbox, payment callbacks, webhooks, booking).

---

## 12. External Providers

Layered: adapter unit · contract/fixture · sandbox/integration · limited live when permitted.  
Ordinary CI must **not** depend on unstable production provider networks.

Fixtures: no secrets · no unnecessary real PII · intentional versioning · provider/version labeled. Raw provider JSON ≠ TravelCore domain model.

---

## 13. Coverage & Mutation

No universal coverage percentage in this constitution. Critical behavior coverage matters more. Risk-based thresholds may come later.

Mutation testing not globally required; may later suit critical pure domain logic.

---

## 14. Naming & Structure

Communicate behavior clearly. Exact naming convention and test project layout deferred to P01/P02.

---

## 15. Candidate Suites (Direction)

### Data quality

UUID v7 · Money decimal · IRR/Toman boundary · mixed PriceComponents · translation uniqueness · schema ownership · no forbidden cross-module FK · optimistic concurrency · migration history ownership.

### Temporal

Instant/LocalDate/LocalTime round-trip · IANA · DST-sensitive conversion · calendar formatting independence. Do not replace NodaTime with DateTime tests.

### i18n

`/fa` fa+RTL · `/en` en+LTR · `/ar` ar+RTL · URL locale authoritative · missing EN ≠ Persian under `/en` · language switch to equivalent slug · UI fallback ≠ entity publication fallback.

### SEO

canonical · old slug redirect · no redirect loops · noindex ∉ sitemap · filter URL ≠ uncontrolled landing · published locale controls hreflang · expired Tour semantics · 404 vs unavailable.

### Page archetype (Foreign Tour)

Desktop · Mobile · FA RTL · EN LTR with IKA/IST/TK875/USD/mixed currencies · states: active · unavailable · expired · partial failure.
