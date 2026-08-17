import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { loadRelatedContentByDestinations } from "@/features/public-experience/load-related-content";
import { loadVisaDetailPage } from "@/features/visa-detail/load-visa-detail";
import { VisaDetailView } from "@/features/visa-detail/visa-detail-view";
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
  params: Promise<{ locale: string; code: string }>;
};

/**
 * Public VisaDetailPage (TC-P17-T007 / P17-R7).
 * Visa owns structured facts. Content may enrich. SEO owns IndexPolicy.
 * Public presence != automatically indexed. No application workflow (P17-R8 UNRESOLVED).
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, code } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = await loadVisaDetailPage(localeParam, code);
  if (!isApiOk(loaded)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const vm = loaded.data;
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path: vm.publicPath,
    localizedTitle: vm.name,
    localizedDescription: vm.summary,
  });

  if (!composed) {
    return {
      title: vm.name,
      description: vm.summary ?? undefined,
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

export default async function VisaDetailPage({ params }: PageProps) {
  const { locale: localeParam, code } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = await loadVisaDetailPage(locale, code);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const vm = loaded.data;
  const destinationIds = [
    ...new Set(
      vm.requirementSets.map((set) => set.applicability.destinationGeographicId),
    ),
  ];
  const relatedContent = await loadRelatedContentByDestinations(
    destinationIds,
    locale,
  );
  const breadcrumbJsonLd = await loadSeoBreadcrumbJsonLd(locale, [
    {
      name: vm.name,
      publicPath: vm.publicPath,
    },
  ]);
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
          {locale === "fa" ? "ویزا" : "Visa"} · <LtrValue>{vm.code}</LtrValue>
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "P17 — ویزای عمومی · SEO metadata"
            : "P17 — public Visa · SEO metadata"}
        </Text>
      }
    >
      {breadcrumbScript ? (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: breadcrumbScript }}
        />
      ) : null}
      <VisaDetailView vm={vm} relatedContent={relatedContent} />
    </PublicShell>
  );
}
