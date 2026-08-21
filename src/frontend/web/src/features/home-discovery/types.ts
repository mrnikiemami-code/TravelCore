import type { AppLocale } from "@/lib/i18n";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import type { UgcTravelogueView } from "@/features/public-experience/load-ugc-composition";

/** Max preview cards per live catalog section on home (P31 commercial density). */
export const HOME_DISCOVERY_PREVIEW_LIMIT = 6;

/** Curated DEMOFEED / known public destination slugs for Home composition (no invented catalog). */
export const HOME_DESTINATION_SLUG_CANDIDATES = [
  "demofeed-istanbul",
  "demofeed-tehran",
  "demofeed-turkey",
  "demofeed-iran",
] as const;

export type HomeDestinationPreview = {
  destinationId: string;
  slug: string;
  name: string;
  description: string | null;
};

export type HomeDiscoveryComposition = {
  locale: AppLocale;
  travelogues: UgcTravelogueView[];
  hotels: HotelBrowseItemView[];
  destinations: HomeDestinationPreview[];
  tours: RelatedTourView[];
};
