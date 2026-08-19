import { asPageViewModel } from "@/lib/api/read-models";
import type { DestinationLandingPageViewModel } from "@/types/pages/destination-landing";

export const destinationLandingFaFixture: DestinationLandingPageViewModel =
  asPageViewModel({
    locale: "fa",
    kind: "City",
    code: "DEST-IST",
    name: "استانبول",
    description:
      "صفحهٔ لندینگ مقصد نمونه برای UIVAL — سلسله‌مراتب و breadcrumb بدون API زنده.",
    slug: "fixture-istanbul",
    englishName: "Istanbul",
    isoCountryCode: "TR",
    latitude: 41.0082,
    longitude: 28.9784,
    breadcrumb: [
      {
        name: "ترکیه",
        slug: "fixture-turkey",
        kind: "Country",
        code: "DEST-TR",
      },
      {
        name: "استانبول",
        slug: "fixture-istanbul",
        kind: "City",
        code: "DEST-IST",
      },
    ],
    children: [
      {
        name: "منطقه تاریخی",
        slug: "fixture-istanbul-historic",
        kind: "Area",
        code: "DEST-IST-HIST",
      },
      {
        name: "سمت آسیایی",
        slug: null,
        kind: "Area",
        code: "DEST-IST-ASIA",
      },
    ],
  });
