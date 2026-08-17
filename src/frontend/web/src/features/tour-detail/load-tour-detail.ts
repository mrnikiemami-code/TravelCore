import { apiGetJson } from "@/lib/api/client";
import { asPageViewModel } from "@/lib/api/read-models";
import { apiFail, isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";
import {
  mediaOriginalContentPath,
  resolveMediaAppProxySrc,
} from "@/lib/media/media-presentation";
import type { AgencyOfferView } from "@/features/public-experience/load-agency-offers";
import { loadAgencyOffersByTourProduct } from "@/features/public-experience/load-agency-offers";
import type { RelatedContentView } from "@/features/public-experience/load-related-content";
import { loadRelatedContentByDestinations } from "@/features/public-experience/load-related-content";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import { loadRelatedToursByProduct } from "@/features/public-experience/load-related-tours";
import type { UgcCompositionView } from "@/features/public-experience/load-ugc-composition";
import { loadUgcComposition } from "@/features/public-experience/load-ugc-composition";

export type TourMediaItemView = {
  mediaAssetId: string;
  role: string;
  sortOrder: number;
  src: string | null;
  alt: string;
  width: number | null;
  height: number | null;
};

export type PublishedDepartureTransportView = {
  sequence: number;
  transportMode: string;
  origin: string;
  destination: string;
};

export type PublishedDepartureAccommodationView = {
  placeId: string;
  nights: number;
  boardType: string;
};

export type PublicMoneyView = {
  amount: number;
  currencyCode: string;
};

export type PublicPriceComponentView = {
  kind: string;
  money: PublicMoneyView;
};

export type PublicOccupancyPriceView = {
  passengerCategory: string;
  occupancyCategory: string;
  money: PublicMoneyView;
};

/** Public price summary facts (P12-R8). Source currency only — no converted display money. */
export type PublicPriceSummaryView = {
  priceId: string;
  targetType: string;
  targetId: string;
  currency: string;
  components: PublicPriceComponentView[];
  occupancyPrices: PublicOccupancyPriceView[];
};

/** Public published execution summary (P11-R8). Published ≠ bookable. */
export type PublishedDepartureView = {
  id: string;
  status: string;
  startDate: string | null;
  endDate: string | null;
  timeZoneId: string | null;
  durationDays: number | null;
  minimumPax: number | null;
  maximumPax: number | null;
  transport: PublishedDepartureTransportView[];
  accommodation: PublishedDepartureAccommodationView[];
  priceSummary: PublicPriceSummaryView | null;
};

export type TourCatalogFactView = {
  code: string;
  detail: string | null;
};

export type ExperienceStopView = {
  sortOrder: number;
  destinationId: string | null;
  placeId: string | null;
};

export type ExperienceItineraryDayView = {
  dayNumber: number;
  stops: ExperienceStopView[];
  meals: string[];
};

export type ExperienceFactView = {
  code: string;
  value: string | null;
  detail: string | null;
};

export type ExperienceEquipmentView = {
  code: string;
  kind: string;
  detail: string | null;
};

export type ExperienceGuideView = {
  guidePartyId: string;
  role: string;
  note: string | null;
};

export type ExperienceStayView = {
  sortOrder: number;
  placeId: string | null;
};

/** Kind-specific Experience slice — not a union with Package fields (P14-R4). */
export type ExperiencePresentationView = {
  difficulty: string | null;
  itineraryDays: ExperienceItineraryDayView[];
  eligibility: ExperienceFactView[];
  equipment: ExperienceEquipmentView[];
  localTransport: ExperienceFactView[];
  guides: ExperienceGuideView[];
  accommodationPlan: ExperienceStayView[];
};

export type TourDetailPageViewModel = {
  locale: AppLocale;
  tourProductId: string;
  kind: string;
  code: string;
  name: string;
  description: string | null;
  slug: string;
  englishName: string;
  catalogStatus: string;
  cover: TourMediaItemView | null;
  gallery: TourMediaItemView[];
  publishedDepartures: PublishedDepartureView[];
  destinationIds: string[];
  originDestinationId: string | null;
  policies: TourCatalogFactView[];
  requirements: TourCatalogFactView[];
  experience: ExperiencePresentationView | null;
  relatedTours: RelatedTourView[];
  relatedContent: RelatedContentView[];
  agencyOffers: AgencyOfferView[];
  ugcComposition: UgcCompositionView;
};

type ApiSlugHit = {
  tourProductId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
};

type ApiTourProduct = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
  localizedTitle?: string | null;
  localizedDescription?: string | null;
  localizedSlug?: string | null;
};

