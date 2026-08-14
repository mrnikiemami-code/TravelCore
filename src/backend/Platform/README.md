# Platform (technical foundations)

Narrow home for **genuine cross-module technical foundations** that are not business modules.

Examples of capabilities that may land here in later P01 tasks:

- Modularity (composition contract) — T002
- ApiFoundation (Problem Details / JSON / OpenAPI) — T003–T004
- Configuration (explicit Options helpers) — T005
- Health (liveness / readiness) — T006
- Observability (logging / correlation) — T007
- Time (NodaTime)
- Money
- Identifiers (UUID v7)
- Observability helpers

## Rules

- This is **not** a generic shared dumping ground.
- Do **not** create `SharedKernel` / `Common` / giant catch-all libraries for arbitrary shared code.
- Prefer capability-oriented folders/projects over vague “shared” buckets.
- Business domain types do **not** belong here.
- Dependency direction: modules may use Platform technical primitives; Platform must not depend on business modules.

No Platform implementation projects are created in T001 — only the physical convention.
