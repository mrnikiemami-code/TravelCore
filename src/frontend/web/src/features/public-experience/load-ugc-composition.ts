import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type { AppLocale } from "@/lib/i18n";

export type UgcCommentView = {
  commentId: string;
  actorId: string;
  body: string;
  createdAt: string;
};

export type UgcDimensionRatingView = {
  dimensionCode: string;
  value: number;
};

export type UgcReviewView = {
  reviewId: string;
  actorId: string;
  overallRating: number;
  title: string | null;
  body: string | null;
  dimensionRatings: UgcDimensionRatingView[];
  comments: UgcCommentView[];
  createdAt: string;
};

export type UgcRatingSummaryView = {
  eligibleReviewCount: number;
  averageOverallRating: number;
};

export type UgcTravelogueView = {
  travelogueId: string;
  actorId: string;
  localeCode: string;
  title: string;
  body: string;
  comments: UgcCommentView[];
  createdAt: string;
};

export type UgcUserPhotoView = {
  userPhotoId: string;
  actorId: string;
  mediaAssetId: string;
  createdAt: string;
};

export type UgcCompositionView = {
  summary: UgcRatingSummaryView;
  reviews: UgcReviewView[];
  travelogues: UgcTravelogueView[];
  userPhotos: UgcUserPhotoView[];
};

type ApiComment = {
  commentId: string;
  actorId: string;
  body: string;
  createdAt: string;
};

type ApiReview = {
  reviewId: string;
  actorId: string;
  overallRating: number;
  title?: string | null;
  body?: string | null;
  dimensionRatings?: Array<{ dimensionCode: string; value: number }> | null;
  comments?: ApiComment[] | null;
  createdAt: string;
};

type ApiReviewPage = {
  summary?: {
    eligibleReviewCount?: number;
    averageOverallRating?: number;
  } | null;
  items?: ApiReview[] | null;
};

type ApiTravelogue = {
  travelogueId: string;
  actorId: string;
  localeCode: string;
  title: string;
  body: string;
  comments?: ApiComment[] | null;
  createdAt: string;
};

type ApiUserPhoto = {
  userPhotoId: string;
  actorId: string;
  mediaAssetId: string;
  createdAt: string;
};

function emptyComposition(): UgcCompositionView {
  return {
    summary: { eligibleReviewCount: 0, averageOverallRating: 0 },
    reviews: [],
    travelogues: [],
    userPhotos: [],
  };
}

function mapComment(item: ApiComment): UgcCommentView {
  return {
    commentId: item.commentId,
    actorId: item.actorId,
    body: item.body,
    createdAt: item.createdAt,
  };
}

function mapReview(item: ApiReview): UgcReviewView {
  return {
    reviewId: item.reviewId,
    actorId: item.actorId,
    overallRating: item.overallRating,
    title: item.title?.trim() || null,
    body: item.body?.trim() || null,
    dimensionRatings: (item.dimensionRatings ?? []).map((rating) => ({
      dimensionCode: rating.dimensionCode,
      value: rating.value,
    })),
    comments: (item.comments ?? []).map(mapComment),
    createdAt: item.createdAt,
  };
}

/**
 * P16-R8: Eligible UGC facts for public composition. Fail soft — empty on error.
 * Publicly eligible != SEO indexed and != automatically Search indexed.
 */
export async function loadUgcComposition(options: {
  targetType: "TourProduct" | "Place" | "Agency";
  targetId: string;
  locale: AppLocale;
}): Promise<UgcCompositionView> {
  const targetId = options.targetId.trim();
  if (!targetId) {
    return emptyComposition();
  }

  const reviewQs = new URLSearchParams({
    targetType: options.targetType,
    targetId,
  });
  const travelogueQs = new URLSearchParams({ localeCode: options.locale });

  const [reviewsResult, traveloguesResult, photosResult] = await Promise.all([
    apiGetJson<ApiReviewPage>(`/api/ugc/public/reviews?${reviewQs.toString()}`, {
      cache: "no-store",
    }),
    apiGetJson<ApiTravelogue[]>(
      `/api/ugc/public/travelogues?${travelogueQs.toString()}`,
      { cache: "no-store" },
    ),
    apiGetJson<ApiUserPhoto[]>(`/api/ugc/public/user-photos`, { cache: "no-store" }),
  ]);

  const page = isApiOk(reviewsResult) ? reviewsResult.data : null;
  const travelogues = isApiOk(traveloguesResult) ? traveloguesResult.data : [];
  const photos = isApiOk(photosResult) ? photosResult.data : [];

  return {
    summary: {
      eligibleReviewCount: page?.summary?.eligibleReviewCount ?? 0,
      averageOverallRating: page?.summary?.averageOverallRating ?? 0,
    },
    reviews: (page?.items ?? []).map(mapReview).slice(0, 6),
    travelogues: (Array.isArray(travelogues) ? travelogues : [])
      .map((item) => ({
        travelogueId: item.travelogueId,
        actorId: item.actorId,
        localeCode: item.localeCode,
        title: item.title,
        body: item.body,
        comments: (item.comments ?? []).map(mapComment),
        createdAt: item.createdAt,
      }))
      .slice(0, 6),
    userPhotos: (Array.isArray(photos) ? photos : [])
      .map((item) => ({
        userPhotoId: item.userPhotoId,
        actorId: item.actorId,
        mediaAssetId: item.mediaAssetId,
        createdAt: item.createdAt,
      }))
      .slice(0, 6),
  };
}
