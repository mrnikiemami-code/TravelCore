import Link from "next/link";
import { Text } from "@/components/ui";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import type { AppLocale } from "@/lib/i18n";

/**
 * Tour discovery card — experience only.
 * RelatedTourView has no media/price; do not invent them.
 */
export function TourCard({
  locale,
  tour,
}: {
  locale: AppLocale;
  tour: RelatedTourView;
}) {
  const copy =
    locale === "fa"
      ? {
          cta: "مشاهده تور",
          imageAlt: "تصویر تور",
          kind: "نوع",
        }
      : locale === "ar"
        ? {
            cta: "عرض الجولة",
            imageAlt: "صورة الجولة",
            kind: "النوع",
          }
        : {
            cta: "View tour",
            imageAlt: "Tour image",
            kind: "Kind",
          };

  const href = `/${locale}/tours/${encodeURIComponent(tour.slug)}`;

  return (
    <article className="flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition-shadow hover:shadow-md">
      <div
        className="aspect-[4/3] w-full bg-gradient-to-br from-primary/80 via-primary/40 to-accent/70"
        aria-hidden
      >
        <div className="flex h-full items-end p-3">
          <Text
            role="caption"
            className="rounded-md bg-background/85 px-2 py-1 text-foreground"
          >
            {copy.imageAlt}
          </Text>
        </div>
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <Text as="h2" role="label">
          <Link
            href={href}
            className="min-h-touch inline-flex underline-offset-2 hover:underline"
          >
            {tour.name}
          </Link>
        </Text>
        <Text role="caption">
          {copy.kind}: {tour.kind}
        </Text>
        <div className="mt-auto flex flex-wrap items-center justify-between gap-2 pt-2">
          <Text role="caption" className="font-mono text-xs">
            {tour.code}
          </Text>
          <Link
            href={href}
            className="min-h-touch inline-flex items-center rounded-md bg-primary px-3 py-2 text-sm font-medium text-primary-foreground hover:opacity-95"
          >
            {copy.cta}
          </Link>
        </div>
      </div>
    </article>
  );
}
