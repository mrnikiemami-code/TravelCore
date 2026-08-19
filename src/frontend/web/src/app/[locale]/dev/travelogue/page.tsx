import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { TravelogueDetailView } from "@/features/travelogue-detail/travelogue-detail-view";
import { loadTravelogueDetailFixture } from "@/lib/fixtures/travelogue-detail";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Travelogue validation",
  robots: { index: false, follow: false },
};

/** UIVAL-T009 Travelogue validation. */
export default async function TravelogueValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const travelogue = loadTravelogueDetailFixture(locale);

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T009 · Travelogue
        </Text>
      }
    >
      <TravelogueDetailView locale={locale} travelogue={travelogue} />
    </PublicShell>
  );
}
