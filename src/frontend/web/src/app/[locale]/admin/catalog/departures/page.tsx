import Link from "next/link";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { DepartureWorkflowIsland } from "@/features/admin-departure/departure-workflow-island";
import { getAdminDepartureWorkflowCopy } from "@/features/admin-departure/copy";
import { getApiBaseUrl } from "@/lib/api/config";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata = {
  robots: { index: false, follow: false },
};

export default async function AdminDepartureCatalogPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getAdminDepartureWorkflowCopy(locale);
  const apiConfigured = Boolean(getApiBaseUrl());

  return (
    <AdminShell
      header={
        <Text as="h1" role="heading">
          {copy.pageTitle}
        </Text>
      }
      navigation={
        <nav aria-label={copy.navLabel}>
          <ul className="flex flex-col gap-2 text-sm">
            <li>
              <Link
                className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                href={`/${locale}/admin/catalog`}
              >
                {copy.backToHub}
              </Link>
            </li>
            <li>
              <Link
                className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                href={`/${locale}/admin/catalog/tours`}
              >
                {copy.toursLink}
              </Link>
            </li>
          </ul>
        </nav>
      }
    >
      <div className="flex flex-col gap-4 p-4">
        <DepartureWorkflowIsland locale={locale} apiConfigured={apiConfigured} />
      </div>
    </AdminShell>
  );
}
