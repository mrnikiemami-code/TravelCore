import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type { AppLocale } from "@/lib/i18n";
import { resolveMediaAppProxySrc } from "@/lib/media/media-presentation";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";

type ApiMediaPresentation = {
  mediaAssetId: string;
  originalContentPath?: string | null;
  preferredVariantContentPath?: string | null;
};

type ApiPlaceMedia = {
  cover?: ApiMediaPresentation | null;
};

/**
 * Frontend-only enrichment: attach Place cover URLs for listing cards.
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
      const cover = media.data.cover;
      const path =
        cover.preferredVariantContentPath?.trim() ||
        cover.originalContentPath?.trim() ||
        null;
      return {
        ...hotel,
        coverSrc: path ? resolveMediaAppProxySrc(path) : null,
      };
    }),
  );

  return [...enriched, ...rest.map((h) => ({ ...h, coverSrc: h.coverSrc ?? null }))];
}
