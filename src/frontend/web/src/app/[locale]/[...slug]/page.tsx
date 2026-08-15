import { notFound } from "next/navigation";

/**
 * Catch unmatched paths under a valid locale so App Router renders
 * `[locale]/not-found.tsx` inside the locale layout (lang/dir preserved).
 * Required catch-all — does not compete with `[locale]/page.tsx` for `/fa`.
 */
export default function LocaleCatchAll(): never {
  notFound();
}
