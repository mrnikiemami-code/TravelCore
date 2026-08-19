import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PlaceDetailView } from "@/features/place-detail/place-detail-view";
import { loadHotelDetailFixture } from "@/lib/fixtures/hotel-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Hotel detail validation",
  robots: { index: false, follow: false },
};

/**
 * UIVAL-T006 dev-only Hotel Detail (Place catalog) validation.
 */
export default async function HotelDetailValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const loaded = loadHotelDetailFixture(locale);
  if (!isApiOk(loaded)) {
    notFound();
  }

  if (loaded.data.kind !== "Hotel") {
    notFound();
  }

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T006 · Hotel Detail
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation · catalog Place · slug{" "}
          <span dir="ltr">{loaded.data.slug}</span>
        </Text>
      }
    >
      <PlaceDetailView vm={loaded.data} />
    </PublicShell>
  );
}
