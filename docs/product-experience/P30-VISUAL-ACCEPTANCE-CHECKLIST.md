# P30 — Visual Acceptance Checklist

| Field | Value |
|-------|--------|
| Document | `docs/product-experience/P30-VISUAL-ACCEPTANCE-CHECKLIST.md` |
| Status | **LOCKED** by `TC-P30-T002` |
| North Star | [`assets/travelcore-ui-ux-north-star.png`](assets/travelcore-ui-ux-north-star.png) |
| Constitution | [`TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md`](TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md) |

---

## 1. Mandatory Pipeline behavior for major UI tasks

```text
Cursor Implementation
        ↓
Automated Validation
        ↓
Required Screenshot Evidence
        ↓
Architect / User Visual Review
        ↓
ACCEPT or REWORK
        ↓
Only then next major visual task
```

Major P30 UI tasks **MUST NOT** receive final architecture acceptance solely from:

- build PASS · tests PASS · lint PASS · typecheck PASS · Lighthouse PASS

**Visual evidence is mandatory.**

---

## 2. Required evidence (minimum)

Representative screenshots:

- **Desktop**
- **Mobile**
- **Tablet** when breakpoint-specific experience materially changes

For key surfaces, include relevant states when visually significant:

- Loaded
- Empty (if important)
- Error (if visually significant)

Screenshot automation need not be built in T002 — later tasks implement a practical evidence mechanism.

Placeholder/demo content in acceptance screenshots must be **disclosed**.

---

## 3. Acceptance dimensions

Review at least:

| # | Dimension | Question |
|---|-----------|----------|
| A | Product feel | Does this look like a mature commercial travel product? |
| B | Visual hierarchy | Can the user immediately understand what this page is, what is important, and what action comes next? |
| C | Travel imagery | Meaningful · high quality · well cropped · appropriately prominent? |
| D | Composition | Sections intentionally composed (not arbitrary stacks)? |
| E | Typography | Strong hierarchy · Persian highly readable? |
| F | Spacing | Intentional density · no giant unexplained whitespace · no cramped blocks? |
| G | Conversion | Primary CTA obvious without being aggressive? |
| H | Trust | Real trust signals visible where supported (no fabrication)? |
| I | Responsive quality | Mobile intentionally designed (not desktop shrinkage)? |
| J | RTL / LTR | Direction correct · bidi-safe technical values? |
| K | Accessibility | Visual design preserves accessibility? |
| L | Domain truth | UI represents real domain facts without fabrication? |
| M | Design System | Surface reuses shared patterns? |
| N | North Star regression | Materially below North Star professional/product quality? |

---

## 4. Rework rule

If visual review fails:

```text
Status = REWORK_REQUIRED
```

Cursor must receive a scoped REWORK envelope.

Do **not** proceed to the next major visual task because automated tests pass.

An actual visual rejection **is** a legitimate Pipeline stop/rework condition.

---

## 5. User-triggered visual checkpoints

User may request at any point:

«ببینیم الان چه شکلی شده» (or equivalent)

That becomes a **VISUAL REVIEW CHECKPOINT**:

1. stop advancing major visual work
2. obtain representative screenshots
3. compare against North Star / Constitution
4. identify regressions
5. issue rework if necessary

This is an intentional Product Review mechanism — not architecture indecision.

---

## 6. P30 checkpoint map

| Checkpoint | After |
|------------|--------|
| A | T003 Design System 2.0 primitives / component board |
| B | T004 Application shells |
| C | T005 Public Home |
| D | T006 Hotel commerce |
| E | T007 Tour commerce |
| F | T008 Admin foundation |
| G | T009 Agency foundation |

Then `TC-P30-GATE`.

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial lock · `TC-P30-T002` |
