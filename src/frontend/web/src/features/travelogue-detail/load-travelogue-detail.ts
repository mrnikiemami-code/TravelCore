import { apiGetJson } from "@/lib/api/client";
import { apiFail, apiOk, isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";
import type { UgcTravelogueView } from "@/features/public-experience/load-ugc-composition";

type ApiComment = {
  commentId: string;
  actorId: string;
  body: string;
  createdAt: string;
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

function mapTravelogue(item: ApiTravelogue): UgcTravelogueView {
  return {
    travelogueId: item.travelogueId,
    actorId: item.actorId,
    localeCode: item.localeCode,
    title: item.title,
    body: item.body,
    comments: (Array.isArray(item.comments) ? item.comments : []).map((comment) => ({
      commentId: comment.commentId,
      actorId: comment.actorId,
      body: comment.body,
      createdAt: comment.createdAt,
    })),
    createdAt: item.createdAt,
  };
}

/**
 * Loads a publicly eligible travelogue by id (TC-PRODSURF-T001/T013).
 * UGC owns eligibility; SEO IndexPolicy applies at page layer.
 */
export async function loadTravelogueDetailPage(
  _locale: AppLocale,
  travelogueId: string,
): Promise<ApiResult<UgcTravelogueView>> {
  const trimmed = travelogueId.trim();
  if (!trimmed) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Travelogue not found.",
    });
  }

  const result = await apiGetJson<ApiTravelogue>(
    `/api/ugc/public/travelogues/${encodeURIComponent(trimmed)}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) {
    return result;
  }

  return apiOk(mapTravelogue(result.data));
}
