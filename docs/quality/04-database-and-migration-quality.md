# Database and Migration Quality

منبع: [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md)  
Data architecture: [`../architecture/07-data-architecture.md`](../architecture/07-data-architecture.md) · [`../architecture/08-persistence-and-migrations.md`](../architecture/08-persistence-and-migrations.md)

---

## 1. Real PostgreSQL Direction

Tests that claim PostgreSQL behavior must exercise PostgreSQL-compatible behavior.

| Approach | Role |
|----------|------|
| Disposable PostgreSQL (containers / ephemeral / dedicated test DB) | Intended verification |
| EF Core InMemory | Not relational correctness proof |
| SQLite | Not authoritative for PG-specific types/constraints/collation/JSON/timezone/concurrency |
| `EnsureCreated` | Not evidence that production migrations are valid |

Accepted Data Architecture forbids treating EnsureCreated as migration lifecycle substitute.

---

## 2. Migration Task Completeness

A migration task is **not** complete merely because a migration file exists.

Relevant checks:

- migration builds  
- migration applies to **clean** database  
- model/migration state consistent  
- important constraints/indexes exist  
- existing-data implications reviewed  
- where applicable: upgrade from previous schema state validated  

Build/compile alone ≠ migration PASS.

---

## 3. Destructive Migrations

Require explicit review. Examples:

`DROP COLUMN` · `DROP TABLE` · type narrowing · non-null conversion · data rewrite · unique constraint introduction · large table transformation.

Do not hide destructive effects inside ordinary migration noise. Clean-DB apply success is **not sufficient** if production-compatible data would be destroyed without plan.

---

## 4. Migration Data Safety

When real production data may exist, review:

backfill · defaults · locking · duration · rollback/forward recovery · compatibility window.

Exact deployment strategy deferred; risk analysis is not.

---

## 5. Ownership & Boundaries

Migration quality includes:

- module-owned schemas/migrations  
- no cross-module ownership violation  
- no sneak-in cross-module FK where policy forbids  
- UUID v7 / Money precision / temporal mappings remain consistent with Accepted ADRs  

---

## 6. Practical Failures

| Situation | Result |
|-----------|--------|
| Migration never applied though required | NOT PASS |
| Applies on clean DB but destroys compatible data | NOT sufficient |
| PostgreSQL env unavailable for required gate | BLOCKED (not PASS) |
| InMemory green used as PG proof | FAIL (invalid evidence) |

---

## 7. Deferred

Exact testcontainers / ephemeral DB tooling · exact migration test harness layout · production rollout runbooks.
