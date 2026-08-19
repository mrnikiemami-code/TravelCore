import { asPageViewModel } from "@/lib/api/read-models";
import type { ContentDetailPageViewModel } from "@/types/pages/content-detail";

export const contentArticleFaFixture: ContentDetailPageViewModel = asPageViewModel({
  locale: "fa",
  kind: "Article",
  code: "ART-IST-GUIDE-01",
  title: "راهنمای سفر به استانبول — نکات عملی",
  excerpt: "مقالهٔ نمونه UIVAL برای ArticlePage — راهنمای اطلاعاتی، نه لندینگ فروش.",
  body: "این متن بدنهٔ کوتاه مقاله است. بلوک‌های ساخت‌یافته در ادامه می‌آیند.",
  slug: "fixture-istanbul-guide",
  englishName: "Istanbul Travel Guide",
  publicPath: "articles/fixture-istanbul-guide",
  destinationIds: ["dest-ist"],
  blocks: [
    {
      id: "b1",
      kind: "Heading",
      sortOrder: 1,
      text: "زمان سفر",
      headingLevel: 2,
      href: null,
    },
    {
      id: "b2",
      kind: "Paragraph",
      sortOrder: 2,
      text: "بهار و پاییز برای گردش شهری مناسب‌ترند.",
      headingLevel: null,
      href: null,
    },
    {
      id: "b3",
      kind: "Cta",
      sortOrder: 3,
      text: "مشاهده تورهای استانبول",
      headingLevel: null,
      href: "/fa/tours?destination=istanbul",
    },
  ],
});
