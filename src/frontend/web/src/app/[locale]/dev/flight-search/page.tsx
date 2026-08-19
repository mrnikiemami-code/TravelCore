import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PublicFlightSearchForm } from "@/features/flight-booking/search-form";
import { getPublicFlightBookingCopy } from "@/features/flight-booking/copy";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Flight search validation",
  robots: { index: false, follow: false },
};

/** UIVAL-T012 Flight Search validation. */
export default async function FlightSearchValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const copy = getPublicFlightBookingCopy(locale);

  return (
    <PublicShell
      header={<Text as="p" role="label">UIVAL-T012 · Flight Search</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto flex w-full max-w-xl flex-col gap-4 px-4">
          <Text as="h1" role="heading">{copy.searchTitle}</Text>
          <Text>{copy.searchNote}</Text>
          <PublicFlightSearchForm locale={locale} />
        </div>
      </div>
    </PublicShell>
  );
}
