# `components/ui`

Shared **direction-neutral UI primitives** for TravelCore web (T004).

## Primitives

| Primitive | Role |
|-----------|------|
| `Container` | max-width content wrapper (`narrow` / `content` / `wide` / `full`) |
| `Stack` | vertical composition via `gap` |
| `Inline` | horizontal composition; flex main-start follows document direction |
| `Surface` | neutral bordered surface using semantic tokens |
| `Text` | semantic typography roles (`display` … `caption` / `muted`) |
| `BidiText` | `<bdi>` isolation; `dir="auto" \| "ltr" \| "rtl"` |
| `LtrValue` | convenience for known LTR identifiers (codes, emails, refs) |
| `MoneyText` | single Money presentation (display-only; ADR 0003) |
| `MixedCurrencyPrice` | multiple supplied components; no FX / no silent sum |
| `SkipLink` | keyboard skip-to-main (`#main-content`) |
| `FieldMessage` / `VisuallyHidden` | form help/error association + SR-only text |

See also: [`ACCESSIBILITY.md`](./ACCESSIBILITY.md) (T006 baseline).


## Rules

- Server Component by default — **no** `"use client"` in this layer
- No business/domain logic (Identity, Party, Tour, Booking, …)
- Logical spacing/alignment only (`gap`, `mx-auto`, `px-*`, `text-start` if needed) — avoid `ml`/`mr`/`pl`/`pr`/`left`/`right`
- Consume T003 semantic tokens (`bg-surface`, `text-muted-foreground`, …)
- Bidi: isolate content; **never** reverse strings or invent visual order

## Example

```tsx
import { Container, Stack, Text, LtrValue } from "@/components/ui";

<Container>
  <Stack>
    <Text role="heading">پرواز</Text>
    <Text role="body">
      کد: <LtrValue>EK978</LtrValue>
    </Text>
  </Stack>
</Container>
```
