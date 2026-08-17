import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";

export type RelatedContentView = {
  contentItemId: string;
  kind: string;
  code: string;
  name: string;
  slug: string;
};

type ApiRelatedContent = {
  contentItemId: string;
  kind: string;
  code: string;
  name: string;
  slug: string;
};

function mapRelated(item: ApiRelatedContent): RelatedContentView {
  return {
    contentItemId: item.contentItemId,
    kind: item.kind,
    code: item.code,
    name: item.name,
    slug: item.slug,
  };
}

function mergeRelated(lists: RelatedContentView[][]): RelatedContentView[] {
  const seen = new Set<string>();
  const merged: RelatedContentView[] = [];
  for (const list of lists) {
    for (const item of list) {
      if (seen.has(item.contentItemId)) {
        continue;
      }
      seen.add(item.contentItemId);
      merged.push(item);
    }
  }
  merged.sort((a, b) => a.code.localeCompare(b.code) || a.contentItemId.localeCompare(b.contentItemId));
  return merged.slice(0, 6);
}

export async function loadRelatedContentByDestination(
  destinationId: string,
  locale: string,
): Promise<RelatedContentView[]> {
  const qs = new URLSearchParams({
    destinationId,
    locale,
  });
  const result = await apiGetJson<ApiRelatedContent[]>(
    `/api/content/items/related-published?${qs.toString()}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result) || !Array.isArray(result.data)) {
    return [];
  }
  return result.data.map(mapRelated);
}

export async function loadRelatedContentByDestinations(
  destinationIds: string[],
  locale: string,
): Promise<RelatedContentView[]> {
  const unique = [...new Set(destinationIds.map((id) => id.trim()).filter(Boolean))];
  if (unique.length === 0) {
    return [];
  }
  const lists = await Promise.all(
    unique.map((id) => loadRelatedContentByDestination(id, locale)),
  );
  return mergeRelated(lists);
}
