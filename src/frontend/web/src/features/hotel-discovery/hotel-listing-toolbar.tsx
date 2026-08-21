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
      className="rounded-2xl border border-border bg-surface/95 p-4 shadow-sm sm:p-5"
    >
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <label className="flex min-w-0 flex-1 flex-col gap-1.5 text-sm">
          <span className="font-medium text-foreground">{copy.filterLabel}</span>
          <input
            type="search"
            name="q"
            defaultValue={criteria.q || ""}
            placeholder={copy.filterPlaceholder}
            autoComplete="off"
            className="min-h-touch rounded-lg border border-border bg-background px-3 py-2 outline-none ring-[#1D4ED8] focus:ring-2"
          />
        </label>
        <label className="flex w-full flex-col gap-1.5 text-sm sm:w-56">
          <span className="font-medium text-foreground">{copy.sortLabel}</span>
          <select
            name="sort"
            defaultValue={criteria.sort}
            className="min-h-touch rounded-lg border border-border bg-background px-3 py-2 outline-none ring-[#1D4ED8] focus:ring-2"
          >
            <option value="name-asc">{copy.sortNameAsc}</option>
            <option value="name-desc">{copy.sortNameDesc}</option>
            <option value="stars-desc">{copy.sortStarsDesc}</option>
            <option value="stars-asc">{copy.sortStarsAsc}</option>
          </select>
        </label>
        <button
          type="submit"
          className="min-h-touch rounded-lg bg-[#1D4ED8] px-5 py-2 text-sm font-semibold text-white hover:bg-[#1E40AF]"
        >
          {copy.apply}
        </button>
      </div>
    </form>
  );
}
