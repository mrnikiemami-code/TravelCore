import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import {
  languagesFromHreflang,
  type SeoHreflangBindings,
} from "@/lib/seo/hreflang-contract";

/**
 * Loads SEO-owned hreflang bindings for a publishable resource.
 * Missing API / missing routes → empty languages (no fabricated alternates).
 */
export async function loadSeoHreflangLanguages(
  resourceType: string,
  resourceId: string,
): Promise<Record<string, string>> {
  const typeEnc = encodeURIComponent(resourceType);
  const idEnc = encodeURIComponent(resourceId);
  const result = await apiGetJson<SeoHreflangBindings>(
    `/api/seo/hreflang/${typeEnc}/${idEnc}`,
    { cache: "no-store" },
  );

  if (!isApiOk(result)) {
    return {};
  }

  return languagesFromHreflang(result.data);
}

/**
 * Loads hreflang via current locale+path (SeoRoute current only).
 * Historical/redirect/unknown paths → empty (no invent).
 */
export async function loadSeoHreflangLanguagesByPath(
  locale: string,
  path: string,
): Promise<Record<string, string>> {
  const localeEnc = encodeURIComponent(locale);
  const normalized = path.replace(/^\/+/, "").replace(/\/+$/, "");
  const result = await apiGetJson<SeoHreflangBindings>(
    `/api/seo/hreflang/by-path/${localeEnc}/${normalized}`,
    { cache: "no-store" },
  );

  if (!isApiOk(result)) {
    return {};
  }

  return languagesFromHreflang(result.data);
}
