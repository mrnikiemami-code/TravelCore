import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { ForeignTourDetailView } from "@/features/foreign-tour-detail/foreign-tour-detail-view";
import { loadForeignTourDetailFixture } from "@/lib/fixtures/foreign-tour-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Foreign tour validation",
  robots: { index: false, follow: false },
};

/**
 * UIVAL-T002 dev-only Foreign Package Tour Detail archetype validation.
 * Uses typed P02 walking-skeleton fixture — not live Tour API.
 */
export default async function ForeignTourValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const loaded = loadForeignTourDetailFixture(locale);
  if (!isApiOk(loaded)) {
    notFound();
  }

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T002 · Foreign Package Tour Detail
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation · fixture{" "}
          <span dir="ltr">{loaded.data.product.productKey}</span>
        </Text>
      }
    >
      <ForeignTourDetailView vm={loaded.data} />
    </PublicShell>
  );
}
