import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { Container } from "@/components/ui";
import { PublicBookingStatusView } from "@/features/booking/status-view";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string; bookingId: string }>;
};

/**
 * Authorized private Booking read (TC-P36-T005 commerce polish).
 * Transaction page: always noindex. Token is never placed in the URL.
 */
export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "TravelCore",
    robots: { index: false, follow: false },
  };
}

export default async function PublicBookingStatusPage({ params }: PageProps) {
  const { locale: localeParam, bookingId } = await params;
  if (!isAppLocale(localeParam) || !bookingId) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  return (
    <PublicShell
      header={<PublicHeader locale={locale} />}
      footer={<PublicFooter locale={locale} />}
    >
      <Container width="narrow" className="py-6 sm:py-8">
        <PublicBookingStatusView locale={locale} bookingId={bookingId} />
      </Container>
    </PublicShell>
  );
}
