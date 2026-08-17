import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { loadTourDetailPage } from "@/features/tour-detail/load-tour-detail";
import { TourDetailView } from "@/features/tour-detail/tour-detail-view";
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
 * Public TourProduct catalog detail (TC-P09-T008/T010).
 * P09-R5: Tour translation owns current slug; SEO owns route binding / history / IndexPolicy.
 * P09-R6: default missing IndexPolicy → noindex, follow (compose / fallback).
 * Draft/Inactive → notFound (no Admin state leak). Published ≠ Index.
 * Media: Cover/Gallery via media/presentation compose (app-proxy only).
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = await loadTourDetailPage(localeParam, slug);
  if (!isApiOk(loaded)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const vm = loaded.data;
  const path = `tours/${slug}`;
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

export default async function TourDetailPage({ params }: PageProps) {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = await loadTourDetailPage(locale, slug);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const vm = loaded.data;
  const crumbs = [
    {
      name: vm.name,
      publicPath: `tours/${vm.slug}`,
    },
  ];
  const breadcrumbJsonLd = await loadSeoBreadcrumbJsonLd(locale, crumbs);
  const breadcrumbScript = serializeBreadcrumbJsonLd(breadcrumbJsonLd);

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={
        <Text role="caption">
          {locale === "fa" ? "تور" : "Tour"} · <LtrValue>{vm.slug}</LtrValue>
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "P09 — تور عمومی · SEO metadata"
            : "P09 — public Tour · SEO metadata"}
        </Text>
      }
    >
      {breadcrumbScript ? (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: breadcrumbScript }}
        />
      ) : null}
      <TourDetailView vm={vm} />
    </PublicShell>
  );
}
