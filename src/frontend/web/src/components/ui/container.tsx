import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

const widthClass = {
  narrow: "max-w-narrow",
  content: "max-w-content",
  wide: "max-w-wide",
  full: "max-w-none",
} as const;

export type ContainerWidth = keyof typeof widthClass;

type ContainerProps = {
  children: ReactNode;
  width?: ContainerWidth;
  className?: string;
};

/**
 * Content width wrapper — direction-neutral (no physical left/right).
 * Mobile-first: full width until max constraint.
 */
export function Container({
  children,
  width = "content",
  className,
}: ContainerProps) {
  return (
    <div className={cn("mx-auto w-full px-4", widthClass[width], className)}>
      {children}
    </div>
  );
}
