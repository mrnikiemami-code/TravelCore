/**
 * P14-R8: Filter presentation state + URL composition.
 * Presentation only — not Search retrieval, not facet calculation, not SEO landing ownership.
 */

export type ListingPresentationSort = "code" | "name";

export type ListingFilterCriteria = {
  destinationSlug: string | null;
  sort: ListingPresentationSort;
};

export function parseListingFilterCriteria(input: {
  destination?: string | string[];
  sort?: string | string[];
}): ListingFilterCriteria {
  const destinationRaw = firstParam(input.destination);
  const sortRaw = firstParam(input.sort);
  return {
    destinationSlug:
      destinationRaw && destinationRaw.trim().length > 0
        ? destinationRaw.trim()
        : null,
    sort: sortRaw === "name" ? "name" : "code",
  };
}

export function listingFilterQuery(criteria: ListingFilterCriteria): string {
  const qs = new URLSearchParams();
  if (criteria.destinationSlug) {
    qs.set("destination", criteria.destinationSlug);
  }
  if (criteria.sort !== "code") {
    qs.set("sort", criteria.sort);
  }
  const text = qs.toString();
  return text.length > 0 ? `?${text}` : "";
}

export function listingFilterHref(
  locale: string,
  criteria: ListingFilterCriteria,
): string {
  return `/${locale}/tours${listingFilterQuery(criteria)}`;
}

function firstParam(value: string | string[] | undefined): string | undefined {
  if (Array.isArray(value)) {
    return value[0];
  }
  return value;
}
