import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { VisaDetailView } from "@/features/visa-detail/visa-detail-view";
import { loadVisaDetailFixture } from "@/lib/fixtures/visa-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Visa detail validation",
  robots: { index: false, follow: false },
};

/** UIVAL-T010 Visa validation. */
export default async function VisaValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const loaded = loadVisaDetailFixture(locale);
  if (!isApiOk(loaded)) {
    notFound();
  }

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T010 · Visa
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation · <span dir="ltr">{loaded.data.code}</span>
        </Text>
      }
    >
      <VisaDetailView vm={loaded.data} relatedContent={[]} />
    </PublicShell>
  );
}
