import {
  Container,
  LtrValue,
  MoneyText,
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
import type { AppLocale } from "@/lib/i18n";
import type {
  PublicPriceSummaryView,
  TourDetailPageViewModel,
} from "./load-tour-detail";

/**
 * Public Tour commerce detail (TC-P30-T007).
 * Catalog + Pricing display · not Booking engine · no invented facts.
 */
export function TourDetailView({ vm }: { vm: TourDetailPageViewModel }) {
  const locale = vm.locale;
  const bookHref = `/${locale}/tours/${encodeURIComponent(vm.slug)}/book`;

  const copy =
    locale === "fa"
      ? {
          gallery: "گالری",
          noGallery: "گالری تصاویر هنوز برای این تور منتشر نشده است.",
          summary: "خلاصه تور",
          destinations: "مقصدها",
          noDestinations: "مقصدی ثبت نشده است.",
          destinationCount: (n: number) => `${n} مقصد ثبت‌شده`,
          origin: "مبدأ ثبت‌شده",
          departures: "تاریخ‌های حرکت",
          departuresNote: "اطلاعات منتشرشده · انتشار ≠ رزرو قطعی",
          noDepartures: "فعلاً تاریخ حرکت منتشرشده‌ای نیست.",
          schedule: "برنامه",
          duration: "مدت",
          days: "روز",
          transport: "پرواز / حمل‌ونقل پکیج",
          transportNote: "خلاصه پکیج · نه موجودی زنده پرواز",
          stay: "اقامت",
          stayNights: "شب",
          price: "قیمت",
          priceNote: "خلاصه قیمت عمومی · نه پیش‌فاکتور خرید",
          from: "از",
          noPrice: "فعلاً قیمت عمومی ثبت نشده است.",
          occupancy: "نرخ اشغال",
          components: "اجزای قیمت",
          policies: "قوانین و الزامات",
          noPolicies: "قانونی ثبت نشده است.",
          trust: "اعتماد و رزرو",
          trustBody:
            "انتشار کاتالوگ به معنای موجودی لحظه‌ای یا پرداخت قطعی نیست. مسیر رزرو از دکمه زیر آغاز می‌شود.",
          request: "درخواست اطلاعات",
          requestBody: "برای پرسش درباره این تور · نه پرداخت.",
        }
      : locale === "ar"
        ? {
            gallery: "المعرض",
            noGallery: "معرض الصور غير منشور بعد لهذه الجولة.",
            summary: "ملخص الجولة",
            destinations: "الوجهات",
            noDestinations: "لا وجهات مسجلة.",
            destinationCount: (n: number) => `${n} وجهة مسجلة`,
            origin: "منشأ مسجل",
            departures: "تواريخ المغادرة",
            departuresNote: "معلومات منشورة · النشر ≠ حجز مؤكد",
            noDepartures: "لا تواريخ مغادرة منشورة حالياً.",
            schedule: "الجدول",
            duration: "المدة",
            days: "أيام",
            transport: "الطيران / نقل الباقة",
            transportNote: "ملخص الباقة · ليس مخزون طيران حي",
            stay: "الإقامة",
            stayNights: "ليالٍ",
            price: "السعر",
            priceNote: "ملخص سعر عام · ليس عرض شراء",
            from: "من",
            noPrice: "لا أسعار عامة مسجلة بعد.",
            occupancy: "أسعار الإشغال",
            components: "مكونات السعر",
            policies: "السياسات والمتطلبات",
            noPolicies: "لا سياسات منشورة.",
            trust: "الثقة والحجز",
            trustBody:
              "نشر الكتالوج لا يعني توفراً لحظياً أو دفعاً مؤكداً. يبدأ مسار الحجز من الزر أدناه.",
            request: "طلب معلومات",
            requestBody: "للاستفسار عن هذه الجولة · ليس دفعاً.",
          }
        : {
            gallery: "Gallery",
            noGallery: "Photo gallery is not published for this tour yet.",
            summary: "Tour summary",
            destinations: "Destinations",
            noDestinations: "No destinations published.",
            destinationCount: (n: number) => `${n} destination(s) recorded`,
            origin: "Origin recorded",
            departures: "Departures",
            departuresNote: "Published facts · published ≠ confirmed booking",
            noDepartures: "No published departures yet.",
            schedule: "Schedule",
            duration: "Duration",
            days: "days",
            transport: "Package flight / transport",
            transportNote: "Package summary · not live flight inventory",
            stay: "Stay",
            stayNights: "nights",
            price: "Price",
            priceNote: "Public price summary · not a purchase quote",
            from: "From",
            noPrice: "No public price facts yet.",
            occupancy: "Occupancy prices",
            components: "Price components",
            policies: "Policies & requirements",
            noPolicies: "No policies published.",
            trust: "Trust & booking",
            trustBody:
              "Catalog publication is not live availability or confirmed payment. Start booking from the action below.",
            request: "Request information",
            requestBody: "Ask about this tour · not payment.",
          };

  const galleryItems =
    vm.gallery.length > 0
      ? vm.gallery
      : vm.cover
        ? [vm.cover]
        : [];

  const priceRows = vm.publishedDepartures.flatMap((d) => {
    const money = d.priceSummary ? startingMoney(d.priceSummary) : null;
    return money
      ? [
          {
            id: d.id,
            startDate: d.startDate,
            amount: money.amount,
            currency: money.currency,
          },
        ]
      : [];
  });

  return (
    <div className="pb-28 pt-6 sm:pt-8 lg:pb-8">
      <Container width="content">
        <Stack gap="lg">
          <section aria-labelledby="tour-gallery-title">
            <h2
              id="tour-gallery-title"
              className="mb-3 text-lg font-semibold tracking-tight text-foreground"
            >
              {copy.gallery}
            </h2>
            {galleryItems.length > 0 ? (
              <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {galleryItems.map((item) =>
                  item.src ? (
                    <li key={item.mediaAssetId}>
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img
                        src={item.src}
                        alt={item.alt || vm.name}
                        width={item.width ?? 960}
                        height={item.height ?? 540}
                        className="aspect-video w-full rounded-xl object-cover"
                      />
                    </li>
                  ) : null,
                )}
              </ul>
            ) : (
              <div className="flex aspect-video items-center justify-center rounded-xl border border-dashed border-border bg-gradient-to-br from-primary/20 via-muted to-accent/30 p-6">
                <Text role="muted">{copy.noGallery}</Text>
              </div>
            )}
          </section>

          <Stack gap="sm">
            <h1 className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">
              {vm.name}
            </h1>
            <Text role="muted">
              {copy.summary} · {vm.kind} · <LtrValue>{vm.code}</LtrValue>
            </Text>
            {vm.description ? <Text as="p">{vm.description}</Text> : null}
          </Stack>

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
                      {copy.origin}: <LtrValue>{vm.originDestinationId}</LtrValue>
                    </Text>
                  ) : null}
                  {vm.destinationIds.length > 0 ? (
                    <Text>{copy.destinationCount(vm.destinationIds.length)}</Text>
                  ) : null}
                </>
              )}
            </Stack>
          </Surface>

          {vm.kind === "Experience" ? (
            <ExperienceTourDetailSections
              locale={locale}
              experience={vm.experience}
            />
          ) : null}

          <div id="published-departures">
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.departures}
              </Text>
              <Text role="caption">{copy.departuresNote}</Text>
              {vm.publishedDepartures.length === 0 ? (
                <Text role="muted">{copy.noDepartures}</Text>
              ) : (
                <ul className="flex flex-col gap-3">
                  {vm.publishedDepartures.map((d) => (
                    <li key={d.id}>
                      <Surface>
                        <Stack gap="sm">
                          <Text>
                            {copy.schedule}:{" "}
                            <LtrValue>
                              {d.startDate ?? "—"} → {d.endDate ?? "—"}
                            </LtrValue>
                            {d.durationDays != null
                              ? ` · ${copy.duration} ${d.durationDays} ${copy.days}`
                              : null}
                          </Text>
                          {d.transport.length > 0 ? (
                            <Stack gap="sm">
                              <Text role="label">{copy.transport}</Text>
                              <Text role="caption">{copy.transportNote}</Text>
                              <ul className="flex flex-wrap gap-2 text-sm">
                                {d.transport.map((t) => (
                                  <li
                                    key={`${d.id}-t-${t.sequence}`}
                                    className="rounded-full border border-border bg-background px-3 py-1"
                                  >
                                    <LtrValue>
                                      {t.transportMode}: {t.origin} →{" "}
                                      {t.destination}
                                    </LtrValue>
                                  </li>
                                ))}
                              </ul>
                            </Stack>
                          ) : null}
                          {d.accommodation.length > 0 ? (
                            <Stack gap="sm">
                              <Text role="label">{copy.stay}</Text>
                              <ul className="flex flex-wrap gap-2 text-sm">
                                {d.accommodation.map((a) => (
                                  <li
                                    key={`${d.id}-a-${a.placeId}-${a.nights}`}
                                    className="rounded-full border border-border bg-background px-3 py-1"
                                  >
                                    {a.nights} {copy.stayNights}
                                    {a.boardType ? ` · ${a.boardType}` : ""}
                                  </li>
                                ))}
                              </ul>
                            </Stack>
                          ) : null}
                          {d.priceSummary ? (
                            <DeparturePriceFacts
                              locale={locale}
                              summary={d.priceSummary}
                              priceLabel={copy.price}
                              fromLabel={copy.from}
                              occupancyLabel={copy.occupancy}
                              componentsLabel={copy.components}
                            />
                          ) : null}
                        </Stack>
                      </Surface>
                    </li>
                  ))}
                </ul>
              )}
            </Stack>
          </div>

          <div id="price-from">
            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {copy.price}
                </Text>
                <Text role="caption">{copy.priceNote}</Text>
                {priceRows.length === 0 ? (
                  <Text role="muted">{copy.noPrice}</Text>
                ) : (
                  <ul className="flex flex-col gap-2">
                    {priceRows.map((row) => (
                      <li key={row.id} className="flex flex-wrap items-baseline gap-2">
                        <Text>{copy.from}</Text>
                        <MoneyText
                          locale={locale}
                          money={{
                            amount: String(row.amount),
                            currencyCode: row.currency,
                          }}
                          className="text-lg font-semibold"
                        />
                        {row.startDate ? (
                          <Text role="caption">
                            <LtrValue>{row.startDate}</LtrValue>
                          </Text>
                        ) : null}
                      </li>
                    ))}
                  </ul>
                )}
              </Stack>
            </Surface>
          </div>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.policies}
              </Text>
              {vm.policies.length === 0 && vm.requirements.length === 0 ? (
                <Text role="muted">{copy.noPolicies}</Text>
              ) : (
                <ul className="list-inside list-disc text-sm">
                  {vm.policies.map((item) => (
                    <li key={`p-${item.code}`}>
                      {item.code}
                      {item.detail ? ` · ${item.detail}` : ""}
                    </li>
                  ))}
                  {vm.requirements.map((item) => (
                    <li key={`r-${item.code}`}>
                      {item.code}
                      {item.detail ? ` · ${item.detail}` : ""}
                    </li>
                  ))}
                </ul>
              )}
            </Stack>
          </Surface>

          <Surface tone="muted">
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.trust}
              </Text>
              <Text>{copy.trustBody}</Text>
            </Stack>
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
      <PublicDetailStickyActions locale={locale} bookHref={bookHref} />
    </div>
  );
}

