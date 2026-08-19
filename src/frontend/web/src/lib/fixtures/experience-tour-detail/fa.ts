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
 * Deterministic FA Experience Tour fixture (UIVAL-T003).
 * Structured itinerary · difficulty · equipment — not Foreign Package layout.
 */
export const experienceTourDetailFaFixture: TourDetailPageViewModel =
  asPageViewModel({
    locale: "fa",
    tourProductId: "fixture-exp-daryache-fa",
    kind: "Experience",
    code: "EXP-DARYACHE-01",
    name: "تور دریاچه دالامپر تا ارومیه — تجربه طبیعت‌گردی",
    description:
      "تجربهٔ ساخت‌یافته با برنامه روزبه‌روز، سختی متوسط و تجهیزات پیشنهادی. دادهٔ نمایشی UIVAL.",
    slug: "fixture-daryache-experience",
    englishName: "Daryache to Urmia Nature Experience",
    catalogStatus: "Published",
    cover: {
      mediaAssetId: "fixture-cover",
      role: "Cover",
      sortOrder: 0,
      src: "/media/foundation-sample.png",
      alt: "نمای نمونه از مسیر طبیعت‌گردی دریاچه",
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
    policies: [{ code: "Cancellation48h", detail: "تا ۴۸ ساعت قبل" }],
    requirements: [{ code: "Passport", detail: "کارت ملی معتبر" }],
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
        { code: "MinAge", value: "12", detail: "همراهی بزرگسال الزامی" },
        { code: "Fitness", value: "Moderate", detail: "پیاده‌روی ۴–۶ ساعت" },
      ],
      equipment: [
        { code: "Boots", kind: "Required", detail: "کفش کوهپیمایی" },
        { code: "RainJacket", kind: "Recommended", detail: "ضد باران" },
      ],
      localTransport: [
        { code: "Van", value: "Shared", detail: "ون گروهی بین توقف‌ها" },
      ],
      guides: [
        {
          guidePartyId: "party-guide-884",
          role: "Lead",
          note: "راهنمای محلی مجوزدار",
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
