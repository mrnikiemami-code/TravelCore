import { notFound } from "next/navigation";
import { AgencyShell } from "@/components/shell";
import {
  loadActingAgencyProfile,
  loadAgencyOffersForActing,
} from "@/features/agency-experience/agency-offer-ops-actions";
import { AgencyOfferOpsListView } from "@/features/agency-experience/agency-offer-ops-view";
import { isAppLocale } from "@/lib/i18n";

/**
 * Agency Offer Operations list (TC-P38-T007) — replaces honesty-empty catalog shell.
 */
export default async function Page({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) notFound();
  const locale = localeParam;

  const profileResult = await loadActingAgencyProfile();
  const profile = profileResult.ok ? profileResult.profile : null;
  const offersResult = profile ? await loadAgencyOffersForActing() : null;
  const items = offersResult?.ok ? offersResult.items : [];
  const loadError = !profileResult.ok
    ? profileResult.message
    : offersResult && !offersResult.ok
      ? offersResult.message
      : null;

  return (
    <AgencyShell
      locale={locale}
      title="catalog"
      breadcrumb={<span>Agency / catalog</span>}
      currentPath={`/${locale}/agency/catalog`}
    >
      <AgencyOfferOpsListView
        locale={locale}
        profile={profile}
        items={items}
        loadError={loadError}
      />
    </AgencyShell>
  );
}
