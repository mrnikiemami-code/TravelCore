import type { ElementType, ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

const roleClass = {
  display: "text-display font-semibold tracking-tight text-foreground",
  heading: "text-heading font-semibold tracking-tight text-foreground",
  title: "text-title font-semibold text-foreground",
  body: "text-body text-foreground",
  label: "text-label font-medium text-foreground",
  caption: "text-caption text-muted-foreground",
  muted: "text-body text-muted-foreground",
} as const;

export type TextRole = keyof typeof roleClass;

type TextProps = {
  children: ReactNode;
  role?: TextRole;
  as?: ElementType;
  className?: string;
};

/**
 * Semantic text roles mapped to T003 typography tokens.
 * Alignment defaults to start (logical) via inheritance — no physical left/right.
 */
export function Text({
  children,
  role = "body",
  as: Comp = "p",
  className,
}: TextProps) {
  return <Comp className={cn(roleClass[role], className)}>{children}</Comp>;
}
