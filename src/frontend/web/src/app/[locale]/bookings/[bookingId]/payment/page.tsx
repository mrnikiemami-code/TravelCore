import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PublicBookingPaymentView } from "@/features/booking/payment-view";
import { getPublicBookingCopy } from "@/features/booking/copy";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string; bookingId: string }>;
};

/**
 * Authorized private Booking payment (TC-P20-T007 / P20-R7).
 * Transaction page: always noindex. Token is never placed in the URL.
 * Browser return is a sibling route and also does not mark Payment successful.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicBookingPaymentPage({ params }: PageProps) {
  const { locale: localeParam, bookingId } = await params;
  if (!isAppLocale(localeParam) || !bookingId) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getPublicBookingCopy(locale);

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={<Text role="caption">{copy.payTitle}</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto w-full max-w-xl px-4">
          <PublicBookingPaymentView locale={locale} bookingId={bookingId} />
        </div>
      </div>
    </PublicShell>
  );
}
