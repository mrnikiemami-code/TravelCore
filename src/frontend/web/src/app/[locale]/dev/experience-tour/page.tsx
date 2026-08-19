import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { TourDetailView } from "@/features/tour-detail/tour-detail-view";
import { loadExperienceTourDetailFixture } from "@/lib/fixtures/experience-tour-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Experience tour validation",
  robots: { index: false, follow: false },
};

/**
 * UIVAL-T003 dev-only Experience Tour Detail archetype validation.
 * Uses typed fixture — not live Tour API.
 */
export default async function ExperienceTourValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const loaded = loadExperienceTourDetailFixture(locale);
  if (!isApiOk(loaded)) {
    notFound();
  }

  const vm = loaded.data;
  if (vm.kind !== "Experience") {
    notFound();
  }

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T003 · Experience Tour Detail
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation · fixture{" "}
          <span dir="ltr">{vm.slug}</span> · kind{" "}
          <span dir="ltr">{vm.kind}</span>
        </Text>
      }
    >
      <TourDetailView vm={vm} />
    </PublicShell>
  );
}
