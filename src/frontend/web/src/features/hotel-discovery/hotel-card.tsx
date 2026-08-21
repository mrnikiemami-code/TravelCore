import Link from "next/link";
import { Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";

/**
 * Hotel discovery card — commercial experience polish (TC-P31-T004).
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
          demoHint: "نمونه کاتالوگ",
        }
      : locale === "ar"
        ? {
            cta: "عرض الفندق",
            stars: "نجوم",
            facilities: "التفاصيل والمرافق في صفحة الفندق",
            imageAlt: "صورة الفندق",
            noStars: "بدون تصنيف نجوم",
            demoHint: "عينة الكتالوج",
          }
        : {
            cta: "View hotel",
            stars: "stars",
            facilities: "Details and facilities on the hotel page",
            imageAlt: "Hotel image",
            noStars: "Star rating pending",
            demoHint: "Sample catalog",
          };

  const href = `/${locale}/hotels/${encodeURIComponent(hotel.slug)}`;
  const isDemo = hotel.slug.startsWith("demofeed-");

  return (
    <article className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/35 hover:shadow-md">
      <Link href={href} className="relative block aspect-[4/3] overflow-hidden bg-gradient-to-br from-primary/80 via-primary/40 to-accent/70">
        {hotel.coverSrc ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={hotel.coverSrc}
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
      </Link>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <Text as="h2" role="label" className="text-base font-semibold">
          <Link
            href={href}
            className="min-h-touch inline-flex text-foreground underline-offset-2 hover:text-primary hover:underline"
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
