import { apiGetJson } from "@/lib/api/client";
import { asPageViewModel } from "@/lib/api/read-models";
import { apiFail, isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";
import type { ContentDetailPageViewModel } from "@/types/pages/content-detail";

type ApiSlugHit = {
  contentItemId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
};

type ApiContentItem = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  localizedTitle?: string | null;
  localizedBody?: string | null;
  localizedExcerpt?: string | null;
  destinationIds?: string[] | null;
};

type ApiBlock = {
  id: string;
  kind: string;
  sortOrder: number;
  text?: string | null;
  headingLevel?: number | null;
  href?: string | null;
};

function publicPathForKind(kind: string, slug: string): string | null {
  switch (kind) {
    case "Article":
      return `articles/${slug}`;
    case "LandingPage":
      return `landing-pages/${slug}`;
    default:
      return null;
  }
}

/**
 * Loads public Content detail for locale + Content-owned slug.
 * publicOnly by-slug requires title+slug. Missing localized title → 404 (ADR 0008).
 * Guide and other kinds without a locked public path → 404.
 */
export async function loadContentDetailPage(
  locale: AppLocale,
  slug: string,
  expectedKind: "Article" | "LandingPage",
): Promise<ApiResult<ContentDetailPageViewModel>> {
  const localeEnc = encodeURIComponent(locale);
  const slugEnc = encodeURIComponent(slug.trim());

  const hitResult = await apiGetJson<ApiSlugHit>(
    `/api/content/items/by-slug/${localeEnc}/${slugEnc}`,
    { cache: "no-store" },
  );
  if (!isApiOk(hitResult)) {
    return hitResult;
  }

  if (hitResult.data.kind !== expectedKind) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Content kind does not match this public route.",
    });
  }

  const publicPath = publicPathForKind(hitResult.data.kind, hitResult.data.slug);
  if (!publicPath) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Content kind is not publicly routed.",
    });
  }

  const id = hitResult.data.contentItemId;
  const [itemResult, blocksResult] = await Promise.all([
    apiGetJson<ApiContentItem>(`/api/content/items/${id}?locale=${localeEnc}`, {
      cache: "no-store",
    }),
    apiGetJson<ApiBlock[]>(`/api/content/items/${id}/blocks`, {
      cache: "no-store",
    }),
  ]);

  if (!isApiOk(itemResult)) return itemResult;
  if (!isApiOk(blocksResult) && blocksResult.status !== 404) {
    return blocksResult;
  }

  const item = itemResult.data;
  const localizedTitle = item.localizedTitle?.trim() || null;
  if (!localizedTitle) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Localized Content representation is missing.",
    });
  }

  const blocks = (isApiOk(blocksResult) ? (blocksResult.data ?? []) : [])
    .slice()
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map((b) => ({
      id: b.id,
      kind: b.kind,
      sortOrder: b.sortOrder,
      text: b.text?.trim() || null,
      headingLevel: b.headingLevel ?? null,
      href: b.href?.trim() || null,
    }));

  return {
    ok: true,
    status: 200,
    data: asPageViewModel({
      locale,
      kind: item.kind,
      code: item.code,
      title: localizedTitle,
      body: item.localizedBody?.trim() || null,
      excerpt: item.localizedExcerpt?.trim() || null,
      slug: hitResult.data.slug,
      englishName: item.englishName,
      publicPath,
      blocks,
      destinationIds: item.destinationIds ?? [],
    }),
  };
}
