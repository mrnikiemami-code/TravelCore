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
 * Hotel commerce listing experience (TC-P36-T003 polish).
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
  const heroCover =
    filtered.find((h) => h.coverSrc)?.coverSrc ??
    hotels.find((h) => h.coverSrc)?.coverSrc ??
    null;

  const copy =
    locale === "fa"
      ? {
          eyebrow: "اقامت حرفه‌ای",
          blurb:
            "کاتالوگ هتل‌های منتشرشده — بدون قیمت، موجودی یا امتیاز ساختگی.",
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
            eyebrow: "إقامة احترافية",
            blurb:
              "كتالوج فنادق منشورة — دون أسعار أو توفر أو تقييمات وهمية.",
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
            eyebrow: "Professional stays",
            blurb:
              "Published hotel catalog — no invented prices, availability, or ratings.",
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
    <div className="pb-14">
      <section className="relative isolate overflow-hidden border-b border-border">
        <div className="absolute inset-0 bg-[#0E172A]" aria-hidden />
        {heroCover ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={heroCover}
            alt=""
            className="absolute inset-0 h-full w-full object-cover opacity-55"
          />
        ) : (
          <div
            aria-hidden
            className="absolute inset-0 bg-[linear-gradient(135deg,#0E172A_0%,#1D4ED8_55%,#0E172A_100%)]"
          />
        )}
        <div
          aria-hidden
          className="absolute inset-0 bg-gradient-to-r from-[#0E172A]/92 via-[#0E172A]/75 to-[#0E172A]/45"
        />
        <Container width="wide" className="relative py-10 sm:py-12">
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[#FBBF24]">
            {copy.eyebrow}
          </p>
          <h1 className="mt-2 text-3xl font-semibold tracking-tight text-white sm:text-4xl">
            {title}
          </h1>
          <p className="mt-2 max-w-2xl text-sm text-white/90 sm:text-base">
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
              className="rounded-2xl border border-border bg-surface p-6 shadow-sm sm:p-8"
            >
              <Text as="h2" role="label">
                {copy.errorTitle}
              </Text>
              <Text role="muted" className="mt-2">
                {copy.errorBody}
              </Text>
              <a
                href={`/${locale}/hotels`}
                className="mt-5 inline-flex min-h-touch items-center justify-center rounded-lg bg-[#1D4ED8] px-4 text-sm font-semibold text-white hover:bg-[#1E40AF]"
              >
                {copy.retry}
              </a>
            </div>
          ) : filtered.length === 0 ? (
            <div className="overflow-hidden rounded-2xl border border-border bg-surface shadow-sm">
              <div className="grid gap-0 md:grid-cols-[0.9fr_1.3fr]">
                <div className="min-h-40 bg-[linear-gradient(145deg,#1D4ED8,#0E172A_55%,#F59E0B)]" />
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
              <p className="text-sm text-muted-foreground">
                {copy.count(filtered.length)}
              </p>
              <ul className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
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
