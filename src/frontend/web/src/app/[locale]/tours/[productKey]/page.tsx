import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { ForeignTourDetailView } from "@/features/foreign-tour-detail/foreign-tour-detail-view";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadForeignTourDetailFixture } from "@/lib/fixtures/foreign-tour-detail";
import { isApiOk } from "@/lib/api/result";

type PageProps = {
  params: Promise<{ locale: string; productKey: string }>;
};

const PUBLISHED_TOUR_LOCALES = ["fa", "en"] as const;

/**
 * Server metadata from T012 PVM/fixture (T015).
 * Not a SEO engine — no persistence, no fabricated AR publication.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, productKey } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = loadForeignTourDetailFixture(localeParam);
  if (!isApiOk(loaded) || loaded.data.product.productKey !== productKey) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const vm = loaded.data;
  const canonicalPath = `/${vm.locale}/tours/${vm.product.productKey}`;

  const languages: Record<string, string> = {};
  for (const loc of PUBLISHED_TOUR_LOCALES) {
    const alt = loadForeignTourDetailFixture(loc);
    if (isApiOk(alt) && alt.data.product.productKey === productKey) {
      languages[loc] = `/${loc}/tours/${productKey}`;
    }
  }

  return {
    title: vm.seo.title,
    description: vm.seo.description,
    alternates: {
      canonical: canonicalPath,
      languages,
    },
    openGraph: {
      title: vm.seo.title,
      description: vm.seo.description,
      locale: vm.locale === "fa" ? "fa_IR" : "en_US",
      type: "website",
      url: canonicalPath,
    },
    robots: {
      index: true,
      follow: true,
    },
  };
}

/**
 * Locale-aware Foreign Package Tour Detail walking skeleton (T013).
 * Server Component only — loads T012 fixture via T009 boundary.
 */
export default async function ForeignTourDetailPage({ params }: PageProps) {
  const { locale: localeParam, productKey } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = loadForeignTourDetailFixture(locale);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const vm = loaded.data;
  if (vm.product.productKey !== productKey) {
    notFound();
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
          {locale === "fa" ? "تور پکیج خارجی" : "Foreign package tour"} ·{" "}
          {vm.product.productKey}
        </Text>
      }
      footer={
        <Text role="caption">
          {locale === "fa"
            ? "Walking Skeleton — بدون رزرو واقعی"
            : "Walking Skeleton — no live booking"}
        </Text>
      }
    >
      <ForeignTourDetailView vm={vm} />
    </PublicShell>
  );
}
