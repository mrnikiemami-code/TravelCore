import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { Container, MoneyText, Text } from "@/components/ui";
import { PublicBookingPrepareForm } from "@/features/booking/prepare-form";
import { getPublicBookingCopy } from "@/features/booking/copy";
import { loadTourDetailPage } from "@/features/tour-detail/load-tour-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string; slug: string }>;
  searchParams: Promise<{ departureId?: string | string[] }>;
};

/**
 * Public Tour Booking initiation (TC-P36-T005 commerce polish).
 * Transaction page: always noindex. Pending initiation only — not confirmation or payment.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicTourBookPage({
  params,
  searchParams,
}: PageProps) {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const loaded = await loadTourDetailPage(locale, slug);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const query = await searchParams;
  const departureIdRaw = query.departureId;
  const initialDepartureId = Array.isArray(departureIdRaw)
    ? departureIdRaw[0]
    : departureIdRaw;
  const copy = getPublicBookingCopy(locale);
  const tour = loaded.data;
  const departures = tour.publishedDepartures.map((departure) => ({
    id: departure.id,
    label:
      [departure.startDate, departure.endDate].filter(Boolean).join(" – ") ||
      (locale === "fa"
        ? "تاریخ حرکت منتشرشده"
        : locale === "ar"
          ? "تاريخ مغادرة منشور"
          : "Published departure"),
  }));
  const selected =
    tour.publishedDepartures.find((d) => d.id === initialDepartureId) ??
    tour.publishedDepartures[0] ??
    null;
  const priceSummary = selected?.priceSummary ?? null;
  const starting =
    priceSummary?.occupancyPrices?.[0]?.money ??
    priceSummary?.components?.find((c) => c.kind === "Base")?.money ??
    null;
  const hero = tour.cover?.src ?? tour.gallery[0]?.src ?? null;

  const summaryCopy =
    locale === "fa"
      ? {
          trip: "خلاصه سفر",
          departure: "حرکت انتخاب‌شده",
          from: "از",
          next: "مرحله بعد: تکمیل اطلاعات و ثبت رزرو موقت",
        }
      : locale === "ar"
        ? {
            trip: "ملخص الرحلة",
            departure: "المغادرة المختارة",
            from: "من",
            next: "الخطوة التالية: إكمال البيانات وإنشاء حجز مؤقت",
          }
        : {
            trip: "Trip summary",
            departure: "Selected departure",
            from: "From",
            next: "Next: complete traveler details and create a Pending booking",
          };

  return (
    <PublicShell
      header={<PublicHeader locale={locale} />}
      footer={<PublicFooter locale={locale} />}
    >
      <section className="relative isolate overflow-hidden border-b border-border">
        <div className="absolute inset-0 bg-[#0E172A]" aria-hidden />
        {hero ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={hero}
            alt=""
            className="absolute inset-0 h-full w-full object-cover opacity-45"
          />
        ) : null}
        <div
          aria-hidden
          className="absolute inset-0 bg-gradient-to-r from-[#0E172A]/95 via-[#0E172A]/80 to-[#0E172A]/50"
        />
        <Container width="narrow" className="relative py-8 sm:py-10">
          <Link
            href={`/${locale}/tours/${encodeURIComponent(slug)}`}
            className="text-xs font-medium text-white/75 underline-offset-2 hover:text-white hover:underline"
          >
            {copy.backToTour}
          </Link>
          <p className="mt-3 text-xs font-semibold uppercase tracking-[0.16em] text-[#FBBF24]">
            {summaryCopy.trip}
          </p>
          <h1 className="mt-2 text-2xl font-semibold tracking-tight text-white sm:text-3xl">
            {copy.prepareTitle}
          </h1>
          <p className="mt-2 max-w-xl text-sm text-white/90">{copy.prepareNote}</p>
        </Container>
      </section>

      <Container width="narrow" className="py-6 sm:py-8">
        <div className="grid gap-5 lg:grid-cols-[0.95fr_1.05fr]">
          <aside className="rounded-2xl border border-border bg-surface p-5 shadow-sm">
            <p className="text-xs font-semibold uppercase tracking-wide text-[#1D4ED8]">
              {summaryCopy.trip}
            </p>
            <p className="mt-2 text-lg font-semibold text-foreground">
              {tour.name}
            </p>
            {tour.kind ? (
              <p className="mt-1 text-sm text-muted-foreground">{tour.kind}</p>
            ) : null}
            {selected ? (
              <div className="mt-4 rounded-xl border border-border bg-background p-3">
                <p className="text-xs font-medium text-muted-foreground">
                  {summaryCopy.departure}
                </p>
                <p className="mt-1 text-sm font-medium text-foreground">
                  {[selected.startDate, selected.endDate]
                    .filter(Boolean)
                    .join(" – ") || "—"}
                </p>
                {selected.durationDays != null ? (
                  <p className="mt-1 text-xs text-muted-foreground">
                    {selected.durationDays}{" "}
                    {locale === "fa" ? "روز" : locale === "ar" ? "أيام" : "days"}
                  </p>
                ) : null}
              </div>
            ) : null}
            {starting ? (
              <div className="mt-4">
                <p className="text-xs text-muted-foreground">{summaryCopy.from}</p>
                <MoneyText
                  locale={locale}
                  money={{
                    amount: String(starting.amount),
                    currencyCode: starting.currencyCode,
                  }}
                  className="mt-1 text-xl font-semibold text-foreground"
                />
                <Text role="caption" className="mt-1">
                  {locale === "fa"
                    ? "خلاصه قیمت عمومی · نه پیش‌فاکتور · نه پرداخت"
                    : locale === "ar"
                      ? "ملخص سعر عام · ليس عرض شراء · ليس دفعاً"
                      : "Public price summary · not a Quote · not payment"}
                </Text>
              </div>
            ) : null}
            <p className="mt-5 text-sm text-muted-foreground">{summaryCopy.next}</p>
          </aside>

          <div className="rounded-2xl border border-border bg-surface p-5 shadow-sm sm:p-6">
            <PublicBookingPrepareForm
              locale={locale}
              slug={slug}
              departures={departures}
              initialDepartureId={initialDepartureId}
            />
          </div>
        </div>
      </Container>
    </PublicShell>
  );
}
