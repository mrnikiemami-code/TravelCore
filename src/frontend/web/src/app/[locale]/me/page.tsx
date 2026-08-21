import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { CustomerShell } from "@/components/shell";
import { CustomerDashboardFoundation } from "@/features/customer-experience/customer-dashboard-foundation";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "My trips · TravelCore",
  robots: { index: false, follow: false },
};

/**
 * Customer Dashboard overview (TC-P37-T002).
 * Consumer product foundation — no fake trips/bookings.
 */
export default async function CustomerDashboardPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const title =
    locale === "fa" ? "سفرهای من" : locale === "ar" ? "رحلاتي" : "My trips";
  const breadcrumb =
    locale === "fa"
      ? "مسافر / نمای کلی"
      : locale === "ar"
        ? "مسافر / نظرة عامة"
        : "Traveler / Overview";

  return (
    <CustomerShell
      locale={locale}
      title={title}
      breadcrumb={<span>{breadcrumb}</span>}
      currentPath={`/${locale}/me`}
    >
      <CustomerDashboardFoundation locale={locale} />
    </CustomerShell>
  );
}
