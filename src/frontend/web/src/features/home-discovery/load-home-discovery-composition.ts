import type { AppLocale } from "@/lib/i18n";
import { loadHotelDiscoveryList } from "@/features/hotel-discovery/load-hotel-discovery-list";
import {
  HOME_DISCOVERY_PREVIEW_LIMIT,
  type HomeDiscoveryComposition,
} from "@/features/home-discovery/types";
import { loadTravelogueDiscoveryList } from "@/features/travelogue-detail/load-travelogue-list";

/**
 * Curated locale-scoped home composition (TC-HOMFEED-T002).
 * Deterministic public loaders only — not user-personalized · not Search.
 */
export async function loadHomeDiscoveryComposition(
  locale: AppLocale,
): Promise<HomeDiscoveryComposition> {
  const [travelogues, hotelLoad] = await Promise.all([
    loadTravelogueDiscoveryList(locale),
    loadHotelDiscoveryList(locale),
  ]);

  return {
    locale,
    travelogues: travelogues.slice(0, HOME_DISCOVERY_PREVIEW_LIMIT),
    hotels: hotelLoad.hotels.slice(0, HOME_DISCOVERY_PREVIEW_LIMIT),
  };
}
