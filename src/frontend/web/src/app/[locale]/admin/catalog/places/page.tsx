import Link from "next/link";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { PlaceWorkflowIsland } from "@/features/admin-place/place-workflow-island";
import { getAdminPlaceWorkflowCopy } from "@/features/admin-place/copy";
import { getApiBaseUrl } from "@/lib/api/config";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata = {
  robots: { index: false, follow: false },
};

export default async function AdminPlaceCatalogPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getAdminPlaceWorkflowCopy(locale);

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
                href={`/${locale}/admin/catalog/destinations`}
              >
                {copy.destinationsLink}
              </Link>
            </li>
            <li>
              <Link
                className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                href={`/${locale}/admin/media`}
              >
                {copy.mediaLink}
              </Link>
            </li>
            <li>
              <Link
                className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                href={`/${locale}/admin/accounts`}
              >
                {copy.accountsJob}
              </Link>
            </li>
          </ul>
        </nav>
      }
    >
      <div className="flex flex-col gap-4 p-4">
        <Text role="muted">{copy.pageIntro}</Text>
        <PlaceWorkflowIsland
          locale={locale}
          apiConfigured={Boolean(getApiBaseUrl())}
        />
      </div>
    </AdminShell>
  );
}
