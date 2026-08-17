import { apiGetJson } from "@/lib/api/client";
import { asPageViewModel } from "@/lib/api/read-models";
import { apiFail, isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";
import {
  mediaOriginalContentPath,
  resolveMediaAppProxySrc,
} from "@/lib/media/media-presentation";

export type TourMediaItemView = {
  mediaAssetId: string;
  role: string;
  sortOrder: number;
  src: string | null;
  alt: string;
  width: number | null;
  height: number | null;
};

export type TourDetailPageViewModel = {
  locale: AppLocale;
  tourProductId: string;
  kind: string;
  code: string;
  name: string;
  description: string | null;
  slug: string;
  englishName: string;
  catalogStatus: string;
  cover: TourMediaItemView | null;
  gallery: TourMediaItemView[];
};

type ApiSlugHit = {
  tourProductId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
};

type ApiTourProduct = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
  localizedTitle?: string | null;
  localizedDescription?: string | null;
  localizedSlug?: string | null;
};

type ApiMediaPresentation = {
  mediaAssetId: string;
  role: string;
  sortOrder: number;
  presentation?: {
    mediaAssetId: string;
    status: string;
    originalContentUrl?: string | null;
    width?: number | null;
    height?: number | null;
    variants?: Array<{
      profile: string;
      status: string;
      contentUrl?: string | null;
      width?: number | null;
      height?: number | null;
    }> | null;
    altCaption?: { altText?: string | null } | null;
  } | null;
};

type ApiTourMedia = {
  tourProductId: string;
  cover?: ApiMediaPresentation | null;
  gallery?: ApiMediaPresentation[] | null;
};

function mapMediaItem(item: ApiMediaPresentation): TourMediaItemView {
  const p = item.presentation;
  const ready = p?.status === "Ready";
  const medium = p?.variants?.find(
    (v) => v.profile.toLowerCase() === "medium" && v.status === "Ready",
  );
  const src =
    (medium?.contentUrl
      ? resolveMediaAppProxySrc(
          medium.contentUrl.startsWith("/")
            ? medium.contentUrl
            : `/${medium.contentUrl}`,
        )
      : null) ??
    (ready && p?.originalContentUrl
      ? resolveMediaAppProxySrc(
          p.originalContentUrl.startsWith("/")
            ? p.originalContentUrl
            : `/${p.originalContentUrl}`,
        )
      : null) ??
    (ready
      ? resolveMediaAppProxySrc(mediaOriginalContentPath(item.mediaAssetId))
      : null);

  return {
    mediaAssetId: item.mediaAssetId,
    role: item.role,
    sortOrder: item.sortOrder,
    src,
    alt: p?.altCaption?.altText?.trim() || "",
    width: medium?.width ?? p?.width ?? null,
    height: medium?.height ?? p?.height ?? null,
  };
}

/**
 * Loads public TourProduct detail for locale + Tour-owned translation slug (P09-R5).
 * Draft/Inactive → 404 (by-slug publicOnly). Missing localized title → 404 (ADR 0008).
 * Catalog Published ≠ SEO Index (P09-R6); IndexPolicy remains SEO-owned.
 * Cover/Gallery via Tour media/presentation compose (Media.Contracts; app-proxy only).
 */
export async function loadTourDetailPage(
  locale: AppLocale,
  slug: string,
): Promise<ApiResult<TourDetailPageViewModel>> {
  const localeEnc = encodeURIComponent(locale);
  const slugEnc = encodeURIComponent(slug.trim());

  const hitResult = await apiGetJson<ApiSlugHit>(
    `/api/tour/products/by-slug/${localeEnc}/${slugEnc}`,
    { cache: "no-store" },
  );
  if (!isApiOk(hitResult)) {
    return hitResult;
  }

  const id = hitResult.data.tourProductId;
  const [productResult, mediaResult] = await Promise.all([
    apiGetJson<ApiTourProduct>(`/api/tour/products/${id}?locale=${localeEnc}`, {
      cache: "no-store",
    }),
    apiGetJson<ApiTourMedia>(
      `/api/tour/products/${id}/media/presentation?locale=${localeEnc}`,
      { cache: "no-store" },
    ),
  ]);

  if (!isApiOk(productResult)) {
    return productResult;
  }
  if (!isApiOk(mediaResult) && mediaResult.status !== 404) {
    return mediaResult;
  }

  const product = productResult.data;
  if (product.catalogStatus !== "Published") {
    return apiFail({
      kind: "http",
      status: 404,
      message: "TourProduct is not publicly available.",
    });
  }

  const localizedTitle = product.localizedTitle?.trim() || null;
  if (!localizedTitle) {
    return apiFail({
      kind: "http",
      status: 404,
      message: "Localized TourProduct representation is missing.",
    });
  }

  const media = isApiOk(mediaResult) ? mediaResult.data : null;
  const cover = media?.cover ? mapMediaItem(media.cover) : null;
  const gallery = (media?.gallery ?? []).map(mapMediaItem);

  return {
    ok: true,
    status: 200,
    data: asPageViewModel({
      locale,
      tourProductId: id,
      kind: product.kind,
      code: product.code,
      name: localizedTitle,
      description: product.localizedDescription?.trim() || null,
      slug: hitResult.data.slug,
      englishName: product.englishName,
      catalogStatus: product.catalogStatus,
      cover,
      gallery,
    }),
  };
}
