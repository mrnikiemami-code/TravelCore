import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type { AppLocale } from "@/lib/i18n";
import {
  loadRelatedToursByDestination,
  type RelatedTourView,
} from "@/features/public-experience/load-related-tours";

export type TourDiscoveryLoadResult =
  | {
      ok: true;
      mode: "needs-destination" | "ready";
      tours: RelatedTourView[];
      error: null;
    }
  | {
      ok: false;
      mode: "error";
      tours: [];
      error: string;
    };

/**
 * Destination-scoped tour discovery (existing related-published contract).
 * No full public browse index exists — do not invent a catalog.
 */
export async function loadTourDiscoveryList(
  locale: AppLocale,
  destinationSlug: string,
): Promise<TourDiscoveryLoadResult> {
  const dest = destinationSlug.trim();
  if (!dest) {
    return { ok: true, mode: "needs-destination", tours: [], error: null };
  }

  const destination = await apiGetJson<{ destinationId: string }>(
    `/api/destination/destinations/by-slug/${encodeURIComponent(locale)}/${encodeURIComponent(dest)}`,
    { cache: "no-store" },
  );

  if (!isApiOk(destination)) {
    return {
      ok: false,
      mode: "error",
      tours: [],
      error: "tour_discovery_destination_failed",
    };
  }

  try {
    const tours = await loadRelatedToursByDestination(
      destination.data.destinationId,
      locale,
    );
    return { ok: true, mode: "ready", tours, error: null };
  } catch {
    return {
      ok: false,
      mode: "error",
      tours: [],
      error: "tour_discovery_load_failed",
    };
  }
}
