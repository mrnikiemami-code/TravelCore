import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PublicHotelBookingStatusView } from "@/features/hotel-booking/status-view";
import { getPublicHotelBookingCopy } from "@/features/hotel-booking/copy";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string; hotelBookingId: string }>;
};

/**
 * Authorized private HotelBooking read (TC-P21-T008 / P21-R8).
 * Transaction page: always noindex. Token is never placed in the URL.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicHotelBookingStatusPage({ params }: PageProps) {
  const { locale: localeParam, hotelBookingId } = await params;
  if (!isAppLocale(localeParam) || !hotelBookingId) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getPublicHotelBookingCopy(locale);

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={<Text role="caption">{copy.statusTitle}</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto w-full max-w-xl px-4">
          <PublicHotelBookingStatusView locale={locale} hotelBookingId={hotelBookingId} />
        </div>
      </div>
    </PublicShell>
  );
}
