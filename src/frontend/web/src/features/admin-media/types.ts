export type MediaAssetStatusView = "PendingStorage" | "Ready" | "Failed" | string;

export type MediaAssetSummaryView = {
  id: string;
  contentType: string;
  byteSize: number;
  width: number | null;
  height: number | null;
  focalX: number | null;
  focalY: number | null;
  status: MediaAssetStatusView;
  createdAt: string;
  updatedAt: string;
};

export type MediaVariantView = {
  id: string;
  mediaAssetId: string;
  profile: string;
  status: string;
  width: number | null;
  height: number | null;
  byteSize: number | null;
  contentType: string | null;
  failureReason: string | null;
};

export type MediaTranslationView = {
  mediaAssetId: string;
  localeCode: string;
  altText: string;
  caption: string | null;
  publicationStatus: string;
};

export type MediaAssetDetailView = {
  asset: MediaAssetSummaryView;
  variants: MediaVariantView[];
  translations: MediaTranslationView[];
};
