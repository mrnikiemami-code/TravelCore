import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { HomeDiscoveryView } from "@/features/home-discovery/home-discovery-view";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Home discovery validation",
  robots: { index: false, follow: false },
};

/** UIVAL-T007 Home / Discovery validation route. */
export default async function HomeDiscoveryValidationPage({
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
          UIVAL-T007 · Home / Discovery
        </Text>
      }
    >
      <HomeDiscoveryView locale={locale} />
    </PublicShell>
  );
}
