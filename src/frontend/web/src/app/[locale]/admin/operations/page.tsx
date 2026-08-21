import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { OpsConsoleShell } from "@/components/shell";
import { AdminOperationsBoard } from "@/features/admin-experience/admin-operations-board";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Admin Operations",
  robots: { index: false, follow: false },
};

/**
 * Data-pattern board (TC-P30-T008) hosted under P37 Ops Console shell.
 * No invented operational KPIs.
 */
export default async function AdminOperationsPage({
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
    locale === "fa" ? "برد الگوهای داده" : locale === "ar" ? "لوحة أنماط البيانات" : "Data-pattern board";

  return (
    <OpsConsoleShell
      locale={locale}
      title={title}
      breadcrumb={
        <span>
          Admin / {locale === "fa" ? "الگوها" : "Patterns"}
        </span>
      }
      currentPath={`/${locale}/admin/operations`}
    >
      <AdminOperationsBoard locale={locale} />
    </OpsConsoleShell>
  );
}
