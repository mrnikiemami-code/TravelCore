# P30 — Admin Console Experience Spec

| Field | Value |
|-------|--------|
| Document | `docs/product-experience/P30-ADMIN-EXPERIENCE-SPEC.md` |
| Status | **LOCKED** by `TC-P30-T002` |
| Audience | Operator |
| Identity | **Operational Command Center** |

---

## 1. What Admin is not

Admin must **NOT** be:

- a public-site restyle
- generic CRUD
- database table forms
- desktop-only
- an endless sidebar of entities
- a generic purchased dashboard template

---

## 2. Shell direction

- responsive sidebar / navigation
- topbar
- breadcrumb / context
- global or contextual search
- command / action affordance
- notification area when relevant
- user menu
- theme control
- contextual primary actions
- clear workspace hierarchy

---

## 3. Dashboard

Prioritize **actionable** information.

Possible patterns: KPI cards · pending work · operational alerts · recent activity · useful charts · quick actions

Charts must **not** exist merely for decoration.

---

## 4. Professional Data Grid contract

Locked requirement direction for major entity/operational collection views (implementation target: `TC-P30-T008`):

- server-side pagination
- type-aware filtering
- sorting
- global/local search
- column reorder · hide/show
- saved views
- row selection · bulk selection · bulk actions
- row-level actions
- export boundary (CSV / Excel where permitted)
- keyboard usability
- loading · error · empty states
- responsive behavior

### Mobile

Desktop table must **not** simply be squeezed.

Mobile operational views may transform into:

**Responsive Card / List Operational View**

while preserving filters · actions · essential fields · selection semantics where practical.

Do **not** implement the grid in T002.

---

## 5. Form / workflow standard

```text
Workflow over Database Form
```

Long administrative workflows must be decomposed by user intent.

Avoid:

- 70-field pages
- one giant Save for complex aggregate workflows

Example direction (Tour authoring — directional only):

Basic Information → Destination → Departure → Hotel → Transport → Pricing → Media → SEO → Review / Publish

Actual workflow must respect domain capabilities and task scope.

Forms must include: clear labels · descriptions when needed · validation near fields · unsaved-change behavior · loading · server error handling · accessibility · mobile usability · destructive action protection

---

## 6. Cross-cutting

- shares Design System with Public/Agency
- FA/EN/AR · RTL/LTR · Light/Dark
- Desktop / Tablet / Mobile independently reviewed
- no fake operational metrics

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial lock · `TC-P30-T002` |
