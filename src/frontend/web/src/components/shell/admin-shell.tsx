import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

export type AdminShellProps = {
  /** Workspace top bar / title region. */
  header?: ReactNode;
  /**
   * Navigation SLOT only — may be empty.
   * Concrete Admin menu taxonomy is page-provided; shell hosts the region.
   */
  navigation?: ReactNode;
  /** Optional contextual actions (filters, primary page actions). */
  actions?: ReactNode;
  /** Optional breadcrumb strip under topbar. */
  breadcrumb?: ReactNode;
  /** Optional dense context line (operator hint / environment). */
  context?: ReactNode;
  /** Primary workspace content. */
  children: ReactNode;
  /**
   * When true, content region is a generic landmark (not `<main>`),
   * for embedded composition smoke inside another page shell.
   */
  embedded?: boolean;
  className?: string;
};

/**
 * ADMIN operational console shell — P30 T004 refined in T008.
 *
 * - Regions: topbar · breadcrumb · navigation · actions · content
 * - Mobile-first: collapsible nav; md+ start rail
 * - Dense, long-session workspace (not marketing spacing)
 * - Direction-neutral: logical `border-e`
 */
export function AdminShell({
  header,
  navigation,
  actions,
  breadcrumb,
  context,
  children,
  embedded = false,
  className,
}: AdminShellProps) {
  const ContentTag = embedded ? "div" : "main";

  return (
    <div
      className={cn(
        "flex min-h-full flex-1 flex-col bg-surface-muted text-foreground",
        className,
      )}
    >
      <header className="sticky top-0 z-30 border-b border-border bg-surface/95 shadow-sm backdrop-blur">
        <div className="flex w-full flex-wrap items-center gap-2 px-3 py-2.5 sm:gap-3 sm:px-4">
          <div className="min-w-0 flex-1">
            {header ? (
              <div className="truncate text-sm font-semibold tracking-tight text-primary sm:text-base">
                {header}
              </div>
            ) : (
              <div className="text-sm font-semibold text-primary">TravelCore Admin</div>
            )}
            {context ? (
              <div className="mt-0.5 text-[11px] text-muted-foreground sm:text-xs">
                {context}
              </div>
            ) : null}
          </div>
          {actions ? (
            <div className="flex flex-wrap items-center gap-1.5 sm:gap-2">{actions}</div>
          ) : null}
        </div>
        {breadcrumb ? (
          <div className="border-t border-border bg-surface-muted/80 px-3 py-1.5 text-[11px] text-muted-foreground sm:px-4 sm:text-xs">
            {breadcrumb}
          </div>
        ) : null}
      </header>

      <div className="flex flex-1 flex-col md:flex-row">
        <aside
          aria-label="Navigation"
          className="w-full shrink-0 border-b border-border bg-surface md:w-56 md:border-b-0 md:border-e lg:w-64"
        >
          {/* Mobile: collapse nav for denser workspace */}
          <details className="group md:hidden">
            <summary className="flex min-h-touch cursor-pointer list-none items-center justify-between px-3 py-2 text-sm font-medium marker:content-none [&::-webkit-details-marker]:hidden">
              <span>منو / Menu</span>
              <span className="text-xs text-muted-foreground group-open:hidden">▾</span>
              <span className="hidden text-xs text-muted-foreground group-open:inline">▴</span>
            </summary>
            <div className="border-t border-border px-2 py-2">{navigation ?? null}</div>
          </details>
          <div className="hidden px-2 py-3 md:block">{navigation ?? null}</div>
        </aside>

        <ContentTag
          {...(embedded
            ? { role: "region", "aria-label": "Workspace" }
            : { id: "main-content", tabIndex: -1 })}
          className={cn(
            "flex min-w-0 flex-1 flex-col bg-background",
            !embedded && "outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring",
          )}
        >
          {children}
        </ContentTag>
      </div>
    </div>
  );
}
