import type { ReactNode } from "react";

/**
 * Passthrough root layout.
 * Document ownership (`html` / `body`, `lang`, `dir`) lives under `app/[locale]/layout.tsx`
 * so attributes are derived from the URL locale segment (ADR 0007).
 */
export default function RootLayout({ children }: { children: ReactNode }) {
  return children;
}
