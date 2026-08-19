import { asPageViewModel } from "@/lib/api/read-models";
import type { DestinationLandingPageViewModel } from "@/types/pages/destination-landing";

export const destinationLandingEnFixture: DestinationLandingPageViewModel =
  asPageViewModel({
    locale: "en",
    kind: "City",
    code: "DEST-IST",
    name: "Istanbul",
    description:
      "Sample destination landing for UIVAL — hierarchy and breadcrumb without live API.",
    slug: "fixture-istanbul",
    englishName: "Istanbul",
    isoCountryCode: "TR",
    latitude: 41.0082,
    longitude: 28.9784,
    breadcrumb: [
      {
        name: "Turkey",
        slug: "fixture-turkey",
        kind: "Country",
        code: "DEST-TR",
      },
      {
        name: "Istanbul",
        slug: "fixture-istanbul",
        kind: "City",
        code: "DEST-IST",
      },
    ],
    children: [
      {
        name: "Historic Peninsula",
        slug: "fixture-istanbul-historic",
        kind: "Area",
        code: "DEST-IST-HIST",
      },
      {
        name: "Asian Side",
        slug: null,
        kind: "Area",
        code: "DEST-IST-ASIA",
      },
    ],
  });
