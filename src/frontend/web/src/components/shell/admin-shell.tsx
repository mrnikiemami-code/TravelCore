import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

export type AdminShellProps = {
  /** Workspace top bar / title region. */
  header?: ReactNode;
  /**
   * Navigation SLOT only — may be empty.
   * Job-based workflow/navigation model: docs/ui/06-cross-domain-workflow-and-navigation.md (T010).
   * Concrete Admin menu taxonomy remains undecided — do not pass durable domain-mirrored trees.
   */
  navigation?: ReactNode;
  /** Optional contextual actions (filters, primary page actions). */
  actions?: ReactNode;
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
 * Generic ADMIN workspace shell — layout mechanics only (T008).
 *
 * - Provides regions/slots: header · navigation · actions · content
 * - Does NOT freeze concrete Admin menu items; hosts T010 job-based nav when provided
 * - Mobile-first: navigation is a block region, not a permanent desktop-only sidebar
 * - Direction-neutral: uses logical `border-e`, no left/right API
 *
 * Landmark ownership: this shell owns `<main id="main-content">` when used as
 * the page root chrome. Do not nest another `<main>`.
 */
export function AdminShell({
  header,
  navigation,
  actions,
  children,
  embedded = false,
  className,
}: AdminShellProps) {
  const ContentTag = embedded ? "div" : "main";

  return (
    <div
      className={cn(
        "flex min-h-full flex-1 flex-col bg-background text-foreground",
        className,
      )}
    >
      {header ? (
        <header className="border-b border-border bg-surface">
          <div className="flex w-full flex-wrap items-center gap-3 px-4 py-3">
            <div className="min-w-0 flex-1">{header}</div>
            {actions ? (
              <div className="flex flex-wrap items-center gap-2">{actions}</div>
            ) : null}
          </div>
        </header>
      ) : null}

      <div className="flex flex-1 flex-col md:flex-row">
        {/*
          Navigation SLOT — structural only.
          On narrow viewports stacks above content (no permanent desktop sidebar requirement).
          On md+ sits at inline-start via row + border-e (logical), not "left sidebar".
        */}
        <aside
          aria-label="Navigation"
          className="w-full shrink-0 border-b border-border bg-surface md:w-60 md:border-b-0 md:border-e"
        >
          <div className="px-4 py-3">{navigation ?? null}</div>
        </aside>

        <ContentTag
          {...(embedded
            ? { role: "region", "aria-label": "Workspace" }
            : { id: "main-content", tabIndex: -1 })}
          className={cn(
            "flex min-w-0 flex-1 flex-col",
            !embedded && "outline-none",
          )}
        >
          {children}
        </ContentTag>
      </div>
    </div>
  );
}
