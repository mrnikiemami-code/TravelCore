import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { HotelDiscoveryView } from "@/features/hotel-discovery/hotel-discovery-view";
import { parseHotelListingCriteria } from "@/features/hotel-discovery/hotel-listing-criteria";
import { loadHotelDiscoveryList } from "@/features/hotel-discovery/load-hotel-discovery-list";
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
 * Public hotel catalog discovery index (TC-HOTIDX-T005 / TC-P30-T006).
 * P07 Place browse — not Search engine · not HotelBooking availability.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const title =
    localeParam === "fa" ? "هتل‌ها" : localeParam === "ar" ? "الفنادق" : "Hotels";
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path: "hotels",
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

export default async function HotelDiscoveryPage({
  params,
  searchParams,
}: PageProps) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const sp = await searchParams;
  const criteria = parseHotelListingCriteria(sp);
  const loaded = await loadHotelDiscoveryList(locale);

  return (
    <PublicShell
      header={<PublicHeader locale={locale} />}
      footer={<PublicFooter locale={locale} />}
    >
      <HotelDiscoveryView
        locale={locale}
        hotels={loaded.hotels}
        criteria={criteria}
        loadError={!loaded.ok}
      />
    </PublicShell>
  );
}