function startingMoney(summary: PublicPriceSummaryView): {
  amount: number;
  currency: string;
} | null {
  if (summary.occupancyPrices.length > 0) {
    const lowest = summary.occupancyPrices.reduce((current, row) =>
      row.money.amount < current.money.amount ? row : current,
    );
    return { amount: lowest.money.amount, currency: summary.currency };
  }

  const base = summary.components.find((c) => c.kind === "Base");
  if (base) {
    return { amount: base.money.amount, currency: summary.currency };
  }

  return null;
}

function categoryLabel(locale: AppLocale, value: string): string {
  if (locale !== "fa") {
    return value;
  }

  switch (value) {
    case "Adult":
      return "بزرگسال";
    case "ChildWithBed":
      return "کودک با تخت";
    case "ChildWithoutBed":
      return "کودک بدون تخت";
    case "SingleRoom":
      return "یک‌تخته";
    case "DoubleRoom":
      return "دو‌تخته";
    case "TwinRoom":
      return "تویین";
    case "Base":
      return "پایه";
    case "Fee":
      return "کارمزد";
    case "Tax":
      return "مالیات";
    default:
      return value;
  }
}

function DeparturePriceFacts({
  locale,
  summary,
  priceLabel,
  fromLabel,
  occupancyLabel,
  componentsLabel,
}: {
  locale: AppLocale;
  summary: PublicPriceSummaryView;
  priceLabel: string;
  fromLabel: string;
  occupancyLabel: string;
  componentsLabel: string;
}) {
  const starting = startingMoney(summary);

  return (
    <Stack gap="sm">
      <Text role="label">{priceLabel}</Text>
      {starting ? (
        <div className="flex flex-wrap items-baseline gap-2">
          <Text>{fromLabel}</Text>
          <MoneyText
            locale={locale}
            money={{
              amount: String(starting.amount),
              currencyCode: starting.currency,
            }}
            className="font-semibold"
          />
        </div>
      ) : null}
      {summary.occupancyPrices.length > 0 ? (
        <Stack gap="sm">
          <Text role="caption">{occupancyLabel}</Text>
          <ul className="list-inside list-disc text-sm">
            {summary.occupancyPrices.map((row) => (
              <li
                key={`${summary.priceId}-${row.passengerCategory}-${row.occupancyCategory}`}
              >
                {categoryLabel(locale, row.passengerCategory)} ·{" "}
                {categoryLabel(locale, row.occupancyCategory)} ·{" "}
                <MoneyText
                  locale={locale}
                  money={{
                    amount: String(row.money.amount),
                    currencyCode: row.money.currencyCode,
                  }}
                />
              </li>
            ))}
          </ul>
        </Stack>
      ) : null}
      {summary.components.length > 0 ? (
        <Stack gap="sm">
          <Text role="caption">{componentsLabel}</Text>
          <ul className="list-inside list-disc text-sm">
            {summary.components.map((component, index) => (
              <li key={`${summary.priceId}-c-${component.kind}-${index}`}>
                {categoryLabel(locale, component.kind)} ·{" "}
                <MoneyText
                  locale={locale}
                  money={{
                    amount: String(component.money.amount),
                    currencyCode: component.money.currencyCode,
                  }}
                />
              </li>
            ))}
          </ul>
        </Stack>
      ) : null}
    </Stack>
  );
}
