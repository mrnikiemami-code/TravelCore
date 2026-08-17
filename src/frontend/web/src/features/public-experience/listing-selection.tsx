import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import { detailPath } from "./listing-landing";
import type { RelatedTourView } from "./load-related-tours";
import type { ListingPresentationSort } from "./filter-presentation";

/**
 * P14-R8: Discovery selection presentation. Uses replaceable catalog read.
 * Not Search ranking. Presentation sort only reshapes already-loaded cards.
 */
export function ListingSelection({
  locale,
  items,
  sort,
  destinationSlug,
}: {
  locale: AppLocale;
  items: RelatedTourView[];
  sort: ListingPresentationSort;
  destinationSlug: string | null;
}) {
  const ordered =
    sort === "name"
      ? [...items].sort(
          (a, b) =>
            a.name.localeCompare(b.name) ||
            a.code.localeCompare(b.code) ||
            a.tourProductId.localeCompare(b.tourProductId),
        )
      : [...items].sort(
          (a, b) =>
            a.code.localeCompare(b.code) ||
            a.tourProductId.localeCompare(b.tourProductId),
        );

  return (
    <Stack gap="sm">
      <Text as="h2" role="heading">
        {locale === "fa" ? "انتخاب" : "Selection"}
      </Text>
      <Text role="caption">
        {locale === "fa"
          ? "انتخاب کاتالوگ · نه نتیجهٔ موتور جستجو"
          : "Catalog selection · not a search-engine result set"}
      </Text>
      {!destinationSlug ? (
        <Text role="muted">
          {locale === "fa"
            ? "برای دیدن انتخاب، مقصد را در فیلتر وارد کنید."
            : "Enter a destination in the filters to see selection."}
        </Text>
      ) : ordered.length === 0 ? (
        <Text role="muted">
          {locale === "fa"
            ? "انتخاب منتشرشده‌ای برای این مقصد نیست."
            : "No published selection for this destination."}
        </Text>
      ) : (
        <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          {ordered.map((item) => (
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
