"use client";

import { useId, useState } from "react";
import { cn } from "@/lib/ui/cn";
import type { BookingCtaView } from "@/types/pages/foreign-tour-detail";
import type { AppLocale } from "@/lib/i18n";

export type BookingCtaIslandProps = {
  cta: BookingCtaView;
  locale: AppLocale;
  className?: string;
};

/**
 * Minimal interactive booking-action island (T014).
 * Not booking authority — only presents T012 CTA state + future handoff hook.
 */
export function BookingCtaIsland({
  cta,
  locale,
  className,
}: BookingCtaIslandProps) {
  const liveId = useId();
  const [note, setNote] = useState<string | null>(null);

  const placeholderMessage =
    locale === "fa"
      ? "رزرو زنده هنوز فعال نیست (Walking Skeleton)."
      : "Live booking is not enabled yet (Walking Skeleton).";

  function onActivate() {
    if (!cta.enabled) return;
    setNote(placeholderMessage);
  }

  return (
    <div
      className={cn(
        // Mobile-first sticky affordance; desktop keeps readable in-flow + stickiness
        "sticky bottom-0 z-40 border-t border-border bg-surface/95 p-3 backdrop-blur supports-[backdrop-filter]:bg-surface/90",
        "md:bottom-3 md:mx-auto md:max-w-content md:rounded-lg md:border md:shadow-sm",
        className,
      )}
    >
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div className="min-w-0">
          {!cta.enabled && cta.reasonDisabled ? (
            <p className="text-caption text-muted-foreground">{cta.reasonDisabled}</p>
          ) : (
            <p className="text-caption text-muted-foreground">
              {locale === "fa"
                ? "اقدام از وضعیت نمایشی CTA — واجد شرایط بودن نهایی با Backend است."
                : "Action from presentation CTA state — Backend remains authoritative."}
            </p>
          )}
          <p id={liveId} className="sr-only" aria-live="polite">
            {note ?? ""}
          </p>
          {note ? (
            <p className="text-caption text-foreground" aria-hidden="true">
              {note}
            </p>
          ) : null}
        </div>

        <button
          type="button"
          disabled={!cta.enabled}
          onClick={onActivate}
          className={cn(
            "inline-flex min-h-touch shrink-0 items-center justify-center rounded-md px-4 text-label",
            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-focus",
            cta.enabled
              ? "bg-primary text-primary-foreground"
              : "cursor-not-allowed bg-surface-muted text-muted-foreground",
          )}
        >
          {cta.label}
        </button>
      </div>
    </div>
  );
}
