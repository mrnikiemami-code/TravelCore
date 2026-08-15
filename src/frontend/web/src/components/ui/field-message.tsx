import type { ReactNode } from "react";
import { cn } from "@/lib/ui/cn";

export type FieldMessageTone = "help" | "error" | "status";

type FieldMessageProps = {
  id: string;
  children: ReactNode;
  tone?: FieldMessageTone;
  className?: string;
};

/**
 * Associated help/error/status text for future forms.
 * Wire via `aria-describedby` / `aria-errormessage` on the control.
 * Error meaning is not color-only (role=alert + text).
 */
export function FieldMessage({
  id,
  children,
  tone = "help",
  className,
}: FieldMessageProps) {
  const role = tone === "error" ? "alert" : tone === "status" ? "status" : undefined;

  return (
    <p
      id={id}
      role={role}
      className={cn(
        "text-caption",
        tone === "error" ? "text-danger" : "text-muted-foreground",
        className,
      )}
    >
      {children}
    </p>
  );
}

type VisuallyHiddenProps = {
  children: ReactNode;
  as?: "span" | "div";
};

/** Screen-reader-only text without affecting layout. */
export function VisuallyHidden({
  children,
  as: Comp = "span",
}: VisuallyHiddenProps) {
  return <Comp className="sr-only">{children}</Comp>;
}
