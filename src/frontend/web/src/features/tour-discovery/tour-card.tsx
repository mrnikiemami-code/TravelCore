import Link from "next/link";
import { Text } from "@/components/ui";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import type { AppLocale } from "@/lib/i18n";

/**
 * Tour discovery card — commercial experience polish (TC-P36-T004).
 * RelatedTourView has no price/availability; do not invent them.
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
          packageLabel: "پکیج",
          details: "جزئیات، حرکت و قیمت در صفحه تور",
          demoHint: "نمونه کاتالوگ",
        }
      : locale === "ar"
        ? {
            cta: "عرض الجولة",
            imageAlt: "صورة الجولة",
            packageLabel: "باقة",
            details: "التفاصيل والمغادرة والسعر في صفحة الجولة",
            demoHint: "عينة الكتالوج",
          }
        : {
            cta: "View tour",
            imageAlt: "Tour image",
            packageLabel: "Package",
            details: "Details, departures, and pricing on the tour page",
            demoHint: "Sample catalog",
          };

  const href = `/${locale}/tours/${encodeURIComponent(tour.slug)}`;
  const isDemo =
    tour.slug.startsWith("demofeed-") || tour.code.startsWith("demofeed-");
  const kindLabel = tour.kind?.trim() || copy.packageLabel;

  return (
    <article className="group flex h-full flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-sm transition hover:-translate-y-0.5 hover:border-[#1D4ED8]/35 hover:shadow-lg">
      <Link
        href={href}
        className="relative block aspect-[4/3] overflow-hidden bg-gradient-to-br from-[#1D4ED8]/80 via-[#1D4ED8]/40 to-[#F59E0B]/70"
      >
        {tour.coverSrc ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={tour.coverSrc}
            alt={copy.imageAlt}
            className="h-full w-full object-cover transition duration-300 group-hover:scale-[1.03]"
          />
        ) : (
          <div className="flex h-full items-end p-3" aria-hidden>
            <Text
              role="caption"
              className="rounded-md bg-background/85 px-2 py-1 text-foreground"
            >
              {copy.imageAlt}
            </Text>
          </div>
        )}
        {isDemo ? (
          <span className="absolute end-2 top-2 rounded-full bg-black/45 px-2 py-0.5 text-[10px] font-medium tracking-wide text-white/90 backdrop-blur-sm">
            {copy.demoHint}
          </span>
        ) : null}
        <span className="absolute bottom-3 start-3 rounded-full bg-white/90 px-2.5 py-1 text-[11px] font-semibold text-[#0E172A]">
          {kindLabel}
        </span>
      </Link>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <Text as="h2" role="label" className="text-base font-semibold">
          <Link
            href={href}
            className="min-h-touch inline-flex text-foreground underline-offset-2 hover:text-[#1D4ED8] hover:underline"
          >
            {tour.name}
          </Link>
        </Text>
        <Text role="muted" className="line-clamp-2 text-sm">
          {copy.details}
        </Text>
        <div className="mt-auto flex flex-wrap items-center justify-end gap-2 pt-2">
          <Link
            href={href}
            className="min-h-touch inline-flex items-center rounded-lg bg-[#1D4ED8] px-3 py-2 text-sm font-semibold text-white hover:bg-[#1E40AF]"
          >
            {copy.cta}
          </Link>
        </div>
      </div>
    </article>
  );
}
