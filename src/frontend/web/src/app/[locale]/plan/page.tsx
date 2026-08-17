import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { TripPlannerPageView } from "@/features/trip-planner/trip-planner-page-view";
import { getTripPlannerWorkflowCopy } from "@/features/trip-planner/copy";
import { getApiBaseUrl } from "@/lib/api/config";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

type PageProps = {
  params: Promise<{ locale: string }>;
};

/**
 * Public Trip Planner route (TC-P18-T008 / P18-R8).
 * Honest follow-up CTA only — no Book Now / Checkout / Pay.
 */
export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const copy = getTripPlannerWorkflowCopy(localeParam);
  return {
    title: copy.pageTitle,
    description: copy.pageIntro,
    robots: { index: false, follow: true },
  };
}

export default async function TripPlannerPage({ params }: PageProps) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getTripPlannerWorkflowCopy(locale);
  const apiConfigured = Boolean(getApiBaseUrl());

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      context={
        <Text role="caption">
          {locale === "fa" ? "برنامه‌ریزی سفر" : locale === "ar" ? "مخطط الرحلة" : "Trip planner"} · P18
        </Text>
      }
      footer={
        <Text role="caption">
          {copy.honestCtaNote}
        </Text>
      }
    >
      <TripPlannerPageView locale={locale} apiConfigured={apiConfigured} />
    </PublicShell>
  );
}
