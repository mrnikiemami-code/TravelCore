import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

export type PublicShellProps = {
  /** Optional top chrome (branding/nav). Prefer PublicHeader. */
  header?: ReactNode;
  /** Optional breadcrumb / context strip. */
  context?: ReactNode;
  /** Primary page content — rendered inside the sole <main>. */
  children: ReactNode;
  /** Optional footer chrome. Prefer PublicFooter. */
  footer?: ReactNode;
  /**
   * When true, content region is a generic landmark (not `<main>`),
   * for embedded composition boards.
   */
  embedded?: boolean;
  className?: string;
};

/**
 * PUBLIC marketplace page shell — P30 T004 visual chrome host.
 *
 * Landmark ownership: this shell owns `<main id="main-content">` when not embedded.
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
  embedded = false,
  className,
}: PublicShellProps) {
  const ContentTag = embedded ? "div" : "main";

  return (
    <div
      className={cn(
        "flex min-h-full flex-1 flex-col bg-background text-foreground",
        className,
      )}
    >
      {header ? (
        <header className="sticky top-0 z-20 border-b border-border bg-surface/95 shadow-sm backdrop-blur supports-[backdrop-filter]:bg-surface/80">
          <div className="mx-auto w-full max-w-wide px-4 py-3">{header}</div>
        </header>
      ) : null}

      {context ? (
        <div className="border-b border-border bg-surface-muted">
          <div className="mx-auto w-full max-w-wide px-4 py-2">{context}</div>
        </div>
      ) : null}

      <ContentTag
        {...(embedded
          ? { role: "region", "aria-label": "Public content" }
          : { id: "main-content", tabIndex: -1 })}
        className={cn("flex flex-1 flex-col", !embedded && "outline-none")}
      >
        {children}
      </ContentTag>

      {footer ? (
        <footer className="mt-auto border-t border-border bg-surface-muted">
          <div className="mx-auto w-full max-w-wide px-4 py-8">{footer}</div>
        </footer>
      ) : null}
    </div>
  );
}
