import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { HomeDiscoveryView } from "@/features/home-discovery/home-discovery-view";
import { loadHomeDiscoveryComposition } from "@/features/home-discovery/load-home-discovery-composition";
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
      ? "TravelCore — بازار گردشگری حرفه‌ای"
      : localeParam === "ar"
        ? "TravelCore — سوق سفر احترافي"
        : "TravelCore — Professional travel marketplace";
  const description =
    localeParam === "fa"
      ? "صفحه اصلی تجاری TravelCore برای کشف مقصد، هتل و تور — کاتالوگ واقعی بدون قیمت یا موجودی جعلی."
      : localeParam === "ar"
        ? "الصفحة التجارية الرئيسية لـ TravelCore لاكتشاف الوجهات والفنادق والجولات — كتالوج حقيقي دون أسعار أو توفر وهمي."
        : "TravelCore commercial home for destinations, hotels, and tours — real catalog without fake prices or availability.";

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
  const composition = await loadHomeDiscoveryComposition(locale);

  return (
    <PublicShell
      header={<PublicHeader locale={locale} />}
      footer={<PublicFooter locale={locale} />}
    >
      <HomeDiscoveryView locale={locale} composition={composition} />
    </PublicShell>
  );
}
