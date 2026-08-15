import Link from "next/link";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { getIdentityPartyWorkflowCopy } from "@/features/admin-identity-party/copy";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { getApiBaseUrl } from "@/lib/api/config";

export const metadata = {
  robots: { index: false, follow: false },
};

export default async function AdminAccountsHubPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = getIdentityPartyWorkflowCopy(locale);
  const apiConfigured = Boolean(getApiBaseUrl());

  return (
    <AdminShell
      header={
        <Text as="h1" role="heading">
          {copy.hubTitle}
        </Text>
      }
      navigation={
        <nav aria-label={copy.navLabel}>
          <ul className="flex flex-col gap-2 text-sm">
            <li>
              <Link
                className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                href={`/${locale}/admin/accounts`}
              >
                {copy.navLabel}
              </Link>
            </li>
            <li>
              <Link
                className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                href={`/${locale}/admin/accounts/onboard`}
              >
                {copy.startJourney}
              </Link>
            </li>
          </ul>
        </nav>
      }
    >
      <div className="flex flex-col gap-4 p-4">
        <Text role="muted">{copy.hubBody}</Text>
        {!apiConfigured ? (
          <Text role="caption">{copy.apiMissing}</Text>
        ) : null}
        <Link
          className="min-h-touch inline-flex w-fit items-center rounded-md bg-foreground px-4 text-background"
          href={`/${locale}/admin/accounts/onboard`}
        >
          {copy.startJourney}
        </Link>
      </div>
    </AdminShell>
  );
}
