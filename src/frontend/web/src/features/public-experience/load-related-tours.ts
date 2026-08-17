import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";

export type RelatedTourView = {
  tourProductId: string;
  kind: string;
  code: string;
  name: string;
  slug: string;
};

type ApiRelatedTour = {
  tourProductId: string;
  kind: string;
  code: string;
  name: string;
  slug: string;
};

function mapRelated(item: ApiRelatedTour): RelatedTourView {
  return {
    tourProductId: item.tourProductId,
    kind: item.kind,
    code: item.code,
    name: item.name,
    slug: item.slug,
  };
}

export async function loadRelatedToursByProduct(
  tourProductId: string,
  locale: string,
): Promise<RelatedTourView[]> {
  const result = await apiGetJson<ApiRelatedTour[]>(
    `/api/tour/products/${encodeURIComponent(tourProductId)}/related-published?locale=${encodeURIComponent(locale)}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result) || !Array.isArray(result.data)) {
    return [];
  }
  return result.data.map(mapRelated);
}

export async function loadRelatedToursByDestination(
  destinationId: string,
  locale: string,
  excludeTourProductId?: string,
): Promise<RelatedTourView[]> {
  const qs = new URLSearchParams({
    destinationId,
    locale,
  });
  if (excludeTourProductId) {
    qs.set("excludeTourProductId", excludeTourProductId);
  }
  const result = await apiGetJson<ApiRelatedTour[]>(
    `/api/tour/products/related-published?${qs.toString()}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result) || !Array.isArray(result.data)) {
    return [];
  }
  return result.data.map(mapRelated);
}
