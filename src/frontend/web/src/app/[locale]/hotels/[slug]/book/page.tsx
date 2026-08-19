import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PublicHotelBookingPrepareForm } from "@/features/hotel-booking/prepare-form";
import { getPublicHotelBookingCopy } from "@/features/hotel-booking/copy";
import { loadPlaceDetailPage } from "@/features/place-detail/load-place-detail";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string; slug: string }>;
};

/**
 * Public HotelBooking initiation via hotels route (TC-PRODSURF-T006 / P21-R8).
 * Child of accepted Hotel catalog route. Transaction page: always noindex.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicHotelBookPage({ params }: PageProps) {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const loaded = await loadPlaceDetailPage(locale, slug);
  if (!isApiOk(loaded) || loaded.data.kind !== "Hotel") {
    notFound();
  }

  const copy = getPublicHotelBookingCopy(locale);

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={<Text role="caption">{copy.prepareTitle}</Text>}
      footer={<Text role="caption">{copy.notConfirmed}</Text>}
    >
      <div className="py-6 sm:py-8">
        <div className="mx-auto flex w-full max-w-xl flex-col gap-4 px-4">
          <Text as="h1" role="heading">
            {copy.prepareTitle}
          </Text>
          <Text>{copy.prepareNote}</Text>
          <PublicHotelBookingPrepareForm
            locale={locale}
            slug={slug}
            placeId={loaded.data.placeId}
          />
        </div>
      </div>
    </PublicShell>
  );
}
