import type { AppLocale } from "@/lib/i18n";
import {
  tourListingCopy,
  type TourListingCriteria,
} from "@/features/tour-discovery/tour-listing-criteria";
import { tourDestinationOptions } from "@/features/tour-discovery/tour-destination-options";

/**
 * Tour listing filter + sort (GET form).
 * Destination-scoped discovery via existing related-published contract.
 * P36-T004: human-friendly destination select (slugs stay as values).
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
  const destinations = tourDestinationOptions(locale);

  return (
    <form
      method="get"
      action={action}
      className="rounded-2xl border border-border bg-surface/95 p-4 shadow-sm sm:p-5"
    >
      <p className="mb-3 text-xs text-muted-foreground">{copy.patternNote}</p>
      <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end">
        <label className="flex min-w-0 flex-1 flex-col gap-1.5 text-sm sm:min-w-[12rem]">
          <span className="font-medium text-foreground">
            {copy.destinationLabel}
          </span>
          <select
            name="destination"
            defaultValue={criteria.destination || ""}
            className="min-h-touch rounded-lg border border-border bg-background px-3 py-2 outline-none ring-[#1D4ED8] focus:ring-2"
          >
            <option value="">{copy.destinationAny}</option>
            {destinations.map((d) => (
              <option key={d.slug} value={d.slug}>
                {d.label}
              </option>
            ))}
          </select>
        </label>
        <label className="flex min-w-0 flex-1 flex-col gap-1.5 text-sm sm:min-w-[12rem]">
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
        <label className="flex w-full flex-col gap-1.5 text-sm sm:w-48">
          <span className="font-medium text-foreground">{copy.sortLabel}</span>
          <select
            name="sort"
            defaultValue={criteria.sort}
            className="min-h-touch rounded-lg border border-border bg-background px-3 py-2 outline-none ring-[#1D4ED8] focus:ring-2"
          >
            <option value="name-asc">{copy.sortNameAsc}</option>
            <option value="name-desc">{copy.sortNameDesc}</option>
            <option value="kind-asc">{copy.sortKindAsc}</option>
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
