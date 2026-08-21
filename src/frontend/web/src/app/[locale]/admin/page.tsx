import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { OpsConsoleShell } from "@/components/shell";
import { AdminConsoleFoundation } from "@/features/admin-experience/admin-console-foundation";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Admin Console",
  robots: { index: false, follow: false },
};

/**
 * Admin Console hub (TC-P37-T004).
 */
export default async function AdminConsolePage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  return (
    <OpsConsoleShell
      locale={locale}
      breadcrumb={<span>Admin / Dashboard</span>}
      currentPath={`/${locale}/admin`}
    >
      <AdminConsoleFoundation locale={locale} />
    </OpsConsoleShell>
  );
}
