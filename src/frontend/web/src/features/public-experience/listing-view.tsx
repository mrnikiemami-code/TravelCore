import { Container, LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { ListingFilterCriteria } from "./filter-presentation";
import { ListingFilters } from "./listing-filters";
import { LISTING_PURPOSE, LISTING_ROUTE_PATTERN } from "./listing-landing";
import { ListingSelection } from "./listing-selection";
import type { RelatedTourView } from "./load-related-tours";

/**
 * P14-R3 / P14-R8: Discovery listing with presentation-only filters.
 * Not Search. Not SEO landing. Not facet ownership.
 */
export function PublicTourListingView({
  locale,
  criteria,
  selection,
}: {
  locale: AppLocale;
  criteria: ListingFilterCriteria;
  selection: RelatedTourView[];
}) {
  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {locale === "fa" ? "فهرست تورها" : "Tour listing"}
            </Text>
            <Text role="caption">
              {locale === "fa"
                ? "سطح کشف · نه موتور جستجو · نه لندینگ سئو"
                : "Discovery surface · not a search engine · not an SEO landing"}
            </Text>
            <Text role="muted">
              {LISTING_PURPOSE} · <LtrValue>{LISTING_ROUTE_PATTERN}</LtrValue>
            </Text>
          </Stack>

          <ListingFilters locale={locale} criteria={criteria} />
          <ListingSelection
            locale={locale}
            items={selection}
            sort={criteria.sort}
            destinationSlug={criteria.destinationSlug}
          />
        </Stack>
      </Container>
    </div>
  );
}
