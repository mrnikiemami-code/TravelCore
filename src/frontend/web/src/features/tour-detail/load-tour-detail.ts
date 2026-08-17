import { apiGetJson } from "@/lib/api/client";
import { asPageViewModel } from "@/lib/api/read-models";
import { apiFail, isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";

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

/**
 * Loads public TourProduct detail for locale + Tour-owned translation slug (P09-R5).
 * Draft/Inactive → 404 (by-slug publicOnly). Missing localized title → 404 (ADR 0008).
 * Catalog Published ≠ SEO Index (P09-R6); IndexPolicy remains SEO-owned.
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
  const productResult = await apiGetJson<ApiTourProduct>(
    `/api/tour/products/${id}?locale=${localeEnc}`,
    { cache: "no-store" },
  );
  if (!isApiOk(productResult)) {
    return productResult;
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
    }),
  };
}
