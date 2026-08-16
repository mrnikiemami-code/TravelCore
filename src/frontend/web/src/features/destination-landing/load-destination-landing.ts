import { apiGetJson } from "@/lib/api/client";
import { asPageViewModel } from "@/lib/api/read-models";
import { apiFail, isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";
import type { DestinationLandingPageViewModel } from "@/types/pages/destination-landing";

type ApiSlugHit = {
  destinationId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
};

type ApiDestination = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  parentId?: string | null;
  isoCountryCode?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  localizedName?: string | null;
  localizedDescription?: string | null;
  locale?: string | null;
};

type ApiPathNode = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  parentId?: string | null;
  depthFromRoot: number;
};

type ApiPath = {
  destinationId: string;
  ancestorsRootFirst: ApiPathNode[];
  self: ApiPathNode;
  breadcrumbRootFirst: ApiPathNode[];
};

type ApiTranslation = {
  destinationId: string;
  localeCode: string;
  name: string;
  description?: string | null;
  slug?: string | null;
};

async function translationForLocale(
  destinationId: string,
  locale: string,
): Promise<{ name: string; slug: string | null; description: string | null } | null> {
  const result = await apiGetJson<ApiTranslation[]>(
    `/api/destination/destinations/${destinationId}/translations`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) return null;
  const hit = (result.data ?? []).find(
    (t) => t.localeCode.toLowerCase() === locale.toLowerCase(),
  );
  if (!hit || !hit.name?.trim()) return null;
  return {
    name: hit.name,
    slug: hit.slug?.trim() ? hit.slug.trim() : null,
    description: hit.description ?? null,
  };
}

/**
 * Loads the public Destination landing PVM for locale + Destination-owned slug.
 * Missing slug / missing localized representation → ApiFailure (caller maps to notFound).
 * Does not fabricate cross-locale content (ADR 0008).
 */
export async function loadDestinationLandingPage(
  locale: AppLocale,
  slug: string,
): Promise<ApiResult<DestinationLandingPageViewModel>> {
  const localeEnc = encodeURIComponent(locale);
  const slugEnc = encodeURIComponent(slug.trim());

  const hitResult = await apiGetJson<ApiSlugHit>(
    `/api/destination/destinations/by-slug/${localeEnc}/${slugEnc}`,
    { cache: "no-store" },
  );
  if (!isApiOk(hitResult)) {
    return hitResult;
  }

  const id = hitResult.data.destinationId;
  const [destResult, pathResult, childrenResult] = await Promise.all([
    apiGetJson<ApiDestination>(
      `/api/destination/destinations/${id}?locale=${localeEnc}`,
      { cache: "no-store" },
    ),
    apiGetJson<ApiPath>(`/api/destination/destinations/${id}/path`, {
      cache: "no-store",
    }),
    apiGetJson<ApiDestination[]>(
      `/api/destination/destinations/${id}/children`,
      { cache: "no-store" },
    ),
  ]);

  if (!isApiOk(destResult)) return destResult;
  if (!isApiOk(pathResult)) return pathResult;
  if (!isApiOk(childrenResult)) return childrenResult;

  const dest = destResult.data;
  const localizedName = dest.localizedName?.trim() || null;
  // ADR 0008: do not present englishName as the locale page body under /{locale}/...
  if (!localizedName) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Localized Destination representation is not available for this locale.",
    });
  }

  const crumbNodes = pathResult.data.breadcrumbRootFirst ?? [];
  const breadcrumb = await Promise.all(
    crumbNodes.map(async (node) => {
      const tr = await translationForLocale(node.id, locale);
      // ADR 0008: do not silently substitute English editorial name under locale URL.
      // Without a locale translation, expose catalog code only (identity, not body copy).
      return {
        name: tr?.name ?? node.code,
        slug: tr?.slug ?? null,
        kind: node.kind,
        code: node.code,
      };
    }),
  );

  const children = await Promise.all(
    (childrenResult.data ?? []).map(async (child) => {
      const tr = await translationForLocale(child.id, locale);
      return {
        name: tr?.name ?? child.code,
        slug: tr?.slug ?? null,
        kind: child.kind,
        code: child.code,
      };
    }),
  );

  return {
    ok: true,
    status: 200,
    data: asPageViewModel({
      locale,
      kind: dest.kind,
      code: dest.code,
      name: localizedName,
      description: dest.localizedDescription ?? null,
      slug: hitResult.data.slug,
      englishName: dest.englishName,
      isoCountryCode: dest.isoCountryCode ?? null,
      latitude: dest.latitude ?? null,
      longitude: dest.longitude ?? null,
      breadcrumb,
      children,
    }),
  };
}
