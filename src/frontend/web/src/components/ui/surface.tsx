import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

const toneClass = {
  default: "bg-surface border-border",
  muted: "bg-surface-muted border-border",
} as const;

export type SurfaceTone = keyof typeof toneClass;

type SurfaceProps = {
  children: ReactNode;
  tone?: SurfaceTone;
  padded?: boolean;
  className?: string;
};

/**
 * Neutral surface / card-like wrapper using T003 semantic tokens.
 */
export function Surface({
  children,
  tone = "default",
  padded = true,
  className,
}: SurfaceProps) {
  return (
    <div
      className={cn(
        "rounded-lg border shadow-sm",
        toneClass[tone],
        padded && "p-4 sm:p-6",
        className,
      )}
    >
      {children}
    </div>
  );
}
