import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

export type SkipLinkProps = {
  /** Target element id without `#` — defaults to main landmark. */
  hrefId?: string;
  children?: ReactNode;
  className?: string;
};

/**
 * Keyboard-first skip control. Visually hidden until focused.
 * Direction-neutral — uses logical inset-inline-start.
 */
export function SkipLink({
  hrefId = "main-content",
  children = "Skip to content",
  className,
}: SkipLinkProps) {
  return (
    <a
      href={`#${hrefId}`}
      className={cn(
        "sr-only focus:not-sr-only focus:absolute focus:z-toast focus:m-2 focus:inline-flex focus:min-h-touch focus:items-center focus:rounded-md focus:bg-primary focus:px-4 focus:text-label focus:text-primary-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-focus focus-visible:ring-offset-2 focus-visible:ring-offset-background",
        "focus:start-2 focus:top-2",
        className,
      )}
    >
      {children}
    </a>
  );
}
