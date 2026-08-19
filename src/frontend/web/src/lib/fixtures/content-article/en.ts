import { asPageViewModel } from "@/lib/api/read-models";
import type { ContentDetailPageViewModel } from "@/types/pages/content-detail";

export const contentArticleEnFixture: ContentDetailPageViewModel = asPageViewModel({
  locale: "en",
  kind: "Article",
  code: "ART-IST-GUIDE-01",
  title: "Istanbul Travel Guide — Practical Tips",
  excerpt: "UIVAL sample Article — informational guidance, not a commerce landing.",
  body: "Short article body. Structured blocks follow below.",
  slug: "fixture-istanbul-guide",
  englishName: "Istanbul Travel Guide",
  publicPath: "articles/fixture-istanbul-guide",
  destinationIds: ["dest-ist"],
  blocks: [
    {
      id: "b1",
      kind: "Heading",
      sortOrder: 1,
      text: "Best time to visit",
      headingLevel: 2,
      href: null,
    },
    {
      id: "b2",
      kind: "Paragraph",
      sortOrder: 2,
      text: "Spring and autumn are comfortable for city exploration.",
      headingLevel: null,
      href: null,
    },
    {
      id: "b3",
      kind: "Cta",
      sortOrder: 3,
      text: "Browse Istanbul tours",
      headingLevel: null,
      href: "/en/tours?destination=istanbul",
    },
  ],
});
