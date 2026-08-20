import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type { AppLocale } from "@/lib/i18n";

export type HotelBrowseItemView = {
  placeId: string;
  localeCode: string;
  slug: string;
  name: string;
  description: string | null;
  starRating: number | null;
};

type ApiHotelBrowseItem = {
  placeId: string;
  localeCode: string;
  slug: string;
  name: string;
  description?: string | null;
  starRating?: number | null;
};

function mapItem(item: ApiHotelBrowseItem): HotelBrowseItemView {
  return {
    placeId: item.placeId,
    localeCode: item.localeCode,
    slug: item.slug,
    name: item.name,
    description: item.description ?? null,
    starRating: item.starRating ?? null,
  };
}

export type HotelDiscoveryLoadResult =
  | { ok: true; hotels: HotelBrowseItemView[]; error: null }
  | { ok: false; hotels: []; error: string };

/** Active hotels with locale slug for discovery index (TC-HOTIDX-T007 / P30-T006). */
export async function loadHotelDiscoveryList(
  locale: AppLocale,
): Promise<HotelDiscoveryLoadResult> {
  const result = await apiGetJson<ApiHotelBrowseItem[]>(
    `/api/place/public/hotels?localeCode=${encodeURIComponent(locale)}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) {
    return {
      ok: false,
      hotels: [],
      error: "hotel_discovery_load_failed",
    };
  }

  return {
    ok: true,
    hotels: (Array.isArray(result.data) ? result.data : []).map(mapItem),
    error: null,
  };
}
