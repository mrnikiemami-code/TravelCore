import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { loadTravelogueDiscoveryList } from "@/features/travelogue-detail/load-travelogue-list";
import { TravelogueDiscoveryView } from "@/features/travelogue-detail/travelogue-discovery-view";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadComposedSeoMetadata } from "@/lib/seo/load-composed-metadata";
import {
  languagesFromComposed,
  robotsFromComposed,
} from "@/lib/seo/metadata-contract";

type PageProps = {
  params: Promise<{ locale: string }>;
};

/**
 * Public travelogue discovery index (TC-DISCLINK-T001).
 * UGC discovery — not editorial CMS listing · not Search engine.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const title =
    localeParam === "fa" ? "سفرنامه‌ها" : "Travelogues";
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path: "travelogues",
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

export default async function TravelogueDiscoveryPage({ params }: PageProps) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const travelogues = await loadTravelogueDiscoveryList(locale);

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={
        <Text role="caption">
          {locale === "fa" ? "کشف سفرنامه" : "Travelogue discovery"}
        </Text>
      }
    >
      <TravelogueDiscoveryView locale={locale} travelogues={travelogues} />
    </PublicShell>
  );
}
