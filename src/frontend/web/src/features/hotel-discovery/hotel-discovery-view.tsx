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
 * Hotel commerce listing experience (TC-P30-T006 · TC-P31-T004 polish).
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
          eyebrow: "Hotel commerce",
          blurb:
            "کاتالوگ Place حرفه‌ای برای دمو تجاری — بدون قیمت، موجودی یا امتیاز ساختگی.",
          emptyTitle: "هتلی برای نمایش نیست",
          emptyBody:
            "فعلاً هتلی با این فیلتر پیدا نشد. به‌محض انتشار کاتالوگ، کارت‌های فروش اینجا می‌آید.",
          errorTitle: "بارگذاری فهرست هتل‌ها ناموفق بود",
          errorBody:
            "اتصال موقتاً برقرار نشد. قیمت یا موجودی جعلی به‌جای خطا نشان داده نمی‌شود.",
          retry: "تلاش دوباره",
          count: (n: number) => `${n} هتل در کاتالوگ`,
        }
      : locale === "ar"
        ? {
            eyebrow: "Hotel commerce",
            blurb:
              "كتالوج Place احترافي للعرض التجاري — دون أسعار أو توفر أو تقييمات وهمية.",
            emptyTitle: "لا فنادق للعرض",
            emptyBody:
              "لا نتائج لهذا التصفية حالياً. ستظهر بطاقات البيع عند توفر الكتالوج.",
            errorTitle: "تعذر تحميل قائمة الفنادق",
            errorBody:
              "الاتصال غير متاح مؤقتاً. لا نعرض أسعاراً أو توفراً وهمياً بدل الخطأ.",
            retry: "إعادة المحاولة",
            count: (n: number) => `${n} فندق في الكتالوج`,
          }
        : {
            eyebrow: "Hotel commerce",
            blurb:
              "Professional Place catalog for commercial demos — no invented prices, availability, or ratings.",
            emptyTitle: "No hotels to show",
            emptyBody:
              "Nothing matched this filter yet. Sales-ready cards appear when the catalog is published.",
            errorTitle: "Couldn’t load hotels",
            errorBody:
              "The connection failed temporarily. We do not invent prices or availability instead.",
            retry: "Try again",
            count: (n: number) => `${n} hotels in catalog`,
          };

  return (
    <div className="pb-10">
      <section className="border-b border-border bg-gradient-to-br from-primary via-primary to-primary/80 text-primary-foreground">
        <Container width="wide" className="py-8 sm:py-10">
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-accent">
            {copy.eyebrow}
          </p>
          <h1 className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl">
            {title}
          </h1>
          <p className="mt-2 max-w-2xl text-sm text-primary-foreground/90 sm:text-base">
            {copy.blurb}
          </p>
        </Container>
      </section>

      <Container width="wide" className="pt-6 sm:pt-8">
        <Stack gap="lg">
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
            <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
              <div className="grid gap-0 md:grid-cols-[1fr_1.2fr]">
                <div className="min-h-40 bg-gradient-to-br from-primary via-primary/70 to-accent" />
                <div className="space-y-3 p-6 sm:p-8">
                  <Text as="h2" role="label">
                    {copy.emptyTitle}
                  </Text>
                  <Text role="muted">{copy.emptyBody}</Text>
                </div>
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
