import type { ReactNode } from "react";
import { Container } from "@/components/ui/container";
import { Stack } from "@/components/ui/stack";
import { Surface } from "@/components/ui/surface";
import { Text } from "@/components/ui/text";
import { cn } from "@/lib/ui/cn";

type RouteStatePanelProps = {
  title: string;
  body: string;
  actions?: ReactNode;
  className?: string;
};

/**
 * Generic, direction-neutral status panel for loading/error/not-found (T007).
 * No domain/product assumptions. Meaning is not color-only.
 */
export function RouteStatePanel({
  title,
  body,
  actions,
  className,
}: RouteStatePanelProps) {
  return (
    <Container width="narrow" className={cn("py-12", className)}>
      <Surface>
        <Stack gap="md">
          <Text as="h1" role="heading">
            {title}
          </Text>
          <Text role="body">{body}</Text>
          {actions ? <div className="pt-2">{actions}</div> : null}
        </Stack>
      </Surface>
    </Container>
  );
}

type RouteLoadingSkeletonProps = {
  label: string;
  className?: string;
};

/**
 * Stable generic loading chrome — no fake progress %, no product skeleton.
 */
export function RouteLoadingSkeleton({
  label,
  className,
}: RouteLoadingSkeletonProps) {
  return (
    <Container width="content" className={cn("py-12", className)}>
      <div
        role="status"
        aria-live="polite"
        aria-busy="true"
        className="w-full"
      >
        <span className="sr-only">{label}</span>
        <Stack gap="md" aria-hidden="true">
          <div className="h-8 w-2/3 max-w-md animate-pulse rounded-md bg-surface-muted motion-reduce:animate-none" />
          <div className="h-4 w-full max-w-lg animate-pulse rounded-md bg-surface-muted motion-reduce:animate-none" />
          <div className="h-4 w-5/6 max-w-md animate-pulse rounded-md bg-surface-muted motion-reduce:animate-none" />
          <Surface tone="muted" className="min-h-32">
            <div className="h-24 w-full animate-pulse rounded-md bg-border/60 motion-reduce:animate-none" />
          </Surface>
        </Stack>
      </div>
    </Container>
  );
}