type ApiMediaPresentation = {
  mediaAssetId: string;
  role: string;
  sortOrder: number;
  presentation?: {
    mediaAssetId: string;
    status: string;
    originalContentUrl?: string | null;
    width?: number | null;
    height?: number | null;
    variants?: Array<{
      profile: string;
      status: string;
      contentUrl?: string | null;
      width?: number | null;
      height?: number | null;
    }> | null;
    altCaption?: { altText?: string | null } | null;
  } | null;
};

type ApiTourMedia = {
  tourProductId: string;
  cover?: ApiMediaPresentation | null;
  gallery?: ApiMediaPresentation[] | null;
};

type ApiPublishedDeparture = {
  id: string;
  tourProductId: string;
  status: string;
  startDate?: string | null;
  endDate?: string | null;
  timeZoneId?: string | null;
  durationDays?: number | null;
  capacity?: { minimumPax?: number | null; maximumPax?: number | null } | null;
  transport?: Array<{
    sequence: number;
    transportMode: string;
    origin: string;
    destination: string;
  }> | null;
  accommodation?: Array<{
    placeId: string;
    nights: number;
    boardType: string;
  }> | null;
};

type ApiCatalogFacts = {
  id: string;
  code: string;
  services?: Array<{ code: string; detail?: string | null }> | null;
  policies?: Array<{ code: string; detail?: string | null }> | null;
  requirements?: Array<{ code: string; detail?: string | null }> | null;
};

type ApiSemanticLinks = {
  id: string;
  destinationIds?: string[] | null;
  originDestinationId?: string | null;
  agencyId?: string | null;
};

type ApiExperiencePresentation = {
  tourProductId: string;
  difficulty?: string | null;
  itineraryDays?: Array<{
    dayNumber: number;
    stops?: Array<{
      sortOrder: number;
      destinationId?: string | null;
      placeId?: string | null;
    }> | null;
    meals?: string[] | null;
  }> | null;
  eligibility?: Array<{
    code: string;
    value?: string | null;
    detail?: string | null;
  }> | null;
  equipment?: Array<{
    code: string;
    kind: string;
    detail?: string | null;
  }> | null;
  localTransport?: Array<{
    code: string;
    value?: string | null;
    detail?: string | null;
  }> | null;
  guides?: Array<{
    guidePartyId: string;
    role: string;
    note?: string | null;
  }> | null;
  accommodationPlan?: Array<{
    sortOrder: number;
    placeId?: string | null;
  }> | null;
};

type ApiPublicMoney = {
  amount: number;
  currencyCode: string;
};

type ApiPublicPriceSummary = {
  priceId: string;
  targetType: string;
  targetId: string;
  currency: string;
  components?: Array<{ kind: string; money: ApiPublicMoney }> | null;
  occupancyPrices?: Array<{
    passengerCategory: string;
    occupancyCategory: string;
    money: ApiPublicMoney;
  }> | null;
};

function mapMediaItem(item: ApiMediaPresentation): TourMediaItemView {
  const p = item.presentation;
  const ready = p?.status === "Ready";
  const medium = p?.variants?.find(
    (v) => v.profile.toLowerCase() === "medium" && v.status === "Ready",
  );
  const src =
    (medium?.contentUrl
      ? resolveMediaAppProxySrc(
          medium.contentUrl.startsWith("/")
            ? medium.contentUrl
            : `/${medium.contentUrl}`,
        )
      : null) ??
    (ready && p?.originalContentUrl
      ? resolveMediaAppProxySrc(
          p.originalContentUrl.startsWith("/")
            ? p.originalContentUrl
            : `/${p.originalContentUrl}`,
        )
      : null) ??
    (ready
      ? resolveMediaAppProxySrc(mediaOriginalContentPath(item.mediaAssetId))
      : null);

  return {
    mediaAssetId: item.mediaAssetId,
    role: item.role,
    sortOrder: item.sortOrder,
    src,
    alt: p?.altCaption?.altText?.trim() || "",
    width: medium?.width ?? p?.width ?? null,
    height: medium?.height ?? p?.height ?? null,
  };
}

