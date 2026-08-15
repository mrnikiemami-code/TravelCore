# Module-Local Transactional Outbox

وضعیت: Active (`TC-P01-T014`)

## Authority

ADR 0001 · ADR 0002 (async / Outbox direction) · NodaTime (`24-…`) · UUID v7 (`23-…`) · DbContext/migrations proofs (`27-…`, `28-…`).

## Ownership rule

A persistent module owns:

- its DbContext
- its schema
- its migrations
- its **module-local Outbox** storage

Business-state change and Outbox insert are intended to commit in **one module transaction** (same DbContext / `SaveChanges` boundary).

There is **no**:

- global / shared Outbox table
- `GlobalOutboxDbContext`
- message broker in P01
- background dispatcher in T014
- `SaveChangesInterceptor` / automatic domain-event harvesting
- Inbox (separate future concern)

## Fixture proof (non-production)

```text
PersistenceFixtureDbContext
  schema: p01_fixture
  PersistenceProbe
  PersistenceFixtureOutboxMessage  →  p01_fixture.outbox_messages
```

| Field | Model | Store |
|-------|--------|--------|
| Id | `Guid` (explicit; UUID v7-compatible) | uuid PK |
| OccurredAt | `Instant` | `timestamp with time zone` |
| MessageType | semantic string (not CLR AQN) | required, max 256 |
| Payload | JSON text | **jsonb** |
| ProcessedAt | `Instant?` | nullable timestamptz |

T014 proves **structural** same-DbContext / same-schema ownership. Real PostgreSQL atomicity → deferred (`TC-P01-T016` / integration gate).

## Delivery semantics

Exactly-once delivery is **not** promised. Future dispatch may publish duplicates; consumers must be idempotent where required.

## Production pattern (future)

```text
TravelCore.Modules.<Module>.Infrastructure
  => <Module>DbContext
  => <module-schema>
  => module entities + module-local outbox
  => module-owned migrations
```

Cross-module workflows do **not** share one EF transaction; each module owns its state + Outbox, then async follow-on.
