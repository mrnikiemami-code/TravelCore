import type { AppLocale } from "@/lib/i18n";
import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import { loadHotelDiscoveryList } from "@/features/hotel-discovery/load-hotel-discovery-list";
import {
  HOME_DESTINATION_SLUG_CANDIDATES,
  HOME_DISCOVERY_PREVIEW_LIMIT,
  type HomeDestinationPreview,
  type HomeDiscoveryComposition,
} from "@/features/home-discovery/types";
import { loadTourDiscoveryList } from "@/features/tour-discovery/load-tour-discovery-list";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import { loadTravelogueDiscoveryList } from "@/features/travelogue-detail/load-travelogue-list";

type DestinationBySlug = { destinationId: string };
type DestinationDetail = {
  destinationId: string;
  name?: string | null;
  description?: string | null;
};

async function loadDestinationPreview(
  locale: AppLocale,
  slug: string,
): Promise<HomeDestinationPreview | null> {
  const hit = await apiGetJson<DestinationBySlug>(
    `/api/destination/destinations/by-slug/${encodeURIComponent(locale)}/${encodeURIComponent(slug)}`,
    { cache: "no-store" },
  );
  if (!isApiOk(hit)) {
    return null;
  }

  const detail = await apiGetJson<DestinationDetail>(
    `/api/destination/destinations/${encodeURIComponent(hit.data.destinationId)}?locale=${encodeURIComponent(locale)}`,
    { cache: "no-store" },
  );

  const name =
    (isApiOk(detail) ? detail.data.name : null)?.trim() ||
    slug.replace(/^demofeed-/, "").replace(/-/g, " ");

  return {
    destinationId: hit.data.destinationId,
    slug,
    name,
    description: isApiOk(detail) ? (detail.data.description ?? null) : null,
  };
}

/**
 * Curated locale-scoped home composition (TC-HOMFEED-T002 · TC-P31-T003).
 * Deterministic public loaders only — not user-personalized · not Search.
 * Composes live DEMOFEED-backed destinations/hotels/tours when public APIs return them.
 */
export async function loadHomeDiscoveryComposition(
  locale: AppLocale,
): Promise<HomeDiscoveryComposition> {
  const [travelogues, hotelLoad, destinationResults, tourBatches] =
    await Promise.all([
      loadTravelogueDiscoveryList(locale),
      loadHotelDiscoveryList(locale),
      Promise.all(
        HOME_DESTINATION_SLUG_CANDIDATES.map((slug) =>
          loadDestinationPreview(locale, slug),
        ),
      ),
      Promise.all(
        HOME_DESTINATION_SLUG_CANDIDATES.slice(0, 2).map((slug) =>
          loadTourDiscoveryList(locale, slug),
        ),
      ),
    ]);

  const destinations = destinationResults.filter(
    (item): item is HomeDestinationPreview => item != null,
  );

  const tourMap = new Map<string, RelatedTourView>();
  for (const batch of tourBatches) {
    if (!batch.ok || batch.mode !== "ready") continue;
    for (const tour of batch.tours) {
      if (!tourMap.has(tour.tourProductId)) {
        tourMap.set(tour.tourProductId, tour);
      }
    }
  }

  return {
    locale,
    travelogues: travelogues.slice(0, HOME_DISCOVERY_PREVIEW_LIMIT),
    hotels: hotelLoad.hotels.slice(0, HOME_DISCOVERY_PREVIEW_LIMIT),
    destinations: destinations.slice(0, HOME_DISCOVERY_PREVIEW_LIMIT),
    tours: [...tourMap.values()].slice(0, HOME_DISCOVERY_PREVIEW_LIMIT),
  };
}
