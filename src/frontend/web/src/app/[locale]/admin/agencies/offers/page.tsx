import Link from "next/link";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { AgencyOfferGovernanceWorkflowIsland } from "@/features/admin-agency-offer-governance/agency-offer-governance-workflow-island";
import { getAdminAgencyOfferGovernanceCopy } from "@/features/admin-agency-offer-governance/copy";
import { getApiBaseUrl } from "@/lib/api/config";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata = {
  title: "Agency offer governance",
  robots: { index: false, follow: false },
};

export default async function AdminAgencyOfferGovernancePage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getAdminAgencyOfferGovernanceCopy(locale);

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
                href={`/${locale}/admin/agencies`}
              >
                {copy.backToAgencies}
              </Link>
            </li>
          </ul>
        </nav>
      }
    >
      <div className="flex flex-col gap-4 p-4">
        <AgencyOfferGovernanceWorkflowIsland
          locale={locale}
          apiConfigured={Boolean(getApiBaseUrl())}
        />
      </div>
    </AdminShell>
  );
}
