import type { AppLocale } from "@/lib/i18n";
import {
  hotelListingCopy,
  type HotelListingCriteria,
} from "@/features/hotel-discovery/hotel-listing-criteria";

/**
 * Hotel listing filter + sort pattern (GET form → same page).
 * Experience-only; does not call Search/HotelBooking engines.
 */
export function HotelListingToolbar({
  locale,
  criteria,
}: {
  locale: AppLocale;
  criteria: HotelListingCriteria;
}) {
  const copy = hotelListingCopy(locale);
  const action = `/${locale}/hotels`;

  return (
    <form
      method="get"
      action={action}
      className="rounded-xl border border-border bg-surface p-4 shadow-sm"
    >
      <p className="mb-3 text-xs text-muted-foreground">{copy.patternNote}</p>
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm">
          <span className="font-medium">{copy.filterLabel}</span>
          <input
            type="search"
            name="q"
            defaultValue={criteria.q}
            placeholder={copy.filterPlaceholder}
            className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
          />
        </label>
        <label className="flex w-full flex-col gap-1 text-sm sm:w-56">
          <span className="font-medium">{copy.sortLabel}</span>
          <select
            name="sort"
            defaultValue={criteria.sort}
            className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
          >
            <option value="name-asc">{copy.sortNameAsc}</option>
            <option value="name-desc">{copy.sortNameDesc}</option>
            <option value="stars-desc">{copy.sortStarsDesc}</option>
            <option value="stars-asc">{copy.sortStarsAsc}</option>
          </select>
        </label>
        <button
          type="submit"
          className="min-h-touch rounded-md border border-border bg-background px-4 py-2 text-sm font-medium underline-offset-2 hover:underline"
        >
          {copy.apply}
        </button>
      </div>
    </form>
  );
}
