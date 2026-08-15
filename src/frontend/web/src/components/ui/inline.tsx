import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

const gapClass = {
  none: "gap-0",
  sm: "gap-2",
  md: "gap-3",
  lg: "gap-4",
} as const;

export type InlineGap = keyof typeof gapClass;

type InlineProps = {
  children: ReactNode;
  gap?: InlineGap;
  wrap?: boolean;
  className?: string;
};

/**
 * Horizontal inline composition. Flex row follows document direction
 * (main-start tracks writing mode) — no separate rtl/ltr variants.
 */
export function Inline({
  children,
  gap = "md",
  wrap = true,
  className,
}: InlineProps) {
  return (
    <div
      className={cn(
        "flex flex-row items-center",
        wrap && "flex-wrap",
        gapClass[gap],
        className,
      )}
    >
      {children}
    </div>
  );
}
