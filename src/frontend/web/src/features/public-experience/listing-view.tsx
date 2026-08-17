import { Container, LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import { LISTING_PURPOSE, LISTING_ROUTE_PATTERN } from "./listing-landing";

export function PublicTourListingView({
  locale,
  destination,
}: {
  locale: AppLocale;
  destination?: string;
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

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "فیلتر نمایشی" : "Filter slot"}
            </Text>
            <Text>
              {destination ? (
                <>
                  {locale === "fa" ? "مقصد کشف:" : "Discovery destination:"}{" "}
                  <LtrValue>{destination}</LtrValue>
                </>
              ) : locale === "fa" ? (
                "جای فیلتر کشف · بدون موتور facet"
              ) : (
                "Discovery filter slot · no faceting engine"
              )}
            </Text>
          </Stack>

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "مرتب‌سازی نمایشی" : "Sort slot"}
            </Text>
            <Text>
              {locale === "fa"
                ? "جای مرتب‌سازی کشف · فقط نمایش"
                : "Discovery sort slot · presentation only"}
            </Text>
          </Stack>

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "انتخاب" : "Selection"}
            </Text>
            <Text>
              {locale === "fa"
                ? "انتخاب از کاتالوگ در این سطح است؛ نتیجهٔ فیلترشدهٔ جستجو نیست."
                : "Catalog selection lives here · not a filtered search result."}
            </Text>
          </Stack>
        </Stack>
      </Container>
    </div>
  );
}
