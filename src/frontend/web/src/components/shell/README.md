# Shell foundations (T008)

Reusable **layout mechanics** for Public and Admin experiences.

| Component | Role |
|-----------|------|
| `PublicShell` | header / context / **main** / footer |
| `AdminShell` | header / **navigation slot** / actions / **main** |

## Invariants

- Server Components only — no `"use client"`
- Direction-neutral (logical CSS; no PublicShellRtl / AdminShellRtl)
- Mobile-first (Admin nav is a slot, not a permanent desktop sidebar)
- Owns `<main id="main-content">` — do not nest another `<main>`
- **Admin navigation IA is UNDECIDED** until `TC-P02-T010`
- No domain→menu mapping; no Identity/Party/Tour menu trees

## Cross-domain compatibility

`AdminShell` exposes an empty **navigation slot**. Future workflow-driven IA from T010 can fill that slot without changing shell structure or mirroring backend modules.
