import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { PublicBookingPrepareForm } from "@/features/booking/prepare-form";
import { getPublicBookingCopy } from "@/features/booking/copy";
import { loadBookingCheckoutFixture } from "@/lib/fixtures/booking-checkout";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Booking checkout validation",
  robots: { index: false, follow: false },
};

/** UIVAL-T011 Booking/Checkout validation — prepare → status → payment flow shell. */
export default async function BookingCheckoutValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const copy = getPublicBookingCopy(locale);
  const fixture = loadBookingCheckoutFixture(locale);

  return (
    <PublicShell
      header={<Text as="p" role="label">UIVAL-T011 · Booking/Checkout</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto flex w-full max-w-xl flex-col gap-4 px-4">
          <Text as="h1" role="heading">{copy.prepareTitle}</Text>
          <Text>{copy.prepareNote}</Text>
          <Text role="caption">
            flow: prepare → /bookings/[id] → /payment · slug{" "}
            <LtrValue>{fixture.slug}</LtrValue>
          </Text>
          <PublicBookingPrepareForm
            locale={locale}
            slug={fixture.slug}
            departures={fixture.departures}
          />
        </div>
      </div>
    </PublicShell>
  );
}
