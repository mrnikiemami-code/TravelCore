import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

export type PublicShellProps = {
  /** Optional top chrome (branding/nav later — not decided here). */
  header?: ReactNode;
  /** Optional breadcrumb / context strip. */
  context?: ReactNode;
  /** Primary page content — rendered inside the sole <main>. */
  children: ReactNode;
  /** Optional footer chrome. */
  footer?: ReactNode;
  className?: string;
};

/**
 * Generic PUBLIC page shell — structure only (T008).
 *
 * Landmark ownership: this shell owns `<main id="main-content">`.
 * Pages using PublicShell must NOT nest another `<main>`.
 * SkipLink in locale layout targets `#main-content`.
 *
 * Direction-neutral · mobile-first · Server Component.
 */
export function PublicShell({
  header,
  context,
  children,
  footer,
  className,
}: PublicShellProps) {
  return (
    <div className={cn("flex min-h-full flex-1 flex-col bg-background text-foreground", className)}>
      {header ? (
        <header className="border-b border-border bg-surface">
          <div className="mx-auto w-full max-w-wide px-4 py-3">{header}</div>
        </header>
      ) : null}

      {context ? (
        <div className="border-b border-border bg-surface-muted">
          <div className="mx-auto w-full max-w-wide px-4 py-2">{context}</div>
        </div>
      ) : null}

      <main
        id="main-content"
        tabIndex={-1}
        className="flex flex-1 flex-col outline-none"
      >
        {children}
      </main>

      {footer ? (
        <footer className="border-t border-border bg-surface">
          <div className="mx-auto w-full max-w-wide px-4 py-4">{footer}</div>
        </footer>
      ) : null}
    </div>
  );
}
