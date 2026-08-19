import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PublicFlightSearchForm } from "@/features/flight-booking/search-form";
import { getPublicFlightBookingCopy } from "@/features/flight-booking/copy";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string }>;
};

/**
 * Public Flight search / selection (TC-P22-T008 / P22-R8).
 * Transactional search must not become an uncontrolled indexable page.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicFlightSearchPage({ params }: PageProps) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
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
      context={<Text role="caption">{copy.searchTitle}</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto flex w-full max-w-xl flex-col gap-4 px-4">
          <Text as="h1" role="heading">
            {copy.searchTitle}
          </Text>
          <Text>{copy.searchNote}</Text>
          <PublicFlightSearchForm locale={locale} />
        </div>
      </div>
    </PublicShell>
  );
}
