import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import { contentPublicPath } from "./listing-landing";
import type { RelatedContentView } from "./load-related-content";

/**
 * P14-R6: Content enrichment composition. Deterministic same-destination links.
 * Content remains CMS SoT. Not copied into Tour. Publication ≠ index rules.
 */
export function RelatedContentList({
  locale,
  items,
}: {
  locale: AppLocale;
  items: RelatedContentView[];
}) {
  const linked = items.filter((item) => contentPublicPath(locale, item.kind, item.slug));
  return (
    <Stack gap="sm">
      <Text as="h2" role="heading">
        {locale === "fa" ? "محتوای مرتبط" : "Related content"}
      </Text>
      <Text role="caption">
        {locale === "fa"
          ? "مقصد مشترک · مالکیت تحریری جدا می‌ماند"
          : "Shared destination · editorial ownership stays in Content"}
      </Text>
      {linked.length === 0 ? (
        <Text role="muted">
          {locale === "fa" ? "محتوای مرتبطی نیست." : "No related content."}
        </Text>
      ) : (
        <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          {linked.map((item) => {
            const href = contentPublicPath(locale, item.kind, item.slug);
            if (!href) {
              return null;
            }
            return (
              <li key={item.contentItemId}>
                <a
                  className="min-h-touch inline-flex w-full flex-col rounded-md border border-border px-3 py-2 text-sm underline-offset-2 hover:underline"
                  href={href}
                >
                  <Text>{item.name}</Text>
                  <Text role="caption">
                    {item.kind} · <LtrValue>{item.code}</LtrValue>
                  </Text>
                </a>
              </li>
            );
          })}
        </ul>
      )}
    </Stack>
  );
}
