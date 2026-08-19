import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
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

/** Public eligible travelogues for locale-scoped discovery index (TC-DISCLINK-T001). */
export async function loadTravelogueDiscoveryList(
  locale: AppLocale,
): Promise<UgcTravelogueView[]> {
  const result = await apiGetJson<ApiTravelogue[]>(
    `/api/ugc/public/travelogues?localeCode=${encodeURIComponent(locale)}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) {
    return [];
  }

  return (Array.isArray(result.data) ? result.data : []).map(mapTravelogue);
}
