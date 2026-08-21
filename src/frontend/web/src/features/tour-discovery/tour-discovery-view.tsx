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
 * Tour commerce listing experience (TC-P30-T007 · TC-P31-T005 polish).
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
  const destinationLabel = criteria.destination.trim();

  const copy =
    locale === "fa"
      ? {
          eyebrow: "Tour commerce",
          blurb:
            "کاتالوگ تور حرفه‌ای برای دمو تجاری — بدون قیمت، موجودی یا ادعای فروش ساختگی.",
          needsTitle: "مقصد را انتخاب کنید",
          needsBody:
            "برای نمایش تورهای منتشرشده، slug مقصد را وارد کنید (مثلاً demofeed-tehran). فهرست سراسری در این لایه موجود نیست.",
          emptyTitle: "توری برای نمایش نیست",
          emptyBody:
            "برای این مقصد یا فیلتر، تور منتشرشده‌ای پیدا نشد. داده جعلی نشان نمی‌دهیم.",
          errorTitle: "بارگذاری فهرست تورها ناموفق بود",
          errorBody:
            "اتصال موقتاً برقرار نشد یا مقصد پیدا نشد. قیمت یا موجودی جعلی به‌جای خطا نشان داده نمی‌شود.",
          retry: "تلاش دوباره",
          count: (n: number, dest: string) =>
            dest ? `${n} تور برای ${dest}` : `${n} تور در فهرست`,
          marketplaceHint: "بازار تور · بر اساس مقصد منتشرشده",
        }
      : locale === "ar"
        ? {
            eyebrow: "Tour commerce",
            blurb:
              "كتالوج جولات احترافي للعرض التجاري — دون أسعار أو توفر أو ادعاءات بيع وهمية.",
            needsTitle: "اختر وجهة",
            needsBody:
              "أدخل slug وجهة منشورة لعرض الجولات (مثل demofeed-tehran). لا توجد قائمة عامة كاملة في هذه الطبقة.",
            emptyTitle: "لا جولات للعرض",
            emptyBody:
              "لا جولات منشورة لهذه الوجهة أو التصفية. لا نعرض بيانات وهمية.",
            errorTitle: "تعذر تحميل قائمة الجولات",
            errorBody:
              "الاتصال غير متاح أو الوجهة غير موجودة. لا نعرض أسعاراً أو توفراً وهمياً بدل الخطأ.",
            retry: "إعادة المحاولة",
            count: (n: number, dest: string) =>
              dest ? `${n} جولة لـ ${dest}` : `${n} جولة في القائمة`,
            marketplaceHint: "سوق الجولات · حسب الوجهة المنشورة",
          }
        : {
            eyebrow: "Tour commerce",
            blurb:
              "Professional tour catalog for commercial demos — no invented prices, availability, or sales claims.",
            needsTitle: "Choose a destination",
            needsBody:
              "Enter a published destination slug to list tours (e.g. demofeed-tehran). A global browse catalog is not available on this layer.",
            emptyTitle: "No tours to show",
            emptyBody:
              "No published tours matched this destination or filter. We do not invent catalog rows.",
            errorTitle: "Couldn’t load tours",
            errorBody:
              "The connection failed or the destination was not found. We do not invent prices or availability instead.",
            retry: "Try again",
            count: (n: number, dest: string) =>
              dest ? `${n} tours for ${dest}` : `${n} tours in list`,
            marketplaceHint: "Tour marketplace · published destination scope",
          };

  const showNeeds = !loadError && needsDestination;
  const showEmpty = !loadError && !needsDestination && filtered.length === 0;

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
          {destinationLabel ? (
            <p className="mt-3 text-sm text-primary-foreground/80">
              {copy.marketplaceHint}
              {" · "}
              <span className="font-medium text-accent">{destinationLabel}</span>
            </p>
          ) : null}
        </Container>
      </section>

      <Container width="wide" className="pt-6 sm:pt-8">
        <Stack gap="lg">
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
            <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
              <div className="grid gap-0 md:grid-cols-[1fr_1.2fr]">
                <div className="min-h-40 bg-gradient-to-br from-primary via-primary/70 to-accent" />
                <div className="space-y-3 p-6 sm:p-8">
                  <Text as="h2" role="label">
                    {copy.needsTitle}
                  </Text>
                  <Text role="muted">{copy.needsBody}</Text>
                  <div className="mt-4 grid grid-cols-3 gap-2">
                    {[0, 1, 2].map((i) => (
                      <div
                        key={i}
                        className="aspect-[4/3] rounded-lg bg-gradient-to-br from-primary/20 via-muted to-accent/25"
                        aria-hidden
                      />
                    ))}
                  </div>
                </div>
              </div>
            </div>
          ) : showEmpty ? (
            <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
              <div className="grid gap-0 md:grid-cols-[1fr_1.2fr]">
                <div className="min-h-40 bg-gradient-to-br from-primary/80 via-primary/50 to-accent/60" />
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
              <Text role="caption">
                {copy.count(filtered.length, destinationLabel)}
              </Text>
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
