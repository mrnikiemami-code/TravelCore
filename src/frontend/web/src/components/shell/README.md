# Shell foundations (P30 T004)

Reusable **application shells** for Public, Admin, and Agency experiences.

| Component | Role |
|-----------|------|
| `PublicShell` | sticky header / context / **main** / footer host |
| `PublicHeader` | brand · primary nav · search entry · mobile menu |
| `PublicFooter` | trust / discovery footer links |
| `AdminShell` | topbar · breadcrumb · nav rail · workspace **main** |
| `AgencyShell` | sales-oriented chrome over AdminShell mechanics |

## Invariants

- Server Components by default — no `"use client"` in shell chrome
- Direction-neutral (logical CSS; no PublicShellRtl)
- Mobile-first (Admin/Agency nav stacks on narrow viewports)
- Owns `<main id="main-content">` — do not nest another `<main>`
- One Design System / Three Experiences — extend, do not fork
- Honest surfaces — no fake commerce facts

## Preview

`/[locale]/dev/shells` — visual board for architect review.
