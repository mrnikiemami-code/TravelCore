import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import {
  listingFilterHref,
  type ListingFilterCriteria,
  type ListingPresentationSort,
} from "./filter-presentation";

/**
 * P14-R8: Server-first discovery filter chrome. URL/query state only.
 * Not a Search engine. Not facet calculation. Not SEO landing ownership.
 */
export function ListingFilters({
  locale,
  criteria,
}: {
  locale: AppLocale;
  criteria: ListingFilterCriteria;
}) {
  const clearHref = listingFilterHref(locale, {
    destinationSlug: null,
    sort: "code",
  });
  const sortLinks: { value: ListingPresentationSort; label: string }[] = [
    {
      value: "code",
      label: locale === "fa" ? "کد" : "Code",
    },
    {
      value: "name",
      label: locale === "fa" ? "نام" : "Name",
    },
  ];

  return (
    <Stack gap="sm">
      <Text as="h2" role="heading">
        {locale === "fa" ? "فیلتر نمایشی" : "Presentation filters"}
      </Text>
      <Text role="caption">
        {locale === "fa"
          ? "وضعیت URL · کشف · بدون موتور جستجو"
          : "URL state · discovery · not a search engine"}
      </Text>

      <form method="get" action={`/${locale}/tours`} className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm">
          <span>{locale === "fa" ? "مقصد (slug)" : "Destination (slug)"}</span>
          <input
            className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
            name="destination"
            defaultValue={criteria.destinationSlug ?? ""}
            placeholder={locale === "fa" ? "مثلاً istanbul" : "e.g. istanbul"}
            autoComplete="off"
          />
        </label>
        <input type="hidden" name="sort" value={criteria.sort} />
        <button
          type="submit"
          className="min-h-touch rounded-md border border-border px-4 py-2 text-sm"
        >
          {locale === "fa" ? "اعمال" : "Apply"}
        </button>
        <a
          className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
          href={clearHref}
        >
          {locale === "fa" ? "پاک کردن" : "Clear"}
        </a>
      </form>

      {criteria.destinationSlug ? (
        <Text>
          {locale === "fa" ? "معیار فعال:" : "Active criteria:"}{" "}
          <LtrValue>{criteria.destinationSlug}</LtrValue>
        </Text>
      ) : (
        <Text role="muted">
          {locale === "fa"
            ? "مقصدی انتخاب نشده است."
            : "No destination selected."}
        </Text>
      )}

      <Stack gap="sm">
        <Text as="h3" role="heading">
          {locale === "fa" ? "مرتب‌سازی نمایشی" : "Presentation sort"}
        </Text>
        <Text role="caption">
          {locale === "fa"
            ? "برچسب نمایش · مالک بازیابی نیست"
            : "Display label · does not own retrieval"}
        </Text>
        <ul className="flex flex-wrap gap-2">
          {sortLinks.map((item) => {
            const href = listingFilterHref(locale, {
              destinationSlug: criteria.destinationSlug,
              sort: item.value,
            });
            const active = criteria.sort === item.value;
            return (
              <li key={item.value}>
                <a
                  className={
                    active
                      ? "min-h-touch inline-flex rounded-md border border-border bg-muted px-3 py-2 text-sm"
                      : "min-h-touch inline-flex rounded-md border border-border px-3 py-2 text-sm underline-offset-2 hover:underline"
                  }
                  href={href}
                  aria-current={active ? "page" : undefined}
                >
                  {item.label}
                </a>
              </li>
            );
          })}
        </ul>
      </Stack>
    </Stack>
  );
}
