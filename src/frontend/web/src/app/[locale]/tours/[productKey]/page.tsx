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
