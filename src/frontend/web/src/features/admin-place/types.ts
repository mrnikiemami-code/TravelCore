export type PlaceKindView = "Hotel" | "Restaurant" | "Attraction" | string;

export type PlaceCatalogStatusView = "Draft" | "Active" | "Inactive" | string;

export type PlaceAddressView = {
  line1: string | null;
  line2: string | null;
  locality: string | null;
  administrativeArea: string | null;
  postalCode: string | null;
  countryCode: string | null;
};

export type PlaceSummaryView = {
  id: string;
  kind: PlaceKindView;
  code: string;
  englishName: string;
  catalogStatus: PlaceCatalogStatusView;
  classificationCode: string | null;
  facilities: string[];
  destinationId: string | null;
  latitude: number | null;
  longitude: number | null;
  address: PlaceAddressView | null;
  hotelStarRating: number | null;
  restaurantCuisineType: string | null;
  attractionCategoryCode: string | null;
  localizedName: string | null;
  localizedDescription: string | null;
  locale: string | null;
  createdAt: string;
  updatedAt: string;
};

export type PlaceTranslationView = {
  placeId: string;
  localeCode: string;
  name: string;
  description: string | null;
};

export type PlaceMediaLinkView = {
  placeId: string;
  mediaAssetId: string;
  role: string;
  sortOrder: number;
};

export type PlaceDetailView = {
  place: PlaceSummaryView;
  translations: PlaceTranslationView[];
  mediaLinks: PlaceMediaLinkView[];
};
