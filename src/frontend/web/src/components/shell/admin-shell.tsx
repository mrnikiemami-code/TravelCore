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
 * ADMIN console workspace shell — P30 T004.
 *
 * - Regions: topbar · breadcrumb · navigation · actions · content
 * - Mobile-first: navigation stacks above content; md+ inline-start rail
 * - Direction-neutral: logical `border-e`
 *
 * Landmark ownership: owns `<main id="main-content">` when not embedded.
 */
export function AdminShell({
  header,
  navigation,
  actions,
  breadcrumb,
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
      {header || actions ? (
        <header className="sticky top-0 z-20 border-b border-border bg-surface shadow-sm">
          <div className="flex w-full flex-wrap items-center gap-3 px-4 py-3">
            <div className="min-w-0 flex-1">
              {header ? (
                <div className="text-sm font-semibold text-primary md:text-base">
                  {header}
                </div>
              ) : null}
            </div>
            {actions ? (
              <div className="flex flex-wrap items-center gap-2">{actions}</div>
            ) : null}
          </div>
          {breadcrumb ? (
            <div className="border-t border-border bg-surface-muted px-4 py-2 text-xs text-muted-foreground">
              {breadcrumb}
            </div>
          ) : null}
        </header>
      ) : null}

      <div className="flex flex-1 flex-col md:flex-row">
        <aside
          aria-label="Navigation"
          className="w-full shrink-0 border-b border-border bg-surface md:w-64 md:border-b-0 md:border-e"
        >
          <div className="px-3 py-3">{navigation ?? null}</div>
        </aside>

        <ContentTag
          {...(embedded
            ? { role: "region", "aria-label": "Workspace" }
            : { id: "main-content", tabIndex: -1 })}
          className={cn(
            "flex min-w-0 flex-1 flex-col bg-background",
            !embedded && "outline-none",
          )}
        >
          {children}
        </ContentTag>
      </div>
    </div>
  );
}
