import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { PublicTourLandingView } from "@/features/public-experience/landing-view";
import { loadRelatedContentByDestination } from "@/features/public-experience/load-related-content";
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
  params: Promise<{ locale: string; slug: string; intent: string }>;
};

/**
 * Public tour SEO landing (TC-P14-T003 / P14-R3).
 * Search-intent composition — not a filtered listing, not Search engine, not SEO policy owner.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, slug, intent } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const title = `${slug} · ${intent}`;
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path: `tours/${slug}/${intent}`,
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

export default async function PublicTourLandingPage({ params }: PageProps) {
  const { locale: localeParam, slug, intent } = await params;
  if (!isAppLocale(localeParam) || !slug || !intent) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const destination = await apiGetJson<{ destinationId: string }>(
    `/api/destination/destinations/by-slug/${encodeURIComponent(locale)}/${encodeURIComponent(slug)}`,
    { cache: "no-store" },
  );
  const relatedTours = isApiOk(destination)
    ? await loadRelatedToursByDestination(destination.data.destinationId, locale)
    : [];
  const relatedContent = isApiOk(destination)
    ? await loadRelatedContentByDestination(destination.data.destinationId, locale)
    : [];

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={
        <Text role="caption">
          {locale === "fa" ? "لندینگ سئو" : "SEO landing"} ·{" "}
          <LtrValue>
            {slug}/{intent}
          </LtrValue>
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "P14 — نیت جستجو · نه فهرست فیلترشده · قواعد نمایه‌سازی نزد SEO"
            : "P14 — search intent · not a filtered listing · SEO still owns index rules"}
        </Text>
      }
    >
      <PublicTourLandingView
        locale={locale}
        topic={slug}
        intent={intent}
        relatedTours={relatedTours}
        relatedContent={relatedContent}
      />
    </PublicShell>
  );
}
