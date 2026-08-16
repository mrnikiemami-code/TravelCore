/**
 * Frontend metadata consumer for SEO hreflang bindings (TC-P05-T006 / ADR 0008).
 *
 * Backend: GET /api/seo/hreflang/{resourceType}/{resourceId}
 * Only genuine SeoRoute locales are returned — never fabricate missing locales.
 */
export type SeoHreflangAlternate = {
  locale: string;
  path: string;
  href: string;
};

export type SeoHreflangBindings = {
  resourceType: string;
  resourceId: string;
  alternates: SeoHreflangAlternate[];
};

/** Maps SEO hreflang bindings to Next.js Metadata.alternates.languages. */
export function languagesFromHreflang(
  bindings: SeoHreflangBindings | null | undefined,
): Record<string, string> {
  const languages: Record<string, string> = {};
  if (!bindings?.alternates?.length) {
    return languages;
  }

  for (const alt of bindings.alternates) {
    if (!alt.locale?.trim() || !alt.href?.trim()) continue;
    languages[alt.locale] = alt.href;
  }

  return languages;
}
