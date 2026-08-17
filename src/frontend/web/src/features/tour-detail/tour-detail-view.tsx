import { Container, LtrValue, Stack, Text } from "@/components/ui";
import { PublicDetailStickyActions } from "@/features/public-experience/detail-sticky-actions";
import { ExperienceTourDetailSections } from "@/features/public-experience/experience-detail-sections";
import { RelatedContentList } from "@/features/public-experience/related-content-list";
import { RelatedToursList } from "@/features/public-experience/related-tours-list";
import type { AppLocale } from "@/lib/i18n";
import type {
  PublicPriceSummaryView,
  TourDetailPageViewModel,
} from "./load-tour-detail";

/**
 * Shared public Tour Detail shell (TC-P14-T004 / P14-R4).
 * Kind-specific Experience sections compose in; Package specialty is not implemented.
 * Catalog Published ≠ bookable. Sticky actions are presentation only (P14-R2).
 * App-proxy media only. Cover + ordered Gallery (no hero role).
 */
export function TourDetailView({ vm }: { vm: TourDetailPageViewModel }) {
  const locale = vm.locale;
  const departuresHeading =
    locale === "fa" ? "اجراهای منتشرشده" : "Published departures";
  const noDepartures =
    locale === "fa"
      ? "فعلاً اجرای منتشرشده‌ای ثبت نشده است."
      : "No published departures yet.";
  const scheduleLabel = locale === "fa" ? "برنامه" : "Schedule";
  const capacityLabel = locale === "fa" ? "ظرفیت برنامه‌ای" : "Planned capacity";
  const transportLabel = locale === "fa" ? "حمل‌ونقل" : "Transport";
  const stayLabel = locale === "fa" ? "اقامت" : "Stay";
  const daysLabel = locale === "fa" ? "روز" : "days";
  const priceLabel = locale === "fa" ? "قیمت" : "Price";
  const fromLabel = locale === "fa" ? "از" : "From";
  const occupancyLabel = locale === "fa" ? "نرخ اشغال" : "Occupancy prices";
  const componentsLabel = locale === "fa" ? "اجزای قیمت" : "Price components";
  const noPublicPrice =
    locale === "fa"
      ? "فعلاً قیمت عمومی ثبت نشده است."
      : "No public price facts yet.";
  const priceRows = vm.publishedDepartures.flatMap((d) => {
    const money = d.priceSummary ? startingMoney(d.priceSummary) : null;
    return money
      ? [{ id: d.id, startDate: d.startDate, amount: money.amount, currency: money.currency }]
      : [];
  });

  return (
    <div className="py-6 pb-28 sm:py-8 lg:pb-8">
      <Container width="content">
        <Stack gap="lg">
          {vm.cover?.src ? (
            // eslint-disable-next-line @next/next/no-img-element -- app-proxy public media
            <img
              src={vm.cover.src}
              alt={vm.cover.alt || vm.name}
              width={vm.cover.width ?? 960}
              height={vm.cover.height ?? 540}
              className="aspect-video w-full rounded-lg object-cover"
            />
          ) : null}

          <Stack gap="sm">
            <Text as="h1" role="heading">
              {vm.name}
            </Text>
            <Text role="caption">
              {vm.kind} · <LtrValue>{vm.code}</LtrValue> ·{" "}
              <LtrValue>{vm.slug}</LtrValue>
            </Text>
            {vm.description ? <Text as="p">{vm.description}</Text> : null}
          </Stack>

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "مقصدها" : "Destinations"}
            </Text>
            {vm.destinationIds.length === 0 && !vm.originDestinationId ? (
              <Text role="muted">
                {locale === "fa" ? "مقصدی ثبت نشده است." : "No destinations published."}
              </Text>
            ) : (
              <ul className="list-inside list-disc">
                {vm.originDestinationId ? (
                  <li>
                    {locale === "fa" ? "مبدأ" : "Origin"}:{" "}
                    <LtrValue>{vm.originDestinationId}</LtrValue>
                  </li>
                ) : null}
                {vm.destinationIds.map((id) => (
                  <li key={id}>
                    <LtrValue>{id}</LtrValue>
                  </li>
                ))}
              </ul>
            )}
          </Stack>

          {vm.kind === "Experience" ? (
            <ExperienceTourDetailSections locale={locale} experience={vm.experience} />
          ) : null}

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "آمادگی عرضه" : "Offer readiness"}
            </Text>
            <Text role="caption">
              {locale === "fa"
                ? "نمایش آگهی عمومی قفل نشده است."
                : "Public offer display is not locked yet."}
            </Text>
          </Stack>

          <div id="published-departures">
            <Stack gap="sm">
            <Text as="h2" role="heading">
              {departuresHeading}
            </Text>
            <Text role="caption">
              {locale === "fa"
                ? "نمایش اطلاعات اجرایی · بدون موتور فروش"
                : "Execution facts only · not a sales engine"}
            </Text>
            {vm.publishedDepartures.length === 0 ? (
              <Text role="muted">{noDepartures}</Text>
            ) : (
              <ul className="flex flex-col gap-3">
                {vm.publishedDepartures.map((d) => (
                  <li
                    key={d.id}
                    className="rounded-md border border-border p-3 text-sm"
                  >
                    <Stack gap="sm">
                      <LtrValue>
                        <Text role="caption">{d.id}</Text>
                      </LtrValue>
                      <Text>
                        {scheduleLabel}:{" "}
                        <LtrValue>
                          {d.startDate ?? "—"} → {d.endDate ?? "—"}
                          {d.timeZoneId ? ` · ${d.timeZoneId}` : ""}
                        </LtrValue>
                        {d.durationDays != null
                          ? ` · ${d.durationDays} ${daysLabel}`
                          : null}
                      </Text>
                      {(d.minimumPax != null || d.maximumPax != null) && (
                        <Text>
                          {capacityLabel}:{" "}
                          <LtrValue>
                            {d.minimumPax ?? "—"}–{d.maximumPax ?? "—"}
                          </LtrValue>
                        </Text>
                      )}
                      {d.transport.length > 0 ? (
                        <Stack gap="sm">
                          <Text role="label">{transportLabel}</Text>
                          <ul className="list-inside list-disc">
                            {d.transport.map((t) => (
                              <li key={`${d.id}-t-${t.sequence}`}>
                                <LtrValue>
                                  #{t.sequence} {t.transportMode}: {t.origin} →{" "}
                                  {t.destination}
                                </LtrValue>
                              </li>
                            ))}
                          </ul>
                        </Stack>
                      ) : null}
                      {d.accommodation.length > 0 ? (
                        <Stack gap="sm">
                          <Text role="label">{stayLabel}</Text>
                          <ul className="list-inside list-disc">
                            {d.accommodation.map((a) => (
                              <li key={`${d.id}-a-${a.placeId}-${a.nights}`}>
                                <LtrValue>
                                  {a.nights}n · {a.boardType} · place {a.placeId}
                                </LtrValue>
                              </li>
                            ))}
                          </ul>
                        </Stack>
                      ) : null}
                      {d.priceSummary ? (
                        <DeparturePriceFacts
                          locale={locale}
                          summary={d.priceSummary}
                          priceLabel={priceLabel}
                          fromLabel={fromLabel}
                          occupancyLabel={occupancyLabel}
                          componentsLabel={componentsLabel}
                        />
                      ) : null}
                    </Stack>
                  </li>
                ))}
              </ul>
            )}
            </Stack>
          </div>

          {vm.gallery.length > 0 ? (
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {locale === "fa" ? "گالری" : "Gallery"}
              </Text>
              <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {vm.gallery.map((item) =>
                  item.src ? (
                    <li key={item.mediaAssetId}>
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img
                        src={item.src}
                        alt={item.alt || vm.name}
                        width={item.width ?? 640}
                        height={item.height ?? 360}
                        className="aspect-video w-full rounded-md object-cover"
                      />
                    </li>
                  ) : null,
                )}
              </ul>
            </Stack>
          ) : null}

          <div id="price-from">
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {priceLabel}
              </Text>
              <Text role="caption">
                {locale === "fa"
                  ? "خلاصه قیمت عمومی · نه پیش‌فاکتور خرید"
                  : "Public price summary · not a purchase quote"}
              </Text>
              {priceRows.length === 0 ? (
                <Text role="muted">{noPublicPrice}</Text>
              ) : (
                <ul className="flex flex-col gap-2">
                  {priceRows.map((row) => (
                    <li key={row.id}>
                      <Text>
                        {fromLabel}{" "}
                        <LtrValue>
                          {row.amount} {row.currency}
                        </LtrValue>
                        {row.startDate ? (
                          <>
                            {" · "}
                            <LtrValue>{row.startDate}</LtrValue>
                          </>
                        ) : null}
                      </Text>
                    </li>
                  ))}
                </ul>
              )}
            </Stack>
          </div>

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "قوانین و الزامات" : "Policies"}
            </Text>
            {vm.policies.length === 0 && vm.requirements.length === 0 ? (
              <Text role="muted">
                {locale === "fa" ? "قانونی ثبت نشده است." : "No policies published."}
              </Text>
            ) : (
              <ul className="list-inside list-disc">
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

          <div id="request-information">
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {locale === "fa" ? "درخواست اطلاعات" : "Request information"}
              </Text>
              <Text>
                {locale === "fa"
                  ? "این اقدام برای دریافت اطلاعات است، نه رزرو و نه پرداخت."
                  : "This action is for information only — not a sale."}
              </Text>
            </Stack>
          </div>

          <RelatedContentList locale={locale} items={vm.relatedContent} />
          <RelatedToursList locale={locale} items={vm.relatedTours} />
        </Stack>
      </Container>
      <PublicDetailStickyActions locale={locale} />
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
        <Text>
          {fromLabel}{" "}
          <LtrValue>
            {starting.amount} {starting.currency}
          </LtrValue>
        </Text>
      ) : null}
      {summary.occupancyPrices.length > 0 ? (
        <Stack gap="sm">
          <Text role="caption">{occupancyLabel}</Text>
          <ul className="list-inside list-disc">
            {summary.occupancyPrices.map((row) => (
              <li
                key={`${summary.priceId}-${row.passengerCategory}-${row.occupancyCategory}`}
              >
                {categoryLabel(locale, row.passengerCategory)} ·{" "}
                {categoryLabel(locale, row.occupancyCategory)} ·{" "}
                <LtrValue>
                  {row.money.amount} {row.money.currencyCode}
                </LtrValue>
              </li>
            ))}
          </ul>
        </Stack>
      ) : null}
      {summary.components.length > 0 ? (
        <Stack gap="sm">
          <Text role="caption">{componentsLabel}</Text>
          <ul className="list-inside list-disc">
            {summary.components.map((component, index) => (
              <li key={`${summary.priceId}-c-${component.kind}-${index}`}>
                {categoryLabel(locale, component.kind)} ·{" "}
                <LtrValue>
                  {component.money.amount} {component.money.currencyCode}
                </LtrValue>
              </li>
            ))}
          </ul>
        </Stack>
      ) : null}
    </Stack>
  );
}
