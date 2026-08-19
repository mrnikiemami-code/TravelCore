import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { HomeDiscoveryView } from "@/features/home-discovery/home-discovery-view";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadComposedSeoMetadata } from "@/lib/seo/load-composed-metadata";
import {
  languagesFromComposed,
  robotsFromComposed,
} from "@/lib/seo/metadata-contract";

/**
 * Production locale home — Home / Discovery entry (TC-PRODDEL-T001/T002).
 * Replaces P02 foundation smoke; UIVAL validation remains at /dev/home-discovery.
 */
export async function generateMetadata({
  params,
}: {
  params: Promise<{ locale: string }>;
}): Promise<Metadata> {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const title =
    localeParam === "fa"
      ? "کشف TravelCore"
      : localeParam === "ar"
        ? "اكتشف TravelCore"
        : "Discover TravelCore";
  const description =
    localeParam === "fa"
      ? "ورودی‌های عمومی محصول TravelCore"
      : localeParam === "ar"
        ? "نقاط دخول عامة لمنتج TravelCore"
        : "TravelCore public product entry points";

  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path: "",
    localizedTitle: title,
    localizedDescription: description,
  });

  if (!composed) {
    return {
      title,
      description,
      robots: { index: false, follow: true },
    };
  }

  const languages = languagesFromComposed(composed);
  const robots = robotsFromComposed(composed);

  return {
    title: composed.title,
    description: composed.description ?? description,
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

export default async function LocaleHomePage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "TravelCore — ورود عمومی"
            : locale === "ar"
              ? "TravelCore — الدخول العام"
              : "TravelCore — public entry"}
        </Text>
      }
    >
      <HomeDiscoveryView locale={locale} />
    </PublicShell>
  );
}
