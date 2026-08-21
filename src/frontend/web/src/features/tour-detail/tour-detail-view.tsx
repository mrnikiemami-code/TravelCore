import {
  Container,
  LtrValue,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import { AgencyOffersList } from "@/features/public-experience/agency-offers-list";
import { PublicDetailStickyActions } from "@/features/public-experience/detail-sticky-actions";
import { ExperienceTourDetailSections } from "@/features/public-experience/experience-detail-sections";
import { RelatedContentList } from "@/features/public-experience/related-content-list";
import { RelatedToursList } from "@/features/public-experience/related-tours-list";
import { UgcCompositionList } from "@/features/public-experience/ugc-composition-list";
import { TourCommercePanel } from "./tour-commerce-panel";
import type { TourDetailPageViewModel } from "./load-tour-detail";

/**
 * Public Tour commerce detail (TC-P30-T007 · TC-P31-T005 · TC-P33-T006 I2).
 * Catalog + Published Departure selection + Pricing summary · booking-boundary CTA.
 * No Booking create · no Payment · no invented money.
 */
export function TourDetailView({ vm }: { vm: TourDetailPageViewModel }) {
  const locale = vm.locale;
  const isDemo =
    vm.slug.startsWith("demofeed-") || vm.code.startsWith("demofeed-");

  const copy =
    locale === "fa"
      ? {
          eyebrow: "Tour commerce",
          gallery: "گالری",
          noGallery: "گالری تصاویر هنوز برای این تور منتشر نشده است.",
          summary: "خلاصه تور",
          destinations: "مقصدها",
          noDestinations: "مقصدی ثبت نشده است.",
          destinationCount: (n: number) => `${n} مقصد ثبت‌شده`,
          origin: "مبدأ ثبت‌شده",
          included: "خدمات و الزامات",
          policies: "قوانین و الزامات",
          noPolicies: "قانونی ثبت نشده است.",
          trust: "اعتماد و شفافیت",
          trustBody:
            "این صفحه کاتالوگ تور است — موجودی لحظه‌ای، کمیابی یا پرداخت قطعی اینجا ادعا نمی‌شود. حرکت و قیمت از APIهای منتشرشده می‌آیند.",
          request: "درخواست اطلاعات",
          requestBody: "برای پرسش درباره این تور · نه پرداخت · نه ایجاد رزرو.",
          demoHint: "نمونه DEMOFEED",
          ctaNote:
            "ترکیب عمومی: انتخاب حرکت → خلاصه قیمت · مرز رزرو بدون ایجاد Booking",
        }
      : locale === "ar"
        ? {
            eyebrow: "Tour commerce",
            gallery: "المعرض",
            noGallery: "معرض الصور غير منشور بعد لهذه الجولة.",
            summary: "ملخص الجولة",
            destinations: "الوجهات",
            noDestinations: "لا وجهات مسجلة.",
            destinationCount: (n: number) => `${n} وجهة مسجلة`,
            origin: "منشأ مسجل",
            included: "الخدمات والمتطلبات",
            policies: "السياسات والمتطلبات",
            noPolicies: "لا سياسات منشورة.",
            trust: "الثقة والشفافية",
            trustBody:
              "هذه صفحة كتالوج الجولة — لا ندّعي توفراً لحظياً أو ندرة أو دفعاً مؤكداً هنا. المغادرة والسعر من واجهات منشورة.",
            request: "طلب معلومات",
            requestBody: "للاستفسار عن هذه الجولة · ليس دفعاً · بلا إنشاء حجز.",
            demoHint: "عينة DEMOFEED",
            ctaNote:
              "تركيب عام: اختيار المغادرة → ملخص السعر · حدود الحجز دون إنشاء Booking",
          }
        : {
            eyebrow: "Tour commerce",
            gallery: "Gallery",
            noGallery: "Photo gallery is not published for this tour yet.",
            summary: "Tour summary",
            destinations: "Destinations",
            noDestinations: "No destinations published.",
            destinationCount: (n: number) => `${n} destination(s) recorded`,
            origin: "Origin recorded",
            included: "Services & requirements",
            policies: "Policies & requirements",
            noPolicies: "No policies published.",
            trust: "Trust & transparency",
            trustBody:
              "This is a tour catalog surface — we do not claim live availability, scarcity, or confirmed payment here. Departures and prices come from published APIs.",
            request: "Request information",
            requestBody: "Ask about this tour · not payment · not booking create.",
            demoHint: "DEMOFEED sample",
            ctaNote:
              "Public composition: select departure → price summary · booking boundary without Booking create",
          };

  const galleryItems =
    vm.gallery.length > 0 ? vm.gallery : vm.cover ? [vm.cover] : [];
  const hero = galleryItems[0] ?? null;
  const thumbs = galleryItems.slice(1, 5);

  return (
    <div className="pb-28">
      <section className="border-b border-border bg-gradient-to-br from-primary/95 via-primary to-primary/80 text-primary-foreground">
        <Container width="wide" className="py-6 sm:py-8">
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-accent">
            {copy.eyebrow}
          </p>
          <h1 className="mt-2 text-2xl font-semibold tracking-tight sm:text-4xl">
            {vm.name}
          </h1>
          <p className="mt-2 text-sm text-primary-foreground/90">
            {copy.summary}
            {" · "}
            {vm.kind}
            {" · "}
            <LtrValue>{vm.code}</LtrValue>
            {isDemo ? (
              <>
                {" · "}
                {copy.demoHint}
              </>
            ) : null}
          </p>
        </Container>
      </section>

      <Container width="wide" className="pt-6 sm:pt-8">
        <Stack gap="lg">
          <section aria-labelledby="tour-gallery-title">
            <h2
              id="tour-gallery-title"
              className="mb-3 text-lg font-semibold tracking-tight text-foreground"
            >
              {copy.gallery}
            </h2>
            {hero?.src ? (
              <div className="grid gap-3 lg:grid-cols-[1.6fr_0.8fr]">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={hero.src}
                  alt={hero.alt || vm.name}
                  width={hero.width ?? 1200}
                  height={hero.height ?? 675}
                  className="aspect-[16/10] w-full rounded-2xl object-cover shadow-sm"
                />
                <ul className="grid grid-cols-2 gap-3">
                  {thumbs.length > 0
                    ? thumbs.map((item) =>
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
                      )
                    : [0, 1, 2, 3].map((i) => (
                        <li
                          key={i}
                          aria-hidden
                          className="aspect-video rounded-xl bg-gradient-to-br from-surface-muted to-primary/20"
                        />
                      ))}
                </ul>
              </div>
            ) : (
              <div className="flex aspect-[16/9] items-center justify-center rounded-2xl border border-dashed border-border bg-muted/30 p-6">
                <Text role="muted">{copy.noGallery}</Text>
              </div>
            )}
          </section>

          <section aria-labelledby="tour-summary-title">
            <Stack gap="sm">
              <h2
                id="tour-summary-title"
                className="text-xl font-semibold tracking-tight text-foreground"
              >
                {copy.summary}
              </h2>
              {vm.description ? <Text as="p">{vm.description}</Text> : null}
            </Stack>
          </section>

          <div className="grid gap-4 lg:grid-cols-2">
            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {copy.destinations}
                </Text>
                {vm.destinationIds.length === 0 && !vm.originDestinationId ? (
                  <Text role="muted">{copy.noDestinations}</Text>
                ) : (
                  <>
                    {vm.originDestinationId ? (
                      <Text role="caption">
                        {copy.origin}:{" "}
                        <LtrValue>{vm.originDestinationId}</LtrValue>
                      </Text>
                    ) : null}
                    {vm.destinationIds.length > 0 ? (
                      <Text>
                        {copy.destinationCount(vm.destinationIds.length)}
                      </Text>
                    ) : null}
                    {vm.destinationIds.length > 0 ? (
                      <ul className="flex flex-wrap gap-2 text-sm">
                        {vm.destinationIds.map((id) => (
                          <li
                            key={id}
                            className="rounded-full border border-border bg-background px-3 py-1"
                          >
                            <LtrValue>{id}</LtrValue>
                          </li>
                        ))}
                      </ul>
                    ) : null}
                  </>
                )}
              </Stack>
            </Surface>

            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {copy.included}
                </Text>
                {vm.policies.length === 0 && vm.requirements.length === 0 ? (
                  <Text role="muted">{copy.noPolicies}</Text>
                ) : (
                  <ul className="flex flex-wrap gap-2 text-sm">
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
                )}
              </Stack>
            </Surface>
          </div>

          {vm.kind === "Experience" ? (
            <ExperienceTourDetailSections
              locale={locale}
              experience={vm.experience}
            />
          ) : null}

          <TourCommercePanel
            locale={locale}
            departures={vm.publishedDepartures}
          />

          <Surface className="border-primary/15 bg-gradient-to-br from-surface to-primary/5">
            <Text as="h2" role="heading" className="text-primary">
              {copy.trust}
            </Text>
            <Text role="muted" className="mt-2">
              {copy.trustBody}
            </Text>
            <Text role="caption" className="mt-3">
              {copy.ctaNote}
            </Text>
          </Surface>

          <AgencyOffersList locale={locale} items={vm.agencyOffers} />
          <UgcCompositionList locale={locale} composition={vm.ugcComposition} />

          <div id="request-information">
            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {copy.request}
                </Text>
                <Text>{copy.requestBody}</Text>
              </Stack>
            </Surface>
          </div>

          <RelatedContentList locale={locale} items={vm.relatedContent} />
          <RelatedToursList locale={locale} items={vm.relatedTours} />
        </Stack>
      </Container>
      {/* I2: no bookHref — sticky stays presentation-only (I3 owns booking initiate). */}
      <PublicDetailStickyActions locale={locale} />
    </div>
  );
}
