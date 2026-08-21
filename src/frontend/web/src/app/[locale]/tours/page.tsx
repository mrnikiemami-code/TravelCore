import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { enrichToursWithCoverMedia } from "@/features/tour-discovery/enrich-tour-covers";
import { TourDiscoveryView } from "@/features/tour-discovery/tour-discovery-view";
import { parseTourListingCriteria } from "@/features/tour-discovery/tour-listing-criteria";
import { loadTourDiscoveryList } from "@/features/tour-discovery/load-tour-discovery-list";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadComposedSeoMetadata } from "@/lib/seo/load-composed-metadata";
import {
  languagesFromComposed,
  robotsFromComposed,
} from "@/lib/seo/metadata-contract";

type PageProps = {
  params: Promise<{ locale: string }>;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

/**
 * Public tour commerce listing (TC-P30-T007 · TC-P31-T005 polish).
 * Destination-scoped related-published discovery — not Search, not a global browse engine.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const title =
    localeParam === "fa" ? "تورها" : localeParam === "ar" ? "الجولات" : "Tours";
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path: "tours",
    localizedTitle: title,
  });

  if (!composed) {
    return { title, robots: { index: false, follow: true } };
  }

  const languages = languagesFromComposed(composed);
  const robots = robotsFromComposed(composed);
  return {
    title: composed.title,
    description: composed.description ?? undefined,
    ...(composed.canonicalHref || Object.keys(languages).length > 0
      ? {
          alternates: {
            ...(composed.canonicalHref
              ? { canonical: composed.canonicalHref }
              : {}),
            ...(Object.keys(languages).length > 0 ? { languages } : {}),
          },
        }
      : {}),
    robots,
  };
}

export default async function PublicTourListingPage({
  params,
  searchParams,
}: PageProps) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const sp = await searchParams;
  const criteria = parseTourListingCriteria(sp);
  const loaded = await loadTourDiscoveryList(locale, criteria.destination);
  const tours =
    loaded.ok && loaded.mode === "ready"
      ? await enrichToursWithCoverMedia(locale, loaded.tours)
      : loaded.tours;

  return (
    <PublicShell
      header={<PublicHeader locale={locale} />}
      footer={<PublicFooter locale={locale} />}
    >
      <TourDiscoveryView
        locale={locale}
        tours={tours}
        criteria={criteria}
        loadError={!loaded.ok}
        needsDestination={loaded.ok && loaded.mode === "needs-destination"}
      />
    </PublicShell>
  );
}
