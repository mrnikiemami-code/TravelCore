export type ContentKindView = "Article" | "LandingPage" | "Guide" | string;

export type ContentItemSummaryView = {
  id: string;
  kind: ContentKindView;
  code: string;
  englishName: string;
  localizedTitle: string | null;
  localizedBody: string | null;
  localizedExcerpt: string | null;
  categoryIds: string[];
  tagIds: string[];
  destinationIds: string[];
  createdAt: string;
  updatedAt: string;
};

export type ContentTranslationView = {
  contentItemId: string;
  localeCode: string;
  title: string;
  body: string | null;
  excerpt: string | null;
  updatedAt: string;
};

export type ContentCategoryView = {
  id: string;
  code: string;
  englishName: string;
  createdAt: string;
  updatedAt: string;
};

export type ContentTagView = {
  id: string;
  code: string;
  englishName: string;
  createdAt: string;
  updatedAt: string;
};

export type ContentBlockGalleryItemView = {
  mediaAssetId: string;
  sortOrder: number;
};

export type ContentBlockFaqItemView = {
  question: string;
  answer: string;
  sortOrder: number;
};

export type ContentBlockView = {
  id: string;
  contentItemId: string;
  kind: string;
  sortOrder: number;
  text: string | null;
  headingLevel: number | null;
  mediaAssetId: string | null;
  href: string | null;
  galleryItems: ContentBlockGalleryItemView[];
  faqItems: ContentBlockFaqItemView[];
};

export type ContentDetailView = {
  item: ContentItemSummaryView;
  translations: ContentTranslationView[];
  blocks: ContentBlockView[];
};
