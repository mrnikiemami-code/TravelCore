import type { AppLocale } from "@/lib/i18n";
import type { PageViewModel } from "@/lib/api/read-models";

/**
 * ContentDetailPage — public Content presentation (TC-P08-T008).
 * Domain Model ≠ API Contract ≠ Page View Model
 * Content owns current locale slug (P08-R3); SEO owns IndexPolicy / route history (P08-R4).
 */

export type ContentBlockView = {
  id: string;
  kind: string;
  sortOrder: number;
  text: string | null;
  headingLevel: number | null;
  href: string | null;
};

export type ContentDetailFields = {
  locale: AppLocale;
  kind: string;
  code: string;
  title: string;
  body: string | null;
  excerpt: string | null;
  slug: string;
  englishName: string;
  publicPath: string;
  blocks: ContentBlockView[];
  destinationIds: string[];
};

export type ContentDetailPageViewModel = PageViewModel<ContentDetailFields>;
