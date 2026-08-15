import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

export type BidiDir = "ltr" | "rtl" | "auto";

type BidiTextProps = {
  children: ReactNode;
  /**
   * Content direction isolation.
   * - `auto` (default): browser detects from first strong character
   * - `ltr` / `rtl`: when the value's script direction is known
   */
  dir?: BidiDir;
  className?: string;
};

/**
 * Isolates mixed-direction values from surrounding page flow using `<bdi>`.
 * Does not reverse strings or inject Unicode bidi controls.
 *
 * Document `dir` (from locale layout) remains separate from content direction.
 */
export function BidiText({ children, dir = "auto", className }: BidiTextProps) {
  return (
    <bdi dir={dir} className={className}>
      {children}
    </bdi>
  );
}

type LtrValueProps = {
  children: ReactNode;
  className?: string;
};

/**
 * Convenience wrapper for known LTR identifiers
 * (airport codes, flight numbers, emails, URLs, currency codes, refs).
 */
export function LtrValue({ children, className }: LtrValueProps) {
  return (
    <BidiText dir="ltr" className={cn("font-mono text-label", className)}>
      {children}
    </BidiText>
  );
}
