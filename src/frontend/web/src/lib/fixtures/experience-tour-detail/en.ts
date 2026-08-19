import { asPageViewModel } from "@/lib/api/read-models";
import type { TourDetailPageViewModel } from "@/features/tour-detail/load-tour-detail";

import type { UgcCompositionView } from "@/features/public-experience/load-ugc-composition";

const emptyUgc: UgcCompositionView = {
  summary: { eligibleReviewCount: 0, averageOverallRating: 0 },
  reviews: [],
  travelogues: [],
  userPhotos: [],
};

/**
 * Deterministic EN Experience Tour fixture (UIVAL-T003).
 * Distinct copy from FA — not silent reuse.
 */
export const experienceTourDetailEnFixture: TourDetailPageViewModel =
  asPageViewModel({
    locale: "en",
    tourProductId: "fixture-exp-daryache-en",
    kind: "Experience",
    code: "EXP-DARYACHE-01",
    name: "Daryache to Urmia Nature Experience",
    description:
      "Structured experience with day-by-day itinerary, moderate difficulty, and equipment guidance. UIVAL fixture only.",
    slug: "fixture-daryache-experience",
    englishName: "Daryache to Urmia Nature Experience",
    catalogStatus: "Published",
    cover: {
      mediaAssetId: "fixture-cover",
      role: "Cover",
      sortOrder: 0,
      src: "/media/foundation-sample.png",
      alt: "Sample nature trail imagery for experience tour validation",
      width: 960,
      height: 540,
    },
    gallery: [],
    publishedDepartures: [
      {
        id: "dep-fixture-2026-10-01",
        status: "Published",
        startDate: "2026-10-01",
        endDate: "2026-10-04",
        timeZoneId: "Asia/Tehran",
        durationDays: 4,
        minimumPax: 6,
        maximumPax: 14,
        transport: [
          {
            sequence: 1,
            transportMode: "Van",
            origin: "Tehran",
            destination: "Daryache",
          },
        ],
        accommodation: [],
        priceSummary: null,
      },
    ],
    destinationIds: ["dest-daryache", "dest-urmia"],
    originDestinationId: "dest-tehran",
    policies: [{ code: "Cancellation48h", detail: "Until 48 hours before" }],
    requirements: [{ code: "Passport", detail: "Valid national ID" }],
    experience: {
      difficulty: "Moderate",
      itineraryDays: [
        {
          dayNumber: 1,
          stops: [
            { sortOrder: 1, destinationId: "dest-daryache", placeId: null },
            { sortOrder: 2, destinationId: null, placeId: "place-viewpoint-a" },
          ],
          meals: ["Breakfast", "Lunch"],
        },
        {
          dayNumber: 2,
          stops: [
            { sortOrder: 1, destinationId: null, placeId: "place-trail-b" },
            { sortOrder: 2, destinationId: "dest-urmia", placeId: null },
          ],
          meals: ["Breakfast", "Dinner"],
        },
        {
          dayNumber: 3,
          stops: [
            { sortOrder: 1, destinationId: null, placeId: "place-lake-shore" },
          ],
          meals: ["Lunch"],
        },
      ],
      eligibility: [
        { code: "MinAge", value: "12", detail: "Adult companion required" },
        { code: "Fitness", value: "Moderate", detail: "4–6 hours hiking" },
      ],
      equipment: [
        { code: "Boots", kind: "Required", detail: "Hiking boots" },
        { code: "RainJacket", kind: "Recommended", detail: "Waterproof layer" },
      ],
      localTransport: [
        { code: "Van", value: "Shared", detail: "Group van between stops" },
      ],
      guides: [
        {
          guidePartyId: "party-guide-884",
          role: "Lead",
          note: "Licensed local guide",
        },
      ],
      accommodationPlan: [
        { sortOrder: 1, placeId: "place-eco-lodge" },
        { sortOrder: 2, placeId: "place-guesthouse-b" },
      ],
    },
    relatedTours: [],
    relatedContent: [],
    agencyOffers: [],
    ugcComposition: emptyUgc,
  });
