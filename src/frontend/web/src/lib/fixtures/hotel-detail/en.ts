import type { UgcCompositionView } from "@/features/public-experience/load-ugc-composition";
import { asPageViewModel } from "@/lib/api/read-models";
import type { PlaceDetailPageViewModel } from "@/types/pages/place-detail";

const emptyUgc: UgcCompositionView = {
  summary: { eligibleReviewCount: 0, averageOverallRating: 0 },
  reviews: [],
  travelogues: [],
  userPhotos: [],
};

export const hotelDetailEnFixture: PlaceDetailPageViewModel = asPageViewModel({
  locale: "en",
  placeId: "fixture-hotel-01",
  kind: "Hotel",
  code: "HTL-IST-SAMPLE",
  name: "Sample Istanbul Hotel",
  description:
    "Hotel catalog Place detail for UIVAL — not a HotelBooking engine surface.",
  slug: "fixture-istanbul-hotel",
  englishName: "Sample Istanbul Hotel",
  catalogStatus: "Published",
  classificationCode: "HTL-4STAR",
  facilities: ["WiFi", "Breakfast", "Parking"],
  latitude: 41.0123,
  longitude: 28.9784,
  addressLine: "Sample Street, Istanbul",
  destination: {
    id: "dest-ist",
    name: "Istanbul",
    slug: "fixture-istanbul",
    kind: "City",
    code: "DEST-IST",
  },
  cover: {
    mediaAssetId: "fixture-hotel-cover",
    role: "Cover",
    sortOrder: 0,
    src: "/media/foundation-sample.png",
    alt: "Sample hotel imagery",
    width: 960,
    height: 540,
  },
  gallery: [],
  hotelStarRating: 4,
  restaurantCuisineType: null,
  attractionCategoryCode: null,
  ugcComposition: emptyUgc,
});
