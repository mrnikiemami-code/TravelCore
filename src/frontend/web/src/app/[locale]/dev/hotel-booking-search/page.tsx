import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { PublicHotelBookingPrepareForm } from "@/features/hotel-booking/prepare-form";
import { getPublicHotelBookingCopy } from "@/features/hotel-booking/copy";
import { loadHotelDetailFixture } from "@/lib/fixtures/hotel-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Hotel booking search validation",
  robots: { index: false, follow: false },
};

/** UIVAL-T013 Hotel Booking Search validation. */
export default async function HotelBookingSearchValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const loaded = loadHotelDetailFixture(locale);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const hotel = loaded.data;
  const copy = getPublicHotelBookingCopy(locale);

  return (
    <PublicShell
      header={<Text as="p" role="label">UIVAL-T013 · Hotel Booking Search</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto flex w-full max-w-xl flex-col gap-4 px-4">
          <Text as="h1" role="heading">{copy.prepareTitle}</Text>
          <Text>{copy.prepareNote}</Text>
          <Text role="caption">
            place <LtrValue>{hotel.placeId}</LtrValue> ·{" "}
            <LtrValue>{hotel.slug}</LtrValue>
          </Text>
          <PublicHotelBookingPrepareForm
            locale={locale}
            slug={hotel.slug}
            placeId={hotel.placeId}
          />
        </div>
      </div>
    </PublicShell>
  );
}
