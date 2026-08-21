"use client";

import { useEffect, useId, useState, useTransition } from "react";
import {
  LtrValue,
  MoneyText,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import { loadPublicDeparturePriceAction } from "./load-public-departure-price";
import type {
  PublicPriceSummaryView,
  PublishedDepartureView,
} from "./load-tour-detail";

type CommerceCopy = {
  departures: string;
  departuresNote: string;
  noDepartures: string;
  selectDeparture: string;
  schedule: string;
  duration: string;
  days: string;
  transport: string;
  transportNote: string;
  stay: string;
  stayNights: string;
  price: string;
  priceNote: string;
  from: string;
  noPrice: string;
  selectForPrice: string;
  loadingPrice: string;
  occupancy: string;
  components: string;
  bookingBoundary: string;
  bookingBoundaryBody: string;
  startBooking: string;
  needDeparture: string;
  needPrice: string;
  loadingGate: string;
};

function commerceCopy(locale: AppLocale): CommerceCopy {
  if (locale === "fa") {
    return {
      departures: "تاریخ‌های حرکت",
      departuresNote:
        "یک تاریخ حرکت منتشرشده را انتخاب کنید · انتشار به‌معنای رزرو قطعی نیست · قیمت فقط از Pricing",
      noDepartures: "فعلاً تاریخ حرکت منتشرشده‌ای نیست.",
      selectDeparture: "یک تاریخ حرکت را انتخاب کنید",
      schedule: "برنامه",
      duration: "مدت",
      days: "روز",
      transport: "پرواز / حمل‌ونقل پکیج",
      transportNote: "خلاصه پکیج · نه موجودی زنده پرواز",
      stay: "اقامت",
      stayNights: "شب",
      price: "خلاصه قیمت",
      priceNote: "خلاصه قیمت عمومی از Pricing · نه پیش‌فاکتور (Quote) · نه پرداخت",
      from: "از",
      noPrice: "برای این حرکت قیمت عمومی ثبت نشده است.",
      selectForPrice: "پس از انتخاب حرکت، خلاصه قیمت اینجا نمایش داده می‌شود.",
      loadingPrice: "در حال دریافت خلاصه قیمت…",
      occupancy: "نرخ اشغال",
      components: "اجزای قیمت",
      bookingBoundary: "شروع رزرو موقت",
      bookingBoundaryBody:
        "با حرکت و قیمت معتبر می‌توانید رزرو Pending بسازید. Quote داخل Booking صادر می‌شود · پرداخت اینجا نیست · Confirmed ساخته نمی‌شود.",
      startBooking: "شروع رزرو موقت",
      needDeparture: "ابتدا یک تاریخ حرکت منتشرشده را انتخاب کنید.",
      needPrice: "بدون خلاصه قیمت معتبر از Pricing، شروع رزرو ممکن نیست.",
      loadingGate: "پس از دریافت خلاصه قیمت، شروع رزرو فعال می‌شود.",
    };
  }

  if (locale === "ar") {
    return {
      departures: "تواريخ المغادرة",
      departuresNote:
        "اختر تاريخ مغادرة منشور · النشر لا يعني حجزاً مؤكداً · السعر من Pricing فقط",
      noDepartures: "لا تواريخ مغادرة منشورة حالياً.",
      selectDeparture: "اختر تاريخ مغادرة",
      schedule: "الجدول",
      duration: "المدة",
      days: "أيام",
      transport: "الطيران / نقل الباقة",
      transportNote: "ملخص الباقة · ليس مخزون طيران حي",
      stay: "الإقامة",
      stayNights: "ليالٍ",
      price: "ملخص السعر",
      priceNote: "ملخص سعر عام من Pricing · ليس عرض شراء (Quote) · ليس دفعاً",
      from: "من",
      noPrice: "لا سعر عام مسجل لهذه المغادرة.",
      selectForPrice: "بعد اختيار المغادرة يظهر ملخص السعر هنا.",
      loadingPrice: "جاري جلب ملخص السعر…",
      occupancy: "أسعار الإشغال",
      components: "مكونات السعر",
      bookingBoundary: "بدء حجز مؤقت",
      bookingBoundaryBody:
        "مع مغادرة وسعر صالح يمكنك إنشاء حجز Pending. يُصدر Quote داخل Booking · بلا دفع هنا · بلا تأكيد.",
      startBooking: "بدء الحجز المؤقت",
      needDeparture: "اختر أولاً تاريخ مغادرة منشور.",
      needPrice: "بدون ملخص سعر صالح من Pricing لا يمكن بدء الحجز.",
      loadingGate: "بعد جلب ملخص السعر يُفعّل بدء الحجز.",
    };
  }

  return {
    departures: "Departures",
    departuresNote:
      "Select a published departure date · published ≠ confirmed booking · money from Pricing only",
    noDepartures: "No published departures yet.",
    selectDeparture: "Select a departure",
    schedule: "Schedule",
    duration: "Duration",
    days: "days",
    transport: "Package flight / transport",
    transportNote: "Package summary · not live flight inventory",
    stay: "Stay",
    stayNights: "nights",
    price: "Price summary",
    priceNote: "Public Pricing summary · not a Quote · not payment",
    from: "From",
    noPrice: "No public price is registered for this departure.",
    selectForPrice: "After you select a departure, its price summary appears here.",
    loadingPrice: "Loading price summary…",
    occupancy: "Occupancy prices",
    components: "Price components",
    bookingBoundary: "Start pending booking",
    bookingBoundaryBody:
      "With a valid departure and price you can create a Pending booking. Quote is issued inside Booking · no payment here · not Confirmed.",
    startBooking: "Start pending booking",
    needDeparture: "Select a published departure first.",
    needPrice: "Without a valid Pricing summary, booking cannot start.",
    loadingGate: "Booking start enables after the price summary loads.",
  };
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

/**
 * I2 composition + I3 booking-start gate (TC-P33-T006 / TC-P33-T007).
 * TourProduct → Published TourDeparture → Pricing summary → book prepare page.
 * Initiate POST happens on /tours/{slug}/book via Booking ownership — not here.
 * No Payment · no Confirmed · no hardcoded departure IDs · no invented money.
 */
export function TourCommercePanel({
  locale,
  slug,
  departures,
}: {
  locale: AppLocale;
  slug: string;
  departures: PublishedDepartureView[];
}) {
  const copy = commerceCopy(locale);
  const groupName = useId();
  const [selectedId, setSelectedId] = useState<string | null>(
    departures[0]?.id ?? null,
  );
  const [priceByDeparture, setPriceByDeparture] = useState<
    Record<string, PublicPriceSummaryView | null | undefined>
  >(() => {
    const initial: Record<string, PublicPriceSummaryView | null | undefined> =
      {};
    for (const d of departures) {
      // Seed from SSR composition when present; undefined = not fetched yet.
      if (d.priceSummary) {
        initial[d.id] = d.priceSummary;
      }
    }
    return initial;
  });
  const [pending, startTransition] = useTransition();

  useEffect(() => {
    if (!selectedId) {
      return;
    }
    if (priceByDeparture[selectedId] !== undefined) {
      return;
    }

    startTransition(() => {
      void (async () => {
        const summary = await loadPublicDeparturePriceAction(selectedId);
        setPriceByDeparture((prev) => ({ ...prev, [selectedId]: summary }));
      })();
    });
  }, [selectedId, priceByDeparture]);

  function onSelect(departureId: string) {
    setSelectedId(departureId);
    if (priceByDeparture[departureId] !== undefined) {
      return;
    }
    startTransition(() => {
      void (async () => {
        const summary = await loadPublicDeparturePriceAction(departureId);
        setPriceByDeparture((prev) => ({ ...prev, [departureId]: summary }));
      })();
    });
  }

  const selected = departures.find((d) => d.id === selectedId) ?? null;
  const selectedPrice =
    selectedId != null ? priceByDeparture[selectedId] : undefined;
  const canStartBooking = selectedId != null && selectedPrice != null;
  const bookHref = canStartBooking
    ? `/${locale}/tours/${encodeURIComponent(slug)}/book?departureId=${encodeURIComponent(selectedId)}`
    : null;
  const gateHint = !selectedId
    ? copy.needDeparture
    : selectedPrice === undefined
      ? copy.loadingGate
      : selectedPrice == null
        ? copy.needPrice
        : null;

  return (
    <div id="published-departures" className="scroll-mt-24">
      <Stack gap="md">
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.departures}
          </Text>
          <Text role="caption">{copy.departuresNote}</Text>
        </Stack>

        {departures.length === 0 ? (
          <Surface>
            <Text role="muted">{copy.noDepartures}</Text>
          </Surface>
        ) : (
          <fieldset className="m-0 border-0 p-0">
            <legend className="sr-only">{copy.selectDeparture}</legend>
            <ul className="flex flex-col gap-3">
              {departures.map((d) => {
                const checked = d.id === selectedId;
                return (
                  <li key={d.id}>
                    <label
                      className={`block cursor-pointer rounded-2xl border p-4 transition-colors ${
                        checked
                          ? "border-[#1D4ED8] bg-[#1D4ED8]/5 shadow-sm"
                          : "border-border bg-surface hover:border-[#1D4ED8]/40"
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <input
                          type="radio"
                          className="mt-1 size-4 shrink-0 accent-primary"
                          name={groupName}
                          value={d.id}
                          checked={checked}
                          onChange={() => onSelect(d.id)}
                        />
                        <Stack gap="sm" className="min-w-0 flex-1">
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
                        </Stack>
                      </div>
                    </label>
                  </li>
                );
              })}
            </ul>
          </fieldset>
        )}

        <div id="price-from" className="scroll-mt-24">
          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.price}
              </Text>
              <Text role="caption">{copy.priceNote}</Text>
              {!selected ? (
                <Text role="muted">{copy.selectForPrice}</Text>
              ) : pending && selectedPrice === undefined ? (
                <Text role="muted">{copy.loadingPrice}</Text>
              ) : selectedPrice == null ? (
                <Text role="muted">{copy.noPrice}</Text>
              ) : (
                <DeparturePriceFacts
                  locale={locale}
                  summary={selectedPrice}
                  fromLabel={copy.from}
                  occupancyLabel={copy.occupancy}
                  componentsLabel={copy.components}
                />
              )}
            </Stack>
          </Surface>
        </div>

        <Surface className="border-primary/15 bg-gradient-to-br from-surface to-primary/5">
          <Stack gap="sm">
            <Text as="h2" role="heading" className="text-primary">
              {copy.bookingBoundary}
            </Text>
            <Text role="muted">{copy.bookingBoundaryBody}</Text>
            {bookHref ? (
              <a
                href={bookHref}
                className="min-h-touch inline-flex w-full items-center justify-center rounded-md bg-accent px-4 text-sm font-semibold text-accent-foreground hover:opacity-95 sm:w-auto"
              >
                {copy.startBooking}
              </a>
            ) : (
              <button
                type="button"
                disabled
                aria-disabled="true"
                title={gateHint ?? copy.needPrice}
                className="min-h-touch inline-flex w-full cursor-not-allowed items-center justify-center rounded-md bg-surface-muted px-4 text-sm font-semibold text-muted-foreground sm:w-auto"
              >
                {copy.startBooking}
              </button>
            )}
            {gateHint ? <Text role="caption">{gateHint}</Text> : null}
          </Stack>
        </Surface>
      </Stack>
    </div>
  );
}

function DeparturePriceFacts({
  locale,
  summary,
  fromLabel,
  occupancyLabel,
  componentsLabel,
}: {
  locale: AppLocale;
  summary: PublicPriceSummaryView;
  fromLabel: string;
  occupancyLabel: string;
  componentsLabel: string;
}) {
  const starting = startingMoney(summary);

  return (
    <Stack gap="sm">
      {starting ? (
        <div className="flex flex-wrap items-baseline gap-2">
          <Text>{fromLabel}</Text>
          <MoneyText
            locale={locale}
            money={{
              amount: String(starting.amount),
              currencyCode: starting.currency,
            }}
            className="text-lg font-semibold"
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
