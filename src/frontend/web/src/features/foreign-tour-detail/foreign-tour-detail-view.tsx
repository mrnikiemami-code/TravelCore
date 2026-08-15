import {
  Container,
  LtrValue,
  MediaImage,
  MixedCurrencyPrice,
  MoneyText,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import type { ForeignTourDetailPageViewModel } from "@/types/pages/foreign-tour-detail";

function statusLabel(vm: ForeignTourDetailPageViewModel): string {
  switch (vm.commercialStatus) {
    case "active":
      return vm.locale === "fa" ? "فعال" : "Active";
    case "no_departure":
      return vm.locale === "fa" ? "بدون تاریخ حرکت فعال" : "No active departure";
    case "expired":
      return vm.locale === "fa" ? "منقضی" : "Expired";
    case "unavailable":
      return vm.locale === "fa" ? "غیرفعال موقت" : "Temporarily unavailable";
  }
}

/**
 * Server-only composition of ForeignTourDetailPage walking skeleton (T013).
 * Not final production Tour UI · no booking client island (T014).
 */
export function ForeignTourDetailView({
  vm,
}: {
  vm: ForeignTourDetailPageViewModel;
}) {
  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          {/* B · Hero / product header */}
          <Surface>
            <Stack gap="md">
              <MediaImage
                src={vm.hero.src}
                alt={vm.hero.alt}
                aspectRatio={vm.hero.aspectRatio}
                sizes={vm.hero.sizes}
                priority={vm.hero.priority}
              />
              <Stack gap="sm">
                <Text as="h1" role="heading">
                  {vm.product.title}
                </Text>
                <Text role="muted">
                  {vm.destination.name}
                  {vm.destination.countryName
                    ? ` · ${vm.destination.countryName}`
                    : ""}
                  {" · "}
                  {vm.duration.label}
                </Text>
                <Text role="label">
                  {vm.locale === "fa" ? "وضعیت تجاری:" : "Commercial status:"}{" "}
                  {statusLabel(vm)}
                </Text>
                {vm.product.summary ? (
                  <Text role="body">{vm.product.summary}</Text>
                ) : null}
                {vm.agency ? (
                  <Text role="caption">
                    {vm.locale === "fa" ? "فروشنده:" : "Seller:"} {vm.agency.name}
                  </Text>
                ) : null}
              </Stack>
            </Stack>
          </Surface>

          {/* C · Departures */}
          <Surface tone="muted">
            <Stack gap="md">
              <Text as="h2" role="title">
                {vm.locale === "fa" ? "تاریخ‌های حرکت" : "Departures"}
              </Text>
              <ul className="flex list-none flex-col gap-2 p-0">
                {vm.departures.map((d) => (
                  <li
                    key={d.departureKey}
                    className="rounded-md border border-border bg-surface px-3 py-2"
                  >
                    <Text role="body">
                      {d.departureDateLabel}
                      {d.availabilityLabel ? ` · ${d.availabilityLabel}` : ""}
                      {d.selected
                        ? vm.locale === "fa"
                          ? " · انتخاب‌شده"
                          : " · selected"
                        : ""}
                    </Text>
                  </li>
                ))}
              </ul>
            </Stack>
          </Surface>

          {/* D · Flights */}
          {vm.flights.length > 0 ? (
            <Surface>
              <Stack gap="md">
                <Text as="h2" role="title">
                  {vm.locale === "fa" ? "پرواز / حمل‌ونقل" : "Flight / transport"}
                </Text>
                <ul className="flex list-none flex-col gap-3 p-0">
                  {vm.flights.map((f, idx) => (
                    <li
                      key={`${f.flightNumber ?? "seg"}-${idx}`}
                      className="rounded-md border border-border px-3 py-3"
                    >
                      <Text role="body">
                        <LtrValue>
                          {f.originAirportCode} → {f.destinationAirportCode}
                        </LtrValue>
                        {f.flightNumber ? (
                          <>
                            {" · "}
                            <LtrValue>{f.flightNumber}</LtrValue>
                          </>
                        ) : null}
                      </Text>
                      {f.departureLocalLabel || f.arrivalLocalLabel ? (
                        <Text role="caption">
                          {f.departureLocalLabel}
                          {f.arrivalLocalLabel
                            ? ` → ${f.arrivalLocalLabel}`
                            : ""}
                        </Text>
                      ) : null}
                      {f.baggageLabel ? (
                        <Text role="caption">{f.baggageLabel}</Text>
                      ) : null}
                    </li>
                  ))}
                </ul>
              </Stack>
            </Surface>
          ) : null}

          {/* E · Hotels */}
          <Surface>
            <Stack gap="md">
              <Text as="h2" role="title">
                {vm.locale === "fa" ? "گزینه‌های هتل" : "Hotel options"}
              </Text>
              <ul className="flex list-none flex-col gap-3 p-0">
                {vm.hotelOptions.map((h) => (
                  <li
                    key={h.optionKey}
                    className="rounded-lg border border-border px-3 py-3"
                  >
                    <Stack gap="sm">
                      <Text role="label">
                        {h.hotelName}
                        {h.mealPlanLabel ? ` · ${h.mealPlanLabel}` : ""}
                      </Text>
                      {h.summary ? <Text role="caption">{h.summary}</Text> : null}
                      {h.relativePrice ? (
                        <MixedCurrencyPrice
                          price={h.relativePrice}
                          locale={vm.locale}
                          irrDisplayUnit={
                            vm.pricingOffers[0]?.irrDisplayUnit ??
                            (vm.locale === "fa" ? "Toman" : "IRR")
                          }
                        />
                      ) : null}
                    </Stack>
                  </li>
                ))}
              </ul>
            </Stack>
          </Surface>

          {/* F · Pricing */}
          <Surface tone="muted">
            <Stack gap="md">
              <Text as="h2" role="title">
                {vm.locale === "fa"
                  ? "قیمت / دسته مسافر و اشغال"
                  : "Pricing / passenger & occupancy"}
              </Text>
              <Text role="caption">
                {vm.locale === "fa"
                  ? "نمایش قیمت ≠ Quote؛ ارزها جمع نمی‌شوند."
                  : "Displayed price ≠ Quote; mixed currencies are not summed."}
              </Text>
              <ul className="flex list-none flex-col gap-3 p-0">
                {vm.pricingOffers.map((o) => (
                  <li
                    key={o.offerKey}
                    className="rounded-md border border-border bg-surface px-3 py-3"
                  >
                    <Text role="label">
                      {o.passengerCategory} · {o.occupancy}
                    </Text>
                    {o.unavailable ? (
                      <Text role="caption">
                        {o.unavailableReason ??
                          (vm.locale === "fa" ? "ناموجود" : "Unavailable")}
                      </Text>
                    ) : (
                      <div className="mt-2">
                        <MixedCurrencyPrice
                          price={o.price}
                          locale={vm.locale}
                          irrDisplayUnit={
                            o.irrDisplayUnit ??
                            (vm.locale === "fa" ? "Toman" : "IRR")
                          }
                        />
                      </div>
                    )}
                  </li>
                ))}
              </ul>
              {vm.pricingOffers[0] && !vm.pricingOffers[0].unavailable ? (
                <div>
                  <Text role="caption">
                    {vm.locale === "fa" ? "نمونه تک‌ارزی:" : "Single-currency sample:"}
                  </Text>
                  <div className="mt-1">
                    <MoneyText
                      money={vm.pricingOffers[0].price.components[0]!}
                      locale={vm.locale}
                      irrDisplayUnit={vm.pricingOffers[0].irrDisplayUnit}
                    />
                  </div>
                </div>
              ) : null}
            </Stack>
          </Surface>

          {/* G · Services */}
          <Surface>
            <Stack gap="md">
              <Text as="h2" role="title">
                {vm.locale === "fa" ? "خدمات" : "Services"}
              </Text>
              <div className="grid gap-4 sm:grid-cols-2">
                <Stack gap="sm">
                  <Text role="label">
                    {vm.locale === "fa" ? "شامل" : "Included"}
                  </Text>
                  <ul className="list-disc ps-5">
                    {vm.services.included.map((item) => (
                      <li key={item}>
                        <Text as="span" role="body">
                          {item}
                        </Text>
                      </li>
                    ))}
                  </ul>
                </Stack>
                <Stack gap="sm">
                  <Text role="label">
                    {vm.locale === "fa" ? "غیرشامل" : "Excluded"}
                  </Text>
                  <ul className="list-disc ps-5">
                    {vm.services.excluded.map((item) => (
                      <li key={item}>
                        <Text as="span" role="body">
                          {item}
                        </Text>
                      </li>
                    ))}
                  </ul>
                </Stack>
              </div>
            </Stack>
          </Surface>

          {/* H · Requirements */}
          {vm.requirements.length > 0 ? (
            <Surface tone="muted">
              <Stack gap="md">
                <Text as="h2" role="title">
                  {vm.locale === "fa" ? "الزامات سفر" : "Travel requirements"}
                </Text>
                <ul className="list-disc ps-5">
                  {vm.requirements.map((item) => (
                    <li key={item}>
                      <Text as="span" role="body">
                        {item}
                      </Text>
                    </li>
                  ))}
                </ul>
              </Stack>
            </Surface>
          ) : null}

          {/* J · Itinerary summary */}
          {vm.itinerarySummary.length > 0 ? (
            <Surface>
              <Stack gap="md">
                <Text as="h2" role="title">
                  {vm.locale === "fa" ? "خلاصه برنامه" : "Itinerary summary"}
                </Text>
                <ol className="flex list-none flex-col gap-3 p-0">
                  {vm.itinerarySummary.map((day) => (
                    <li key={day.day} className="rounded-md border border-border px-3 py-2">
                      <Text role="label">
                        {vm.locale === "fa" ? "روز" : "Day"} {day.day}: {day.title}
                      </Text>
                      <Text role="caption">{day.summary}</Text>
                    </li>
                  ))}
                </ol>
              </Stack>
            </Surface>
          ) : null}

          {/* M · CTA slot (presentation only — interactive sticky = T014) */}
          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="title">
                {vm.locale === "fa" ? "اقدام" : "Action"}
              </Text>
              <p>
                <span
                  className={
                    vm.cta.enabled
                      ? "inline-flex min-h-touch items-center rounded-md bg-primary px-4 text-label text-primary-foreground"
                      : "inline-flex min-h-touch items-center rounded-md bg-surface-muted px-4 text-label text-muted-foreground"
                  }
                  aria-disabled={!vm.cta.enabled}
                >
                  {vm.cta.label}
                </span>
              </p>
              {!vm.cta.enabled && vm.cta.reasonDisabled ? (
                <Text role="caption">{vm.cta.reasonDisabled}</Text>
              ) : (
                <Text role="caption">
                  {vm.locale === "fa"
                    ? "اسلات CTA نمایشی — جزیرهٔ تعاملی sticky در T014."
                    : "Presentation CTA slot — interactive sticky island in T014."}
                </Text>
              )}
            </Stack>
          </Surface>
        </Stack>
      </Container>
    </div>
  );
}
