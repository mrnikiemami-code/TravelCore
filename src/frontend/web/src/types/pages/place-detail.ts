import type { AppLocale } from "@/lib/i18n";
import type { PageViewModel } from "@/lib/api/read-models";
import type { UgcCompositionView } from "@/features/public-experience/load-ugc-composition";

/**
 * PlaceDetailPage — public catalog presentation (TC-P07-T007).
 * Domain Model ≠ API Contract ≠ Page View Model
 * Place remains content SoR; SEO owns IndexPolicy / route history.
 */

export type PlaceMediaItemView = {
  mediaAssetId: string;
  role: string;
  sortOrder: number;
  src: string | null;
  alt: string;
  width: number | null;
  height: number | null;
};

export type PlaceDetailFields = {
  locale: AppLocale;
  placeId: string;
  kind: string;
  code: string;
  name: string;
  description: string | null;
  slug: string;
  englishName: string;
  catalogStatus: string;
  classificationCode: string | null;
  facilities: string[];
  latitude: number | null;
  longitude: number | null;
  addressLine: string | null;
  destination: {
    id: string;
    name: string;
    slug: string | null;
    kind: string;
    code: string;
  } | null;
  cover: PlaceMediaItemView | null;
  gallery: PlaceMediaItemView[];
  hotelStarRating: number | null;
  restaurantCuisineType: string | null;
  attractionCategoryCode: string | null;
  ugcComposition: UgcCompositionView;
};

export type PlaceDetailPageViewModel = PageViewModel<PlaceDetailFields>;
