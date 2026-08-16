import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { DestinationLandingView } from "@/features/destination-landing/destination-landing-view";
import { loadDestinationLandingPage } from "@/features/destination-landing/load-destination-landing";
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
 * Public Destination detail baseline (TC-P04-T009).
 * TC-P05-T007: SEO-aware metadata composition (robots via IndexPolicy / R2).
 * TC-P05-T008: truthful BreadcrumbList JSON-LD via SEO structured-data framework.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = await loadDestinationLandingPage(localeParam, slug);
  if (!isApiOk(loaded)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const vm = loaded.data;
  const path = `destinations/${slug}`;
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path,
    localizedTitle: vm.name,
    localizedDescription: vm.description,
  });

  // Conservative fallback if SEO compose unavailable — still noindex.
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

export default async function DestinationLandingPage({ params }: PageProps) {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = await loadDestinationLandingPage(locale, slug);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const vm = loaded.data;
  const breadcrumbJsonLd = await loadSeoBreadcrumbJsonLd(
    locale,
    vm.breadcrumb.map((crumb) => ({
      name: crumb.name,
      publicPath: crumb.slug ? `destinations/${crumb.slug}` : null,
    })),
  );
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
          {locale === "fa" ? "مقصد" : "Destination"} ·{" "}
          <LtrValue>{vm.slug}</LtrValue>
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "P05 — مقصد عمومی · SEO metadata"
            : "P05 — public Destination · SEO metadata"}
        </Text>
      }
    >
      {breadcrumbScript ? (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: breadcrumbScript }}
        />
      ) : null}
      <DestinationLandingView vm={vm} />
    </PublicShell>
  );
}
