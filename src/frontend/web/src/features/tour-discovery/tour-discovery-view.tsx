import Link from "next/link";
import { Container, Stack, Text } from "@/components/ui";
import { TourCard } from "@/features/tour-discovery/tour-card";
import {
  applyTourListingCriteria,
  humanDestinationLabel,
  type TourListingCriteria,
} from "@/features/tour-discovery/tour-listing-criteria";
import { TourListingToolbar } from "@/features/tour-discovery/tour-listing-toolbar";
import { tourDestinationOptions } from "@/features/tour-discovery/tour-destination-options";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import type { AppLocale } from "@/lib/i18n";

/**
 * Tour commerce listing experience (TC-P36-T004 polish).
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
  const destinationLabel = humanDestinationLabel(
    locale,
    criteria.destination.trim(),
  );
  const heroCover = filtered.find((t) => t.coverSrc)?.coverSrc ?? null;
  const quickDestinations = tourDestinationOptions(locale);

  const copy =
    locale === "fa"
      ? {
          eyebrow: "پکیج‌های سفر",
          blurb:
            "تورهای منتشرشده بر اساس مقصد — بدون قیمت، موجودی یا ادعای فروش ساختگی.",
          needsTitle: "یک مقصد را انتخاب کنید",
          needsBody:
            "تورهای منتشرشده بر اساس مقصد نمایش داده می‌شوند. از فهرست زیر یک مقصد انتخاب کنید — نیازی به دانستن شناسه فنی نیست.",
          emptyTitle: "توری برای نمایش نیست",
          emptyBody:
            "برای این مقصد یا فیلتر، تور منتشرشده‌ای پیدا نشد. داده جعلی نشان نمی‌دهیم.",
          errorTitle: "بارگذاری فهرست تورها ناموفق بود",
          errorBody:
            "اتصال موقتاً برقرار نشد یا مقصد پیدا نشد. قیمت یا موجودی جعلی به‌جای خطا نشان داده نمی‌شود.",
          retry: "تلاش دوباره",
          count: (n: number, dest: string) =>
            dest ? `${n} تور برای ${dest}` : `${n} تور در فهرست`,
          marketplaceHint: "بازار تور · مقصد منتشرشده",
          quickPick: "مقصدهای آماده",
        }
      : locale === "ar"
        ? {
            eyebrow: "باقات السفر",
            blurb:
              "جولات منشورة حسب الوجهة — دون أسعار أو توفر أو ادعاءات بيع وهمية.",
            needsTitle: "اختر وجهة",
            needsBody:
              "تُعرض الجولات المنشورة حسب الوجهة. اختر من القائمة أدناه — لا حاجة لمعرفات تقنية.",
            emptyTitle: "لا جولات للعرض",
            emptyBody:
              "لا جولات منشورة لهذه الوجهة أو التصفية. لا نعرض بيانات وهمية.",
            errorTitle: "تعذر تحميل قائمة الجولات",
            errorBody:
              "الاتصال غير متاح أو الوجهة غير موجودة. لا نعرض أسعاراً أو توفراً وهمياً بدل الخطأ.",
            retry: "إعادة المحاولة",
            count: (n: number, dest: string) =>
              dest ? `${n} جولة لـ ${dest}` : `${n} جولة في القائمة`,
            marketplaceHint: "سوق الجولات · وجهة منشورة",
            quickPick: "وجهات جاهزة",
          }
        : {
            eyebrow: "Travel packages",
            blurb:
              "Published tours by destination — no invented prices, availability, or sales claims.",
            needsTitle: "Choose a destination",
            needsBody:
              "Published tours are listed by destination. Pick from the friendly list below — you do not need an internal slug.",
            emptyTitle: "No tours to show",
            emptyBody:
              "No published tours matched this destination or filter. We do not invent catalog rows.",
            errorTitle: "Couldn’t load tours",
            errorBody:
              "The connection failed or the destination was not found. We do not invent prices or availability instead.",
            retry: "Try again",
            count: (n: number, dest: string) =>
              dest ? `${n} tours for ${dest}` : `${n} tours in list`,
            marketplaceHint: "Tour marketplace · published destination",
            quickPick: "Ready destinations",
          };

  const showNeeds = !loadError && needsDestination;
  const showEmpty = !loadError && !needsDestination && filtered.length === 0;

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
          {destinationLabel ? (
            <p className="mt-3 text-sm text-white/80">
              {copy.marketplaceHint}
              {" · "}
              <span className="font-medium text-[#FBBF24]">
                {destinationLabel}
              </span>
            </p>
          ) : null}
        </Container>
      </section>

      <Container width="wide" className="pt-6 sm:pt-8">
        <Stack gap="lg">
          <TourListingToolbar locale={locale} criteria={criteria} />

          {showNeeds ? (
            <div className="space-y-3">
              <p className="text-sm font-medium text-foreground">
                {copy.quickPick}
              </p>
              <ul className="flex flex-wrap gap-2">
                {quickDestinations.map((d) => (
                  <li key={d.slug}>
                    <Link
                      href={`/${locale}/tours?destination=${encodeURIComponent(d.slug)}`}
                      className="inline-flex min-h-touch items-center rounded-full border border-border bg-surface px-4 text-sm font-medium text-foreground hover:border-[#1D4ED8]/40 hover:text-[#1D4ED8]"
                    >
                      {d.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

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
                href={`/${locale}/tours`}
                className="mt-5 inline-flex min-h-touch items-center justify-center rounded-lg bg-[#1D4ED8] px-4 text-sm font-semibold text-white hover:bg-[#1E40AF]"
              >
                {copy.retry}
              </a>
            </div>
          ) : showNeeds ? (
            <div className="overflow-hidden rounded-2xl border border-border bg-surface shadow-sm">
              <div className="grid gap-0 md:grid-cols-[0.9fr_1.3fr]">
                <div className="min-h-40 bg-[linear-gradient(145deg,#1D4ED8,#0E172A_55%,#F59E0B)]" />
                <div className="space-y-3 p-6 sm:p-8">
                  <Text as="h2" role="label">
                    {copy.needsTitle}
                  </Text>
                  <Text role="muted">{copy.needsBody}</Text>
                </div>
              </div>
            </div>
          ) : showEmpty ? (
            <div className="overflow-hidden rounded-2xl border border-border bg-surface shadow-sm">
              <div className="grid gap-0 md:grid-cols-[0.9fr_1.3fr]">
                <div className="min-h-40 bg-[linear-gradient(145deg,#1D4ED8_10%,#F59E0B_90%)] opacity-90" />
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
                {copy.count(filtered.length, destinationLabel)}
              </p>
              <ul className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
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
