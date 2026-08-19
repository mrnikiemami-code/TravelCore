import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { DestinationLandingView } from "@/features/destination-landing/destination-landing-view";
import { loadDestinationLandingFixture } from "@/lib/fixtures/destination-landing";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Destination landing validation",
  robots: { index: false, follow: false },
};

/**
 * UIVAL-T005 dev-only Destination Landing validation.
 */
export default async function DestinationLandingValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const loaded = loadDestinationLandingFixture(locale);
  if (!isApiOk(loaded)) {
    notFound();
  }

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T005 · Destination Landing
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation · slug <span dir="ltr">{loaded.data.slug}</span>
        </Text>
      }
    >
      <DestinationLandingView vm={loaded.data} />
    </PublicShell>
  );
}
