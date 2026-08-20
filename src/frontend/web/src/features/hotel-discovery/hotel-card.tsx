import Link from "next/link";
import { Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";

/**
 * Hotel discovery card — experience only.
 * No invented prices, availability, or ratings beyond catalog starRating when present.
 */
export function HotelCard({
  locale,
  hotel,
}: {
  locale: AppLocale;
  hotel: HotelBrowseItemView;
}) {
  const copy =
    locale === "fa"
      ? {
          cta: "مشاهده هتل",
          stars: "ستاره",
          facilities: "جزئیات و امکانات در صفحه هتل",
          imageAlt: "تصویر هتل",
          noStars: "ستاره ثبت‌نشده",
        }
      : locale === "ar"
        ? {
            cta: "عرض الفندق",
            stars: "نجوم",
            facilities: "التفاصيل والمرافق في صفحة الفندق",
            imageAlt: "صورة الفندق",
            noStars: "بدون تصنيف نجوم",
          }
        : {
            cta: "View hotel",
            stars: "stars",
            facilities: "Details and facilities on the hotel page",
            imageAlt: "Hotel image",
            noStars: "Star rating pending",
          };

  const href = `/${locale}/hotels/${encodeURIComponent(hotel.slug)}`;

  return (
    <article className="flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition-shadow hover:shadow-md">
      <div
        className="aspect-[4/3] w-full bg-gradient-to-br from-primary/80 via-primary/40 to-accent/70"
        aria-hidden
      >
        <div className="flex h-full items-end p-3">
          <Text role="caption" className="rounded-md bg-background/85 px-2 py-1 text-foreground">
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
            {hotel.name}
          </Link>
        </Text>
        {hotel.description ? (
          <Text role="muted" className="line-clamp-2 text-sm">
            {hotel.description}
          </Text>
        ) : (
          <Text role="caption">{copy.facilities}</Text>
        )}
        <div className="mt-auto flex flex-wrap items-center justify-between gap-2 pt-2">
          <Text role="caption">
            {hotel.starRating != null
              ? `${hotel.starRating} ${copy.stars}`
              : copy.noStars}
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
