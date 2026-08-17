import { Container, LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import { LANDING_PURPOSE, LANDING_ROUTE_PATTERN } from "./listing-landing";
import { RelatedContentList } from "./related-content-list";
import { RelatedToursList } from "./related-tours-list";
import type { RelatedContentView } from "./load-related-content";
import type { RelatedTourView } from "./load-related-tours";

export function PublicTourLandingView({
  locale,
  topic,
  intent,
  relatedTours,
  relatedContent,
}: {
  locale: AppLocale;
  topic: string;
  intent: string;
  relatedTours: RelatedTourView[];
  relatedContent: RelatedContentView[];
}) {
  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {locale === "fa" ? "لندینگ سئو تور" : "Tour SEO landing"}
            </Text>
            <Text role="caption">
              {locale === "fa"
                ? "ترکیب نیت جستجو · نه فهرست فیلترشده"
                : "Search-intent composition · not a filtered listing"}
            </Text>
            <Text role="muted">
              {LANDING_PURPOSE} · <LtrValue>{LANDING_ROUTE_PATTERN}</LtrValue>
            </Text>
          </Stack>

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "نیت کاربر" : "User intent"}
            </Text>
            <Text>
              <LtrValue>
                {topic} / {intent}
              </LtrValue>
            </Text>
          </Stack>

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "محتوای گزینشی" : "Curated content"}
            </Text>
            <Text>
              {locale === "fa"
                ? "ترکیب محتوای تحریری از CMS · مالک کاتالوگ تور اینجا نیست."
                : "Editorial CMS composition · Tour catalog ownership stays outside this surface."}
            </Text>
            <RelatedContentList locale={locale} items={relatedContent} />
          </Stack>

          <RelatedToursList locale={locale} items={relatedTours} />

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "فراداده سئو" : "SEO metadata"}
            </Text>
            <Text>
              {locale === "fa"
                ? "ترکیب فراداده از SEO · مالک IndexPolicy اینجا نیست."
                : "Metadata is composed from SEO · IndexPolicy is not owned here."}
            </Text>
          </Stack>
        </Stack>
      </Container>
    </div>
  );
}
