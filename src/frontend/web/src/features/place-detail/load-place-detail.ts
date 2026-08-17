import { apiGetJson } from "@/lib/api/client";
import { asPageViewModel } from "@/lib/api/read-models";
import { apiFail, isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";
import {
  mediaOriginalContentPath,
  resolveMediaAppProxySrc,
} from "@/lib/media/media-presentation";
import { loadUgcComposition } from "@/features/public-experience/load-ugc-composition";
import type {
  PlaceDetailPageViewModel,
  PlaceMediaItemView,
} from "@/types/pages/place-detail";

type ApiSlugHit = {
  placeId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
};

type ApiPlace = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
  classificationCode?: string | null;
  facilities?: string[] | null;
  destinationId?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  address?: {
    line1?: string | null;
    line2?: string | null;
    locality?: string | null;
    administrativeArea?: string | null;
    postalCode?: string | null;
    countryCode?: string | null;
  } | null;
  hotel?: { starRating?: number | null } | null;
  restaurant?: { cuisineType?: string | null } | null;
  attraction?: { categoryCode?: string | null } | null;
  localizedName?: string | null;
  localizedDescription?: string | null;
  locale?: string | null;
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

type ApiPlaceMedia = {
  placeId: string;
  cover?: ApiMediaPresentation | null;
  gallery?: ApiMediaPresentation[] | null;
};

type ApiDestination = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  localizedName?: string | null;
};

type ApiDestinationTranslation = {
  destinationId: string;
  localeCode: string;
  name: string;
  slug?: string | null;
};

function mapMediaItem(item: ApiMediaPresentation): PlaceMediaItemView {
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

function formatAddress(place: ApiPlace): string | null {
  const a = place.address;
  if (!a) return null;
  const parts = [
    a.line1,
    a.line2,
    a.locality,
    a.administrativeArea,
    a.postalCode,
    a.countryCode,
  ]
    .map((x) => x?.trim())
    .filter(Boolean);
  return parts.length ? parts.join(", ") : null;
}

/**
 * Loads public Place detail for locale + Place-owned slug.
 * Draft/Inactive → 404 (by-slug publicOnly). Missing localized name → 404 (ADR 0008).
 */
export async function loadPlaceDetailPage(
  locale: AppLocale,
  slug: string,
): Promise<ApiResult<PlaceDetailPageViewModel>> {
  const localeEnc = encodeURIComponent(locale);
  const slugEnc = encodeURIComponent(slug.trim());

  const hitResult = await apiGetJson<ApiSlugHit>(
    `/api/place/places/by-slug/${localeEnc}/${slugEnc}`,
    { cache: "no-store" },
  );
  if (!isApiOk(hitResult)) {
    return hitResult;
  }

  const id = hitResult.data.placeId;
  const [placeResult, mediaResult] = await Promise.all([
    apiGetJson<ApiPlace>(`/api/place/places/${id}?locale=${localeEnc}`, {
      cache: "no-store",
    }),
    apiGetJson<ApiPlaceMedia>(
      `/api/place/places/${id}/media/presentation?locale=${localeEnc}`,
      { cache: "no-store" },
    ),
  ]);

  if (!isApiOk(placeResult)) return placeResult;
  if (!isApiOk(mediaResult) && mediaResult.status !== 404) {
    return mediaResult;
  }

  const place = placeResult.data;
  if (place.catalogStatus !== "Active") {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Place is not publicly available.",
    });
  }

  const localizedName = place.localizedName?.trim() || null;
  if (!localizedName) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Localized Place representation is missing.",
    });
  }

  let destination: PlaceDetailPageViewModel["destination"] = null;
  if (place.destinationId) {
    const destId = encodeURIComponent(place.destinationId);
    const [destResult, destTranslations] = await Promise.all([
      apiGetJson<ApiDestination>(
        `/api/destination/destinations/${destId}?locale=${localeEnc}`,
        { cache: "no-store" },
      ),
      apiGetJson<ApiDestinationTranslation[]>(
        `/api/destination/destinations/${destId}/translations`,
        { cache: "no-store" },
      ),
    ]);
    if (isApiOk(destResult)) {
      const tr = isApiOk(destTranslations)
        ? (destTranslations.data ?? []).find(
            (t) => t.localeCode.toLowerCase() === locale.toLowerCase(),
          )
        : null;
      destination = {
        id: destResult.data.id,
        name:
          destResult.data.localizedName?.trim() ||
          tr?.name ||
          destResult.data.englishName,
        slug: tr?.slug?.trim() || null,
        kind: destResult.data.kind,
        code: destResult.data.code,
      };
    }
  }

  const media = isApiOk(mediaResult) ? mediaResult.data : null;
  const cover = media?.cover ? mapMediaItem(media.cover) : null;
  const gallery = (media?.gallery ?? []).map(mapMediaItem);
  const ugcComposition = await loadUgcComposition({
    targetType: "Place",
    targetId: place.id,
    locale,
  });

  return {
    ok: true,
    status: 200,
    data: asPageViewModel({
      locale,
      placeId: place.id,
      kind: place.kind,
      code: place.code,
      name: localizedName,
      description: place.localizedDescription?.trim() || null,
      slug: hitResult.data.slug,
      englishName: place.englishName,
      catalogStatus: place.catalogStatus,
      classificationCode: place.classificationCode ?? null,
      facilities: place.facilities ?? [],
      latitude: place.latitude ?? null,
      longitude: place.longitude ?? null,
      addressLine: formatAddress(place),
      destination,
      cover,
      gallery,
      hotelStarRating: place.hotel?.starRating ?? null,
      restaurantCuisineType: place.restaurant?.cuisineType ?? null,
      attractionCategoryCode: place.attraction?.categoryCode ?? null,
      ugcComposition,
    }),
  };
}
