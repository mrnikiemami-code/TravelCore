import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { LtrValue, Text } from "@/components/ui";
import { DestinationLandingView } from "@/features/destination-landing/destination-landing-view";
import { loadDestinationLandingPage } from "@/features/destination-landing/load-destination-landing";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadSeoHreflangLanguagesByPath } from "@/lib/seo/load-hreflang";

type PageProps = {
  params: Promise<{ locale: string; slug: string }>;
};

/**
 * Public Destination detail baseline (TC-P04-T009).
 * R3 RESOLVED: page may exist for humans; robots = noindex, follow.
 * TC-P05-T005: IndexPolicy API/contract exists but this page keeps hardcoded
 * noindex until metadata composition (T007+) — no mass flip.
 * TC-P05-T006: hreflang alternates from SEO bindings only (no fabricated locales).
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
  const languages = await loadSeoHreflangLanguagesByPath(
    localeParam,
    `destinations/${slug}`,
  );

  return {
    title: vm.name,
    description: vm.description ?? undefined,
    ...(Object.keys(languages).length > 0
      ? { alternates: { languages } }
      : {}),
    robots: {
      index: false,
      follow: true,
    },
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
            ? "P04 — مقصد عمومی · noindex"
            : "P04 — public Destination · noindex"}
        </Text>
      }
    >
      <DestinationLandingView vm={vm} />
    </PublicShell>
  );
}
