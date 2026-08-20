import type { AppLocale } from "@/lib/i18n";
import {
  tourListingCopy,
  type TourListingCriteria,
} from "@/features/tour-discovery/tour-listing-criteria";

/**
 * Tour listing filter + sort (GET form).
 * Destination-scoped discovery via existing related-published contract.
 */
export function TourListingToolbar({
  locale,
  criteria,
}: {
  locale: AppLocale;
  criteria: TourListingCriteria;
}) {
  const copy = tourListingCopy(locale);
  const action = `/${locale}/tours`;

  return (
    <form
      method="get"
      action={action}
      className="rounded-xl border border-border bg-surface p-4 shadow-sm"
    >
      <p className="mb-3 text-xs text-muted-foreground">{copy.patternNote}</p>
      <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end">
        <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm sm:min-w-[12rem]">
          <span className="font-medium">{copy.destinationLabel}</span>
          <input
            type="text"
            name="destination"
            defaultValue={criteria.destination}
            placeholder={copy.destinationPlaceholder}
            className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
            autoComplete="off"
          />
        </label>
        <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm sm:min-w-[12rem]">
          <span className="font-medium">{copy.filterLabel}</span>
          <input
            type="search"
            name="q"
            defaultValue={criteria.q}
            placeholder={copy.filterPlaceholder}
            className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
          />
        </label>
        <label className="flex w-full flex-col gap-1 text-sm sm:w-48">
          <span className="font-medium">{copy.sortLabel}</span>
          <select
            name="sort"
            defaultValue={criteria.sort}
            className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
          >
            <option value="name-asc">{copy.sortNameAsc}</option>
            <option value="name-desc">{copy.sortNameDesc}</option>
            <option value="kind-asc">{copy.sortKindAsc}</option>
          </select>
        </label>
        <button
          type="submit"
          className="min-h-touch rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-95"
        >
          {copy.apply}
        </button>
      </div>
    </form>
  );
}
