import type { AppLocale } from "@/lib/i18n";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";
import type { UgcTravelogueView } from "@/features/public-experience/load-ugc-composition";

/** Max preview cards per section on home (TC-HOMFEED-T001). Not a recommendation engine cap. */
export const HOME_DISCOVERY_PREVIEW_LIMIT = 3;

export type HomeDiscoveryComposition = {
  locale: AppLocale;
  travelogues: UgcTravelogueView[];
  hotels: HotelBrowseItemView[];
};
