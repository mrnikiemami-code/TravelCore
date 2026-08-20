import { Container, Stack, Text } from "@/components/ui";
import { HotelCard } from "@/features/hotel-discovery/hotel-card";
import {
  applyHotelListingCriteria,
  type HotelListingCriteria,
} from "@/features/hotel-discovery/hotel-listing-criteria";
import { HotelListingToolbar } from "@/features/hotel-discovery/hotel-listing-toolbar";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";
import type { AppLocale } from "@/lib/i18n";

/**
 * Hotel commerce listing experience (TC-P30-T006).
 * Place catalog discovery · not Search · not HotelBooking availability.
 */
export function HotelDiscoveryView({
  locale,
  hotels,
  criteria,
  loadError,
}: {
  locale: AppLocale;
  hotels: HotelBrowseItemView[];
  criteria: HotelListingCriteria;
  loadError: boolean;
}) {
  const title = locale === "fa" ? "هتل‌ها" : locale === "ar" ? "الفنادق" : "Hotels";
  const filtered = applyHotelListingCriteria(hotels, criteria);

  const copy =
    locale === "fa"
      ? {
          blurb: "هتل‌های منتخب را کشف کنید · بدون قیمت یا موجودی ساختگی",
          emptyTitle: "هتلی برای نمایش نیست",
          emptyBody:
            "فعلاً هتلی با این فیلتر پیدا نشد. به‌محض انتشار کاتالوگ، اینجا نمایش داده می‌شود.",
          errorTitle: "بارگذاری فهرست هتل‌ها ناموفق بود",
          errorBody:
            "اتصال موقتاً برقرار نشد. لطفاً کمی بعد دوباره تلاش کنید.",
          retry: "تلاش دوباره",
          count: (n: number) => `${n} هتل`,
        }
      : locale === "ar"
        ? {
            blurb: "اكتشف الفنادق المختارة · دون أسعار أو توفر وهمي",
            emptyTitle: "لا فنادق للعرض",
            emptyBody:
              "لا نتائج لهذا التصفية حالياً. ستظهر الفنادق عند توفر الكتالوج.",
            errorTitle: "تعذر تحميل قائمة الفنادق",
            errorBody: "الاتصال غير متاح مؤقتاً. حاول مرة أخرى بعد قليل.",
            retry: "إعادة المحاولة",
            count: (n: number) => `${n} فندق`,
          }
        : {
            blurb: "Discover selected hotels · no invented prices or availability",
            emptyTitle: "No hotels to show",
            emptyBody:
              "Nothing matched this filter yet. Hotels appear here when the catalog is published.",
            errorTitle: "Couldn’t load hotels",
            errorBody:
              "The connection failed temporarily. Please try again in a moment.",
            retry: "Try again",
            count: (n: number) => `${n} hotels`,
          };

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {title}
            </Text>
            <Text role="caption">{copy.blurb}</Text>
          </Stack>

          <HotelListingToolbar locale={locale} criteria={criteria} />

          {loadError ? (
            <div
              role="alert"
              className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-8"
            >
              <Text as="h2" role="label">
                {copy.errorTitle}
              </Text>
              <Text role="muted" className="mt-2">
                {copy.errorBody}
              </Text>
              <a
                href={`/${locale}/hotels`}
                className="mt-5 inline-flex min-h-touch items-center justify-center rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground hover:opacity-95"
              >
                {copy.retry}
              </a>
            </div>
          ) : filtered.length === 0 ? (
            <div className="rounded-xl border border-dashed border-border bg-surface/60 p-8 text-center">
              <Text as="h2" role="label">
                {copy.emptyTitle}
              </Text>
              <Text role="muted" className="mt-2">
                {copy.emptyBody}
              </Text>
              <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {[0, 1, 2].map((i) => (
                  <div
                    key={i}
                    className="aspect-[4/3] rounded-xl bg-gradient-to-br from-primary/25 via-muted to-accent/30"
                    aria-hidden
                  />
                ))}
              </div>
            </div>
          ) : (
            <Stack gap="md">
              <Text role="caption">{copy.count(filtered.length)}</Text>
              <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {filtered.map((item) => (
                  <li key={item.placeId}>
                    <HotelCard locale={locale} hotel={item} />
                  </li>
                ))}
              </ul>
            </Stack>
          )}
        </Stack>
      </Container>
    </div>
  );
}
