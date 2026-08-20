import { Container, Stack, Text } from "@/components/ui";
import { TourCard } from "@/features/tour-discovery/tour-card";
import {
  applyTourListingCriteria,
  type TourListingCriteria,
} from "@/features/tour-discovery/tour-listing-criteria";
import { TourListingToolbar } from "@/features/tour-discovery/tour-listing-toolbar";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import type { AppLocale } from "@/lib/i18n";

/**
 * Tour commerce listing experience (TC-P30-T007).
 * Destination-scoped related-published discovery · no invented catalog.
 */
export function TourDiscoveryView({
  locale,
  tours,
  criteria,
  loadError,
  needsDestination,
}: {
  locale: AppLocale;
  tours: RelatedTourView[];
  criteria: TourListingCriteria;
  loadError: boolean;
  needsDestination: boolean;
}) {
  const title = locale === "fa" ? "تورها" : locale === "ar" ? "الجولات" : "Tours";
  const filtered = applyTourListingCriteria(tours, criteria);

  const copy =
    locale === "fa"
      ? {
          blurb: "تورهای منتشرشده را کشف کنید · بدون قیمت یا موجودی ساختگی",
          needsTitle: "مقصد را انتخاب کنید",
          needsBody:
            "برای نمایش تورها، slug مقصد منتشرشده را وارد کنید (مثلاً istanbul). فهرست کامل سراسری در این لایه موجود نیست.",
          emptyTitle: "توری برای نمایش نیست",
          emptyBody:
            "برای این مقصد یا فیلتر، تور منتشرشده‌ای پیدا نشد. داده جعلی نشان نمی‌دهیم.",
          errorTitle: "بارگذاری فهرست تورها ناموفق بود",
          errorBody:
            "اتصال موقتاً برقرار نشد یا مقصد پیدا نشد. لطفاً کمی بعد دوباره تلاش کنید.",
          retry: "تلاش دوباره",
          count: (n: number) => `${n} تور`,
        }
      : locale === "ar"
        ? {
            blurb: "اكتشف الجولات المنشورة · دون أسعار أو توفر وهمي",
            needsTitle: "اختر وجهة",
            needsBody:
              "أدخل slug وجهة منشورة لعرض الجولات. لا توجد قائمة عامة كاملة في هذه الطبقة.",
            emptyTitle: "لا جولات للعرض",
            emptyBody:
              "لا جولات منشورة لهذه الوجهة أو التصفية. لا نعرض بيانات وهمية.",
            errorTitle: "تعذر تحميل قائمة الجولات",
            errorBody:
              "الاتصال غير متاح أو الوجهة غير موجودة. حاول مرة أخرى بعد قليل.",
            retry: "إعادة المحاولة",
            count: (n: number) => `${n} جولة`,
          }
        : {
            blurb: "Discover published tours · no invented prices or availability",
            needsTitle: "Choose a destination",
            needsBody:
              "Enter a published destination slug to list tours (e.g. istanbul). A global browse catalog is not available on this layer.",
            emptyTitle: "No tours to show",
            emptyBody:
              "No published tours matched this destination or filter. We do not invent catalog rows.",
            errorTitle: "Couldn’t load tours",
            errorBody:
              "The connection failed or the destination was not found. Please try again in a moment.",
            retry: "Try again",
            count: (n: number) => `${n} tours`,
          };

  const showNeeds = !loadError && needsDestination;
  const showEmpty = !loadError && !needsDestination && filtered.length === 0;

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

          <TourListingToolbar locale={locale} criteria={criteria} />

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
                href={`/${locale}/tours`}
                className="mt-5 inline-flex min-h-touch items-center justify-center rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground hover:opacity-95"
              >
                {copy.retry}
              </a>
            </div>
          ) : showNeeds ? (
            <div className="rounded-xl border border-dashed border-border bg-surface/60 p-8 text-center">
              <Text as="h2" role="label">
                {copy.needsTitle}
              </Text>
              <Text role="muted" className="mt-2">
                {copy.needsBody}
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
          ) : showEmpty ? (
            <div className="rounded-xl border border-dashed border-border bg-surface/60 p-8 text-center">
              <Text as="h2" role="label">
                {copy.emptyTitle}
              </Text>
              <Text role="muted" className="mt-2">
                {copy.emptyBody}
              </Text>
            </div>
          ) : (
            <Stack gap="md">
              <Text role="caption">{copy.count(filtered.length)}</Text>
              <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {filtered.map((item) => (
                  <li key={item.tourProductId}>
                    <TourCard locale={locale} tour={item} />
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
