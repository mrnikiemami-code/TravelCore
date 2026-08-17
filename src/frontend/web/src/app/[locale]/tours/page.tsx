import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { parseListingFilterCriteria } from "@/features/public-experience/filter-presentation";
import { PublicTourListingView } from "@/features/public-experience/listing-view";
import { loadRelatedToursByDestination } from "@/features/public-experience/load-related-tours";
import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadComposedSeoMetadata } from "@/lib/seo/load-composed-metadata";
import {
  languagesFromComposed,
  robotsFromComposed,
} from "@/lib/seo/metadata-contract";

type PageProps = {
  params: Promise<{ locale: string }>;
  searchParams: Promise<{ destination?: string; sort?: string }>;
};

/**
 * Public tour listing (TC-P14-T003 / P14-R3 · TC-P14-T008 / P14-R8).
 * Discovery + presentation filters only — not Search, not SEO landing owner.
 * Filtered query variants keep the same listing SEO path (`tours`).
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const title = localeParam === "fa" ? "فهرست تورها" : "Tours";
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
  const query = await searchParams;
  const criteria = parseListingFilterCriteria(query);

  let selection: Awaited<ReturnType<typeof loadRelatedToursByDestination>> = [];
  if (criteria.destinationSlug) {
    const destination = await apiGetJson<{ destinationId: string }>(
      `/api/destination/destinations/by-slug/${encodeURIComponent(locale)}/${encodeURIComponent(criteria.destinationSlug)}`,
      { cache: "no-store" },
    );
    if (isApiOk(destination)) {
      selection = await loadRelatedToursByDestination(
        destination.data.destinationId,
        locale,
      );
    }
  }

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={
        <Text role="caption">
          {locale === "fa" ? "فهرست عمومی تور" : "Public tour listing"}
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "P14 — کشف · نه جستجو · نه لندینگ سئو"
            : "P14 — discovery · not search · not SEO landing"}
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
