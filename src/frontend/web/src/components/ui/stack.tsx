import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

const gapClass = {
  none: "gap-0",
  sm: "gap-2",
  md: "gap-4",
  lg: "gap-6",
  xl: "gap-8",
} as const;

export type StackGap = keyof typeof gapClass;

type StackProps = {
  children: ReactNode;
  gap?: StackGap;
  className?: string;
};

/**
 * Vertical stack using gap (logical block flow). Works under rtl and ltr.
 */
export function Stack({ children, gap = "md", className }: StackProps) {
  return (
    <div className={cn("flex flex-col", gapClass[gap], className)}>
      {children}
    </div>
  );
}
