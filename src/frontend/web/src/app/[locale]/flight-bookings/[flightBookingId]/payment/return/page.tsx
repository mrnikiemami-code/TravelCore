import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PublicFlightBookingPaymentView } from "@/features/flight-booking/payment-view";
import { getPublicFlightBookingCopy } from "@/features/flight-booking/copy";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string; flightBookingId: string }>;
};

/**
 * Provider browser-return navigation only (TC-P22-T008 / P22-R8).
 * BrowserReturn != PaymentSuccess. Query parameters never mutate Payment.
 * Transaction page: always noindex. Token is never placed in the URL.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicFlightBookingPaymentReturnPage({ params }: PageProps) {
  const { locale: localeParam, flightBookingId } = await params;
  if (!isAppLocale(localeParam) || !flightBookingId) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getPublicFlightBookingCopy(locale);

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
          <PublicFlightBookingPaymentView
            locale={locale}
            flightBookingId={flightBookingId}
            returnedFromProvider
          />
        </div>
      </div>
    </PublicShell>
  );
}
