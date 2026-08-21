import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type { AppLocale } from "@/lib/i18n";
import {
  mediaOriginalContentPath,
  resolveMediaAppProxySrc,
} from "@/lib/media/media-presentation";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";

type ApiMediaPresentation = {
  mediaAssetId: string;
  presentation?: {
    status: string;
    originalContentUrl?: string | null;
    variants?: Array<{
      profile: string;
      status: string;
      contentUrl?: string | null;
    }> | null;
  } | null;
};

type ApiTourMedia = {
  cover?: ApiMediaPresentation | null;
};

function resolveCoverSrc(cover: ApiMediaPresentation): string | null {
  const p = cover.presentation;
  if (!p || p.status !== "Ready") {
    return null;
  }
  const medium = p.variants?.find(
    (v) => v.profile.toLowerCase() === "medium" && v.status === "Ready",
  );
  if (medium?.contentUrl?.trim()) {
    const url = medium.contentUrl.trim();
    return resolveMediaAppProxySrc(url.startsWith("/") ? url : `/${url}`);
  }
  if (p.originalContentUrl?.trim()) {
    const url = p.originalContentUrl.trim();
    return resolveMediaAppProxySrc(url.startsWith("/") ? url : `/${url}`);
  }
  return resolveMediaAppProxySrc(mediaOriginalContentPath(cover.mediaAssetId));
}

/**
 * Frontend-only enrichment: attach Tour cover URLs for listing cards.
 * Uses existing Tour media/presentation API — omit coverSrc when unavailable.
 */
export async function enrichToursWithCoverMedia(
  locale: AppLocale,
  tours: RelatedTourView[],
  limit = 12,
): Promise<RelatedTourView[]> {
  const targets = tours.slice(0, limit);
  const rest = tours.slice(limit);

  const enriched = await Promise.all(
    targets.map(async (tour) => {
      const media = await apiGetJson<ApiTourMedia>(
        `/api/tour/products/${encodeURIComponent(tour.tourProductId)}/media/presentation?locale=${encodeURIComponent(locale)}`,
        { cache: "no-store" },
      );
      if (!isApiOk(media) || !media.data.cover) {
        return { ...tour, coverSrc: null };
      }
      return {
        ...tour,
        coverSrc: resolveCoverSrc(media.data.cover),
      };
    }),
  );

  return [
    ...enriched,
    ...rest.map((t) => ({ ...t, coverSrc: t.coverSrc ?? null })),
  ];
}
