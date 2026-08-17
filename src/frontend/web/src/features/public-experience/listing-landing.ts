/**
 * P14-R3: Listing vs SEO Landing route and composition boundary.
 * Listing = Discovery. Landing = Search Intent. Not a filtered listing.
 * Search engine (query / ranking / FTS) is P15. IndexPolicy stays in SEO.
 */

export const LISTING_PURPOSE = "Discovery" as const;
export const LANDING_PURPOSE = "SearchIntent" as const;
export const LANDING_IS_FILTERED_LISTING = false;

export const LISTING_ROUTE_PATTERN = "/tours" as const;
export const LANDING_ROUTE_PATTERN = "/tours/{topic}/{intent}" as const;
export const DETAIL_ROUTE_PATTERN = "/tours/{slug}" as const;

export const LISTING_COMPOSITION = "FilterSlot+SortSlot+Selection" as const;
export const LANDING_COMPOSITION =
  "CuratedContent+RelatedToursSlot+SeoMetadata+UserIntent" as const;

export const SEARCH_ENGINE_OWNER = "Search" as const;
export const INDEX_POLICY_OWNER = "Seo" as const;

export function listingPath(locale: string): string {
  return `/${locale}/tours`;
}

export function landingPath(locale: string, topic: string, intent: string): string {
  return `/${locale}/tours/${encodeURIComponent(topic)}/${encodeURIComponent(intent)}`;
}

export function detailPath(locale: string, slug: string): string {
  return `/${locale}/tours/${encodeURIComponent(slug)}`;
}
