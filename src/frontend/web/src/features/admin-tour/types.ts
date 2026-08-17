export type TourProductSummaryView = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
  classificationCode: string | null;
  localizedTitle: string | null;
  localizedSlug: string | null;
  createdAt: string;
  updatedAt: string;
};

export type TourProductDetailView = TourProductSummaryView & {
  originDestinationId: string | null;
  agencyId: string | null;
  destinationIds: string[];
  localizedDescription: string | null;
};

export type TourTranslationView = {
  tourProductId: string;
  localeCode: string;
  title: string;
  description: string | null;
  slug: string | null;
  updatedAt: string;
};

export type TourMediaView = {
  id: string;
  code: string;
  coverMediaAssetId: string | null;
  galleryMediaAssetIds: string[];
};
