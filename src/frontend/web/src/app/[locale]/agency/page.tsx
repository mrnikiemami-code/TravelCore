import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { AgencyShell } from "@/components/shell";
import { AgencyDashboardFoundation } from "@/features/agency-experience/agency-dashboard-foundation";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Agency Portal",
  robots: { index: false, follow: false },
};

/**
 * Agency Portal foundation (TC-P30-T009).
 * Sales workspace chrome + honest dashboard patterns — no fake B2B metrics.
 */
export default async function AgencyPanelPage({
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
    locale === "fa"
      ? "داشبورد فروش"
      : locale === "ar"
        ? "لوحة المبيعات"
        : "Sales dashboard";

  const breadcrumb =
    locale === "fa"
      ? "Agency / داشبورد"
      : locale === "ar"
        ? "Agency / لوحة"
        : "Agency / Dashboard";

  return (
    <AgencyShell
      locale={locale}
      title={title}
      breadcrumb={<span>{breadcrumb}</span>}
      currentPath={`/${locale}/agency`}
      actions={
        <a
          href={`/${locale}/tours`}
          className="min-h-touch inline-flex items-center rounded-md bg-accent px-3 text-xs font-semibold text-accent-foreground hover:opacity-95"
        >
          {locale === "fa" ? "شروع فروش" : locale === "ar" ? "بدء البيع" : "Start selling"}
        </a>
      }
    >
      <AgencyDashboardFoundation locale={locale} />
    </AgencyShell>
  );
}
