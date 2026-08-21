import Link from "next/link";
import { Container, Stack, Surface, Text } from "@/components/ui";
import { AgencyOffersList } from "@/features/public-experience/agency-offers-list";
import { PublicDetailStickyActions } from "@/features/public-experience/detail-sticky-actions";
import { ExperienceTourDetailSections } from "@/features/public-experience/experience-detail-sections";
import { RelatedContentList } from "@/features/public-experience/related-content-list";
import { RelatedToursList } from "@/features/public-experience/related-tours-list";
import { UgcCompositionList } from "@/features/public-experience/ugc-composition-list";
import { TourCommercePanel } from "./tour-commerce-panel";
import type { TourDetailPageViewModel } from "./load-tour-detail";

function isNoiseDescription(value: string | null | undefined): boolean {
  if (!value) return true;
  const v = value.trim().toLowerCase();
  return (
    v.length === 0 ||
    v.includes("demofeed sample data") ||
    v === "non-production" ||
    v.includes("undefined") ||
    v.includes("null")
  );
}

/**
 * Public Tour commerce detail (TC-P36-T004 polish).
 * Catalog + Published Departure + Pricing summary → book prepare (Pending initiation).
 * No Payment · no Confirmed theater · no invented money · no UUID leakage.
 */
export function TourDetailView({ vm }: { vm: TourDetailPageViewModel }) {
  const locale = vm.locale;
  const isDemo =
    vm.slug.startsWith("demofeed-") || vm.code.startsWith("demofeed-");

  const copy =
    locale === "fa"
      ? {
          eyebrow: "پکیج تور",
          gallery: "گالری",
          noGallery: "گالری تصاویر هنوز برای این تور منتشر نشده است.",
          summary: "درباره این تور",
          destinations: "مقصدها",
          noDestinations: "مقصدی ثبت نشده است.",
          destinationCount: (n: number) =>
            n === 1 ? "۱ مقصد در این پکیج" : `${n} مقصد در این پکیج`,
          included: "خدمات و الزامات",
          noPolicies: "قانون یا الزامی منتشر نشده است.",
          trust: "مسیر فروش صادقانه",
          trustBody:
            "این صفحه کاتالوگ تور است — موجودی لحظه‌ای یا پرداخت قطعی اینجا ادعا نمی‌شود. حرکت و قیمت از APIهای منتشرشده می‌آیند.",
          request: "درخواست اطلاعات",
          requestBody: "برای پرسش درباره این تور · نه پرداخت · نه ایجاد رزرو.",
          demoHint: "نمونه کاتالوگ",
          back: "بازگشت به تورها",
          ctaNote:
            "انتخاب حرکت → خلاصه قیمت → شروع رزرو موقت · بدون پرداخت و بدون Confirmed",
        }
      : locale === "ar"
        ? {
            eyebrow: "باقة جولة",
            gallery: "المعرض",
            noGallery: "معرض الصور غير منشور بعد لهذه الجولة.",
            summary: "عن هذه الجولة",
            destinations: "الوجهات",
            noDestinations: "لا وجهات مسجلة.",
            destinationCount: (n: number) =>
              n === 1 ? "وجهة واحدة في هذه الباقة" : `${n} وجهات في هذه الباقة`,
            included: "الخدمات والمتطلبات",
            noPolicies: "لا سياسات أو متطلبات منشورة.",
            trust: "مسار بيع صادق",
            trustBody:
              "هذه صفحة كتالوج الجولة — لا ندّعي توفراً لحظياً أو دفعاً مؤكداً هنا. المغادرة والسعر من واجهات منشورة.",
            request: "طلب معلومات",
            requestBody: "للاستفسار عن هذه الجولة · ليس دفعاً · بلا إنشاء حجز.",
            demoHint: "عينة الكتالوج",
            back: "العودة إلى الجولات",
            ctaNote:
              "اختر المغادرة → ملخص السعر → ابدأ حجزاً معلقاً · بلا دفع وبلا تأكيد",
          }
        : {
            eyebrow: "Tour package",
            gallery: "Gallery",
            noGallery: "Photo gallery is not published for this tour yet.",
            summary: "About this tour",
            destinations: "Destinations",
            noDestinations: "No destinations published.",
            destinationCount: (n: number) =>
              n === 1
                ? "1 destination in this package"
                : `${n} destinations in this package`,
            included: "Services & requirements",
            noPolicies: "No policies or requirements published.",
            trust: "Honest sell path",
            trustBody:
              "This is a tour catalog surface — we do not claim live availability or confirmed payment here. Departures and prices come from published APIs.",
            request: "Request information",
            requestBody: "Ask about this tour · not payment · not booking create.",
            demoHint: "Sample catalog",
            back: "Back to tours",
            ctaNote:
              "Select departure → price summary → start Pending booking · no payment · not Confirmed",
          };

  const galleryItems =
    vm.gallery.length > 0 ? vm.gallery : vm.cover ? [vm.cover] : [];
  const hero = galleryItems[0] ?? null;
  const thumbs = galleryItems.slice(1, 5);
  const showDescription = !isNoiseDescription(vm.description);
  const destinationCount = vm.destinationIds.length;
  const hasPolicies =
    vm.policies.length > 0 || vm.requirements.length > 0;
  const hasUgc =
    vm.ugcComposition.reviews.length > 0 ||
    vm.ugcComposition.travelogues.length > 0 ||
    vm.ugcComposition.userPhotos.length > 0;

  return (
    <div className="pb-28">
      <section className="relative isolate overflow-hidden border-b border-border">
        <div className="absolute inset-0 bg-[#0E172A]" aria-hidden />
        {hero?.src ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={hero.src}
            alt=""
            className="absolute inset-0 h-full w-full object-cover opacity-50"
          />
        ) : null}
        <div
          aria-hidden
          className="absolute inset-0 bg-gradient-to-r from-[#0E172A]/95 via-[#0E172A]/78 to-[#0E172A]/45"
        />
        <Container width="wide" className="relative py-8 sm:py-10">
          <Link
            href={`/${locale}/tours`}
            className="text-xs font-medium text-white/75 underline-offset-2 hover:text-white hover:underline"
          >
            {copy.back}
          </Link>
          <div className="mt-3 flex flex-wrap items-center gap-2">
            {vm.kind ? (
              <span className="rounded-full bg-white/15 px-2.5 py-1 text-xs font-medium text-white">
                {vm.kind}
              </span>
            ) : null}
            {isDemo ? (
              <span className="rounded-full bg-black/35 px-2.5 py-1 text-[10px] font-medium tracking-wide text-white/90">
                {copy.demoHint}
              </span>
            ) : null}
          </div>
          <h1 className="mt-3 max-w-3xl text-3xl font-semibold tracking-tight text-white sm:text-4xl">
            {vm.name}
          </h1>
          <p className="mt-2 text-sm text-white/85">{copy.eyebrow}</p>
        </Container>
      </section>

      <Container width="wide" className="pt-6 sm:pt-8">
        <Stack gap="xl">
          <section aria-labelledby="tour-gallery-title">
            <h2
              id="tour-gallery-title"
              className="mb-3 text-lg font-semibold tracking-tight text-foreground"
            >
              {copy.gallery}
            </h2>
            {hero?.src ? (
              <div className="grid gap-3 lg:grid-cols-[1.7fr_0.8fr]">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={hero.src}
                  alt={hero.alt || vm.name}
                  width={hero.width ?? 1200}
                  height={hero.height ?? 675}
                  className="aspect-[16/10] w-full rounded-2xl object-cover shadow-sm"
                />
                {thumbs.length > 0 ? (
                  <ul className="grid grid-cols-2 gap-3 content-start">
                    {thumbs.map((item) =>
                      item.src ? (
                        <li key={item.mediaAssetId}>
                          {/* eslint-disable-next-line @next/next/no-img-element */}
                          <img
                            src={item.src}
                            alt={item.alt || vm.name}
                            width={item.width ?? 640}
                            height={item.height ?? 360}
                            className="aspect-video w-full rounded-xl object-cover"
                          />
                        </li>
                      ) : null,
                    )}
                  </ul>
                ) : (
                  <Surface className="flex min-h-40 items-center justify-center rounded-2xl border-dashed p-4">
                    <Text role="muted" className="text-center text-sm">
                      {copy.trustBody}
                    </Text>
                  </Surface>
                )}
              </div>
            ) : (
              <div className="flex aspect-[16/9] items-center justify-center rounded-2xl border border-dashed border-border bg-muted/30 p-6">
                <Text role="muted">{copy.noGallery}</Text>
              </div>
            )}
          </section>

          <div className="grid gap-5 lg:grid-cols-[1.4fr_0.8fr]">
            <section aria-labelledby="tour-summary-title" className="space-y-3">
              <h2
                id="tour-summary-title"
                className="text-xl font-semibold tracking-tight text-foreground"
              >
                {copy.summary}
              </h2>
              {showDescription ? (
                <p className="text-sm leading-relaxed text-foreground/90 sm:text-base">
                  {vm.description}
                </p>
              ) : (
                <p className="text-sm leading-relaxed text-muted-foreground">
                  {copy.trustBody}
                </p>
              )}
            </section>

            <div className="space-y-4">
              {destinationCount > 0 ? (
                <Surface className="rounded-2xl p-5">
                  <h2 className="text-sm font-semibold text-[#1D4ED8]">
                    {copy.destinations}
                  </h2>
                  <p className="mt-2 text-sm text-foreground">
                    {copy.destinationCount(destinationCount)}
                  </p>
                  <Link
                    href={`/${locale}`}
                    className="mt-3 inline-flex text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
                  >
                    {locale === "fa"
                      ? "بازگشت به صفحه اصلی"
                      : locale === "ar"
                        ? "العودة إلى الرئيسية"
                        : "Back to home"}
                  </Link>
                </Surface>
              ) : null}

              {hasPolicies ? (
                <Surface className="rounded-2xl p-5">
                  <h2 className="text-sm font-semibold text-[#1D4ED8]">
                    {copy.included}
                  </h2>
                  <ul className="mt-3 flex flex-wrap gap-2 text-sm">
                    {vm.policies.map((item) => (
                      <li
                        key={`p-${item.code}`}
                        className="rounded-full border border-border bg-background px-3 py-1"
                      >
                        {item.code}
                        {item.detail ? ` · ${item.detail}` : ""}
                      </li>
                    ))}
                    {vm.requirements.map((item) => (
                      <li
                        key={`r-${item.code}`}
                        className="rounded-full border border-border bg-background px-3 py-1"
                      >
                        {item.code}
                        {item.detail ? ` · ${item.detail}` : ""}
                      </li>
                    ))}
                  </ul>
                </Surface>
              ) : null}
            </div>
          </div>

          {vm.kind === "Experience" ? (
            <ExperienceTourDetailSections
              locale={locale}
              experience={vm.experience}
            />
          ) : null}

          <TourCommercePanel
            locale={locale}
            slug={vm.slug}
            departures={vm.publishedDepartures}
          />

          <Surface className="rounded-2xl border-[#1D4ED8]/15 bg-gradient-to-br from-surface to-[#1D4ED8]/[0.04] p-5">
            <p className="text-sm font-semibold text-[#1D4ED8]">{copy.trust}</p>
            <p className="mt-2 text-sm text-muted-foreground">{copy.trustBody}</p>
            <p className="mt-3 text-xs text-muted-foreground">{copy.ctaNote}</p>
          </Surface>

          {vm.agencyOffers.length > 0 ? (
            <AgencyOffersList locale={locale} items={vm.agencyOffers} />
          ) : null}

          {hasUgc ? (
            <UgcCompositionList
              locale={locale}
              composition={vm.ugcComposition}
            />
          ) : null}

          <div id="request-information">
            <Surface className="rounded-2xl p-5">
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {copy.request}
                </Text>
                <Text>{copy.requestBody}</Text>
              </Stack>
            </Surface>
          </div>

          {vm.relatedContent.length > 0 ? (
            <RelatedContentList locale={locale} items={vm.relatedContent} />
          ) : null}
          {vm.relatedTours.length > 0 ? (
            <RelatedToursList locale={locale} items={vm.relatedTours} />
          ) : null}
        </Stack>
      </Container>
      <PublicDetailStickyActions
        locale={locale}
        bookHref={
          vm.publishedDepartures.length > 0
            ? `/${locale}/tours/${encodeURIComponent(vm.slug)}/book`
            : undefined
        }
      />
    </div>
  );
}
