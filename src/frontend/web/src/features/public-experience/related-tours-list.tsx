import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import { detailPath } from "./listing-landing";
import type { RelatedTourView } from "./load-related-tours";

/**
 * P14-R5: Related tours composition. Deterministic same-destination links.
 * Not a recommendation engine. Not Search ranking.
 */
export function RelatedToursList({
  locale,
  items,
}: {
  locale: AppLocale;
  items: RelatedTourView[];
}) {
  return (
    <Stack gap="sm">
      <Text as="h2" role="heading">
        {locale === "fa" ? "تورهای مرتبط" : "Related tours"}
      </Text>
      <Text role="caption">
        {locale === "fa"
          ? "مقصد مشترک · بدون پیشنهاد شخصی"
          : "Shared destination · not personalized"}
      </Text>
      {items.length === 0 ? (
        <Text role="muted">
          {locale === "fa" ? "تور مرتبطی نیست." : "No related tours."}
        </Text>
      ) : (
        <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          {items.map((item) => (
            <li key={item.tourProductId}>
              <a
                className="min-h-touch inline-flex w-full flex-col rounded-md border border-border px-3 py-2 text-sm underline-offset-2 hover:underline"
                href={detailPath(locale, item.slug)}
              >
                <Text>{item.name}</Text>
                <Text role="caption">
                  {item.kind} · <LtrValue>{item.code}</LtrValue>
                </Text>
              </a>
            </li>
          ))}
        </ul>
      )}
    </Stack>
  );
}
