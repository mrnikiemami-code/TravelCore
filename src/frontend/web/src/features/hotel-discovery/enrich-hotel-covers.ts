import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type { AppLocale } from "@/lib/i18n";
import {
  mediaOriginalContentPath,
  resolveMediaAppProxySrc,
} from "@/lib/media/media-presentation";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";

type ApiMediaPresentation = {
  mediaAssetId: string;
  presentation?: {
    status: string;
    originalContentUrl?: string | null;
    variants?: Array<{
      profile: string;
      status: string;
      contentUrl?: string | null;
    }> | null;
  } | null;
};

type ApiPlaceMedia = {
  cover?: ApiMediaPresentation | null;
};

function resolveCoverSrc(cover: ApiMediaPresentation): string | null {
  const p = cover.presentation;
  if (!p || p.status !== "Ready") {
    return null;
  }
  const medium = p.variants?.find(
    (v) => v.profile.toLowerCase() === "medium" && v.status === "Ready",
  );
  if (medium?.contentUrl?.trim()) {
    const url = medium.contentUrl.trim();
    return resolveMediaAppProxySrc(url.startsWith("/") ? url : `/${url}`);
  }
  if (p.originalContentUrl?.trim()) {
    const url = p.originalContentUrl.trim();
    return resolveMediaAppProxySrc(url.startsWith("/") ? url : `/${url}`);
  }
  return resolveMediaAppProxySrc(mediaOriginalContentPath(cover.mediaAssetId));
}

/**
 * Frontend-only enrichment: attach Place cover URLs for listing cards.
 * Matches Place media/presentation compose shape (same pattern as Tour covers).
 * Does not invent media — omit coverSrc when presentation is empty/unavailable.
 */
export async function enrichHotelsWithCoverMedia(
  locale: AppLocale,
  hotels: HotelBrowseItemView[],
  limit = 12,
): Promise<HotelBrowseItemView[]> {
  const targets = hotels.slice(0, limit);
  const rest = hotels.slice(limit);

  const enriched = await Promise.all(
    targets.map(async (hotel) => {
      const media = await apiGetJson<ApiPlaceMedia>(
        `/api/place/places/${encodeURIComponent(hotel.placeId)}/media/presentation?locale=${encodeURIComponent(locale)}`,
        { cache: "no-store" },
      );
      if (!isApiOk(media) || !media.data.cover) {
        return { ...hotel, coverSrc: null };
      }
      return {
        ...hotel,
        coverSrc: resolveCoverSrc(media.data.cover),
      };
    }),
  );

  return [...enriched, ...rest.map((h) => ({ ...h, coverSrc: h.coverSrc ?? null }))];
}
