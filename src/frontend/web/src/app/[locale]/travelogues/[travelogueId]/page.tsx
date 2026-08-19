import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { loadTravelogueDetailPage } from "@/features/travelogue-detail/load-travelogue-detail";
import { TravelogueDetailView } from "@/features/travelogue-detail/travelogue-detail-view";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadComposedSeoMetadata } from "@/lib/seo/load-composed-metadata";
import {
  languagesFromComposed,
  robotsFromComposed,
} from "@/lib/seo/metadata-contract";

type PageProps = {
  params: Promise<{ locale: string; travelogueId: string }>;
};

/**
 * Public Travelogue detail (TC-PRODSURF-T002).
 * UGC narrative — not editorial Content Article.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, travelogueId } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = await loadTravelogueDetailPage(localeParam, travelogueId);
  if (!isApiOk(loaded)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const travelogue = loaded.data;
  const path = `travelogues/${travelogue.travelogueId}`;
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path,
    localizedTitle: travelogue.title,
    localizedDescription: travelogue.body,
  });

  if (!composed) {
    return {
      title: travelogue.title,
      description: travelogue.body.slice(0, 160),
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

export default async function TravelogueDetailPage({ params }: PageProps) {
  const { locale: localeParam, travelogueId } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = await loadTravelogueDetailPage(locale, travelogueId);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const travelogue = loaded.data;

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={
        <Text role="caption">
          {locale === "fa" ? "سفرنامه" : "Travelogue"} ·{" "}
          <LtrValue>{travelogue.travelogueId}</LtrValue>
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "P16 — سفرنامه عمومی · UGC"
            : "P16 — public Travelogue · UGC"}
        </Text>
      }
    >
      <TravelogueDetailView locale={locale} travelogue={travelogue} />
    </PublicShell>
  );
}
