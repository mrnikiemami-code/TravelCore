import { notFound } from "next/navigation";
import { AgencyShell } from "@/components/shell";
import { loadAgencyOfferDetail } from "@/features/agency-experience/agency-offer-ops-actions";
import { AgencyOfferOpsDetailView } from "@/features/agency-experience/agency-offer-ops-view";
import { Text } from "@/components/ui";
import { isAppLocale } from "@/lib/i18n";
import Link from "next/link";

/** Agency Offer detail + lifecycle (TC-P38-T007). */
export default async function Page({
  params,
}: {
  params: Promise<{ locale: string; offerId: string }>;
}) {
  const { locale: localeParam, offerId } = await params;
  if (!isAppLocale(localeParam) || !offerId) notFound();
  const locale = localeParam;

  const loaded = await loadAgencyOfferDetail(offerId);
  if (!loaded.ok) {
    return (
      <AgencyShell
        locale={locale}
        title="catalog"
        breadcrumb={<span>Agency / catalog / offer</span>}
        currentPath={`/${locale}/agency/catalog`}
      >
        <div className="flex flex-col gap-4">
          <Text role="muted">{loaded.message}</Text>
          <Link
            href={`/${locale}/agency/catalog`}
            className="min-h-touch inline-flex w-fit items-center rounded-lg border border-border px-4 text-sm"
          >
            Back to offers
          </Link>
        </div>
      </AgencyShell>
    );
  }

  return (
    <AgencyShell
      locale={locale}
      title="catalog"
      breadcrumb={<span>Agency / catalog / offer</span>}
      currentPath={`/${locale}/agency/catalog`}
    >
      <AgencyOfferOpsDetailView locale={locale} item={loaded.item} />
    </AgencyShell>
  );
}
