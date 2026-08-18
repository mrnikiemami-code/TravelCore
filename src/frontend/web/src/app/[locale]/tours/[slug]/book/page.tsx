import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
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
 * Public Tour Booking initiation (TC-P19-T008 / P19-R8).
 * Transaction page: always noindex. Pending initiation only — not confirmation or payment.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicTourBookPage({ params, searchParams }: PageProps) {
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
  const departures = loaded.data.publishedDepartures.map((departure) => ({
    id: departure.id,
    label: [departure.startDate, departure.endDate].filter(Boolean).join(" – ") || departure.id,
  }));

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={<Text role="caption">{copy.prepareTitle}</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto flex w-full max-w-xl flex-col gap-4 px-4">
          <Text as="h1" role="heading">
            {copy.prepareTitle}
          </Text>
          <Text>{copy.prepareNote}</Text>
          <PublicBookingPrepareForm
            locale={locale}
            slug={slug}
            departures={departures}
            initialDepartureId={initialDepartureId}
          />
        </div>
      </div>
    </PublicShell>
  );
}
