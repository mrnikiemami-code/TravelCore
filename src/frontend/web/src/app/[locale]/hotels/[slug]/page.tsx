import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { HotelDetailView } from "@/features/hotel-detail/hotel-detail-view";
import { loadHotelDiscoveryList } from "@/features/hotel-discovery/load-hotel-discovery-list";
import { loadPlaceDetailPage } from "@/features/place-detail/load-place-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadSeoBreadcrumbJsonLd } from "@/lib/seo/load-breadcrumb-jsonld";
import { loadComposedSeoMetadata } from "@/lib/seo/load-composed-metadata";
import {
  languagesFromComposed,
  robotsFromComposed,
} from "@/lib/seo/metadata-contract";
import { serializeBreadcrumbJsonLd } from "@/lib/seo/structured-data-contract";

type PageProps = {
  params: Promise<{ locale: string; slug: string }>;
};

/**
 * Public Hotel catalog detail (TC-PRODSURF-T004 / TC-P30-T006).
 * Place catalog SoR — not HotelBooking availability engine.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = await loadPlaceDetailPage(localeParam, slug);
  if (!isApiOk(loaded) || loaded.data.kind !== "Hotel") {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const vm = loaded.data;
  const path = `hotels/${slug}`;
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path,
    localizedTitle: vm.name,
    localizedDescription: vm.description,
  });

  if (!composed) {
    return {
      title: vm.name,
      description: vm.description ?? undefined,
      robots: { index: false, follow: true },
    };
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

export default async function HotelDetailPage({ params }: PageProps) {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = await loadPlaceDetailPage(locale, slug);
  if (!isApiOk(loaded) || loaded.data.kind !== "Hotel") {
    notFound();
  }

  const vm = loaded.data;
  const discovery = await loadHotelDiscoveryList(locale);
  const similarHotels = discovery.hotels
    .filter((h) => h.placeId !== vm.placeId && h.slug !== vm.slug)
    .slice(0, 3);

  const crumbs = [
    ...(vm.destination
      ? [
          {
            name: vm.destination.name,
            publicPath: vm.destination.slug
              ? `destinations/${vm.destination.slug}`
              : null,
          },
        ]
      : []),
    {
      name: vm.name,
      publicPath: `hotels/${vm.slug}`,
    },
  ];
  const breadcrumbJsonLd = await loadSeoBreadcrumbJsonLd(locale, crumbs);
  const breadcrumbScript = serializeBreadcrumbJsonLd(breadcrumbJsonLd);

  return (
    <PublicShell
      header={<PublicHeader locale={locale} />}
      footer={<PublicFooter locale={locale} />}
    >
      {breadcrumbScript ? (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: breadcrumbScript }}
        />
      ) : null}
      <HotelDetailView vm={vm} similarHotels={similarHotels} />
    </PublicShell>
  );
}
