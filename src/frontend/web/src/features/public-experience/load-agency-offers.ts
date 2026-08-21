import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";

export type AgencyOfferView = {
  agencyOfferId: string;
  tourProductId: string;
  agencyDisplayName: string;
  agencyDescription: string | null;
  publicEmail: string | null;
  publicPhone: string | null;
  websiteUrl: string | null;
  titleOverride: string | null;
  highlight: string | null;
  requiresManualConfirmation: boolean;
};

type ApiAgencyOffer = {
  agencyOfferId: string;
  tourProductId: string;
  agencyDisplayName: string;
  agencyDescription?: string | null;
  publicEmail?: string | null;
  publicPhone?: string | null;
  websiteUrl?: string | null;
  titleOverride?: string | null;
  highlight?: string | null;
  requiresManualConfirmation: boolean;
};

function mapOffer(item: ApiAgencyOffer): AgencyOfferView {
  return {
    agencyOfferId: item.agencyOfferId,
    tourProductId: item.tourProductId,
    agencyDisplayName: item.agencyDisplayName,
    agencyDescription: item.agencyDescription?.trim() || null,
    publicEmail: item.publicEmail?.trim() || null,
    publicPhone: item.publicPhone?.trim() || null,
    websiteUrl: item.websiteUrl?.trim() || null,
    titleOverride: item.titleOverride?.trim() || null,
    highlight: item.highlight?.trim() || null,
    requiresManualConfirmation: item.requiresManualConfirmation === true,
  };
}

/**
 * P38-T004 / P14-R7: Published Public-channel AgencyOffer facts. Fail soft — empty on error.
 */
export async function loadAgencyOffersByTourProduct(
  tourProductId: string,
): Promise<AgencyOfferView[]> {
  const id = tourProductId.trim();
  if (!id) {
    return [];
  }

  const qs = new URLSearchParams({ tourProductId: id });
  const result = await apiGetJson<ApiAgencyOffer[]>(
    `/api/agency-marketplace/offers/related-published?${qs.toString()}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result) || !Array.isArray(result.data)) {
    return [];
  }
  return result.data.map(mapOffer).slice(0, 6);
}