/**
 * Loads public TourProduct detail for locale + Tour-owned translation slug (P09-R5).
 * Draft/Inactive → 404 (by-slug publicOnly). Missing localized title → 404 (ADR 0008).
 * Catalog Published ≠ SEO Index (P09-R6); IndexPolicy remains SEO-owned.
 * Cover/Gallery via Tour media/presentation compose (Media.Contracts; app-proxy only).
 * Published execution summaries via departures/published (P11-R8); Published ≠ bookable.
 * Optional public price summary via /api/pricing/public (P12-R8); missing summary does not hide the tour.
 */
export async function loadTourDetailPage(
  locale: AppLocale,
  slug: string,
): Promise<ApiResult<TourDetailPageViewModel>> {
  const localeEnc = encodeURIComponent(locale);
  const slugEnc = encodeURIComponent(slug.trim());

  const hitResult = await apiGetJson<ApiSlugHit>(
    `/api/tour/products/by-slug/${localeEnc}/${slugEnc}`,
    { cache: "no-store" },
  );
  if (!isApiOk(hitResult)) {
    return hitResult;
  }

  const id = hitResult.data.tourProductId;
  const [productResult, mediaResult, departuresResult, linksResult, factsResult] =
    await Promise.all([
    apiGetJson<ApiTourProduct>(`/api/tour/products/${id}?locale=${localeEnc}`, {
      cache: "no-store",
    }),
    apiGetJson<ApiTourMedia>(
      `/api/tour/products/${id}/media/presentation?locale=${localeEnc}`,
      { cache: "no-store" },
    ),
    apiGetJson<ApiPublishedDeparture[]>(
      `/api/tour/products/${id}/departures/published`,
      { cache: "no-store" },
    ),
    apiGetJson<ApiSemanticLinks>(`/api/tour/products/${id}/semantic-links`, {
      cache: "no-store",
    }),
    apiGetJson<ApiCatalogFacts>(`/api/tour/products/${id}/catalog-facts`, {
      cache: "no-store",
    }),
  ]);

  if (!isApiOk(productResult)) {
    return productResult;
  }
  if (!isApiOk(mediaResult) && mediaResult.status !== 404) {
    return mediaResult;
  }
  if (!isApiOk(departuresResult) && departuresResult.status !== 404) {
    return departuresResult;
  }
  if (!isApiOk(linksResult) && linksResult.status !== 404) {
    return linksResult;
  }
  if (!isApiOk(factsResult) && factsResult.status !== 404) {
    return factsResult;
  }

  const product = productResult.data;
  if (product.catalogStatus !== "Published") {
    return apiFail({
      kind: "http",
      status: 404,
      message: "TourProduct is not publicly available.",
    });
  }

  const localizedTitle = product.localizedTitle?.trim() || null;
  if (!localizedTitle) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Localized TourProduct representation is missing.",
    });
  }

  const media = isApiOk(mediaResult) ? mediaResult.data : null;
  const cover = media?.cover ? mapMediaItem(media.cover) : null;
  const gallery = (media?.gallery ?? []).map(mapMediaItem);
  const publishedBase = (isApiOk(departuresResult) ? departuresResult.data : [])
    .filter((d) => d.status === "Published")
    .map((d) => ({
      id: d.id,
      status: d.status,
      startDate: d.startDate ?? null,
      endDate: d.endDate ?? null,
      timeZoneId: d.timeZoneId ?? null,
      durationDays: d.durationDays ?? null,
      minimumPax: d.capacity?.minimumPax ?? null,
      maximumPax: d.capacity?.maximumPax ?? null,
      transport: (d.transport ?? []).map((t) => ({
        sequence: t.sequence,
        transportMode: t.transportMode,
        origin: t.origin,
        destination: t.destination,
      })),
      accommodation: (d.accommodation ?? []).map((a) => ({
        placeId: a.placeId,
        nights: a.nights,
        boardType: a.boardType,
      })),
    }));

  const priceResults = await Promise.all(
    publishedBase.map((d) => loadPublicPriceSummary(d.id)),
  );
  const publishedDepartures: PublishedDepartureView[] = publishedBase.map(
    (d, index) => ({
      ...d,
      priceSummary: priceResults[index] ?? null,
    }),
  );

  const links = isApiOk(linksResult) ? linksResult.data : null;
  const facts = isApiOk(factsResult) ? factsResult.data : null;
  let experience: ExperiencePresentationView | null = null;
  if (product.kind === "Experience") {
    experience = await loadExperiencePresentation(id);
  }
  const relatedTours = await loadRelatedToursByProduct(id, locale);
  const relatedContent = await loadRelatedContentByDestinations(
    links?.destinationIds ?? [],
    locale,
  );
  const agencyOffers = await loadAgencyOffersByTourProduct(id);
  const ugcComposition = await loadUgcComposition({
    targetType: "TourProduct",
    targetId: id,
    locale,
  });

  return {
    ok: true,
    status: 200,
    data: asPageViewModel({
      locale,
      tourProductId: id,
      kind: product.kind,
      code: product.code,
      name: localizedTitle,
      description: product.localizedDescription?.trim() || null,
      slug: hitResult.data.slug,
      englishName: product.englishName,
      catalogStatus: product.catalogStatus,
      cover,
      gallery,
      publishedDepartures,
      destinationIds: links?.destinationIds ?? [],
      originDestinationId: links?.originDestinationId ?? null,
      policies: (facts?.policies ?? []).map((item) => ({
        code: item.code,
        detail: item.detail ?? null,
      })),
      requirements: (facts?.requirements ?? []).map((item) => ({
        code: item.code,
        detail: item.detail ?? null,
      })),
      experience,
      relatedTours,
      relatedContent,
      agencyOffers,
      ugcComposition,
    }),
  };
}

