import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { AdminNav } from "@/features/admin-experience/admin-nav";
import { AdminOperationsBoard } from "@/features/admin-experience/admin-operations-board";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Admin Operations",
  robots: { index: false, follow: false },
};

/**
 * Representative Admin Experience board (TC-P30-T008).
 * Shell + data-grid + workflow patterns — no invented operational KPIs.
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
    locale === "fa" ? "کنسول عملیات" : locale === "ar" ? "وحدة العمليات" : "Operations console";
  const context =
    locale === "fa"
      ? "پایه تجربه ادمین · بدون KPI جعلی"
      : locale === "ar"
        ? "أساس تجربة الإدارة · بدون مؤشرات وهمية"
        : "Admin experience foundation · no fake KPIs";

  return (
    <AdminShell
      header={title}
      context={context}
      breadcrumb={
        <span>
          Admin / {locale === "fa" ? "عملیات" : "Operations"}
        </span>
      }
      navigation={<AdminNav locale={locale} currentPath={`/${locale}/admin/operations`} />}
      actions={
        <a
          href={`/${locale}/admin/catalog`}
          className="min-h-touch inline-flex items-center rounded-md border border-border bg-surface px-3 text-xs font-medium hover:bg-surface-muted"
        >
          {locale === "fa" ? "کاتالوگ" : "Catalog"}
        </a>
      }
    >
      <AdminOperationsBoard locale={locale} />
    </AdminShell>
  );
}
