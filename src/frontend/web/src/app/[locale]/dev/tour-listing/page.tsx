import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { parseListingFilterCriteria } from "@/features/public-experience/filter-presentation";
import { PublicTourListingView } from "@/features/public-experience/listing-view";
import { loadTourListingFixtureSelection } from "@/lib/fixtures/tour-listing";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Tour listing validation",
  robots: { index: false, follow: false },
};

/**
 * UIVAL-T004 dev-only Tour Listing/Search presentation validation.
 * Uses fixture selection when destination filter is set — not live Search API.
 */
export default async function TourListingValidationPage({
  params,
  searchParams,
}: {
  params: Promise<{ locale: string }>;
  searchParams: Promise<{ destination?: string; sort?: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const query = await searchParams;
  const criteria = parseListingFilterCriteria(query);
  const selection = criteria.destinationSlug
    ? loadTourListingFixtureSelection(locale)
    : [];

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T004 · Tour Listing
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation · presentation filters only · not Search engine
        </Text>
      }
    >
      <PublicTourListingView
        locale={locale}
        criteria={criteria}
        selection={selection}
      />
    </PublicShell>
  );
}