function mapPublicMoney(money: ApiPublicMoney): PublicMoneyView {
  return {
    amount: money.amount,
    currencyCode: money.currencyCode,
  };
}

/**
 * Optional public price facts (P12-R8). 404 / transport errors omit the summary
 * so the catalog page still renders.
 */
async function loadPublicPriceSummary(
  tourDepartureId: string,
): Promise<PublicPriceSummaryView | null> {
  const result = await apiGetJson<ApiPublicPriceSummary>(
    `/api/pricing/public/tour-departures/${tourDepartureId}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) {
    return null;
  }

  const data = result.data;
  return {
    priceId: data.priceId,
    targetType: data.targetType,
    targetId: data.targetId,
    currency: data.currency,
    components: (data.components ?? []).map((c) => ({
      kind: c.kind,
      money: mapPublicMoney(c.money),
    })),
    occupancyPrices: (data.occupancyPrices ?? []).map((row) => ({
      passengerCategory: row.passengerCategory,
      occupancyCategory: row.occupancyCategory,
      money: mapPublicMoney(row.money),
    })),
  };
}

async function loadExperiencePresentation(
  tourProductId: string,
): Promise<ExperiencePresentationView | null> {
  const result = await apiGetJson<ApiExperiencePresentation>(
    `/api/tour/products/${tourProductId}/experience/presentation`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) {
    return null;
  }

  const data = result.data;
  return {
    difficulty: data.difficulty ?? null,
    itineraryDays: (data.itineraryDays ?? []).map((day) => ({
      dayNumber: day.dayNumber,
      stops: (day.stops ?? []).map((stop) => ({
        sortOrder: stop.sortOrder,
        destinationId: stop.destinationId ?? null,
        placeId: stop.placeId ?? null,
      })),
      meals: day.meals ?? [],
    })),
    eligibility: (data.eligibility ?? []).map((item) => ({
      code: item.code,
      value: item.value ?? null,
      detail: item.detail ?? null,
    })),
    equipment: (data.equipment ?? []).map((item) => ({
      code: item.code,
      kind: item.kind,
      detail: item.detail ?? null,
    })),
    localTransport: (data.localTransport ?? []).map((item) => ({
      code: item.code,
      value: item.value ?? null,
      detail: item.detail ?? null,
    })),
    guides: (data.guides ?? []).map((item) => ({
      guidePartyId: item.guidePartyId,
      role: item.role,
      note: item.note ?? null,
    })),
    accommodationPlan: (data.accommodationPlan ?? []).map((item) => ({
      sortOrder: item.sortOrder,
      placeId: item.placeId ?? null,
    })),
  };
}
