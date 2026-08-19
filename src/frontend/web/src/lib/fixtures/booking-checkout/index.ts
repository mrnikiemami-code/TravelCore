import type { AppLocale } from "@/lib/i18n";

export type BookingCheckoutDepartureFixture = {
  id: string;
  label: string;
};

const faDepartures: BookingCheckoutDepartureFixture[] = [
  { id: "dep-fixture-2026-10-01", label: "2026-10-01 – 2026-10-04" },
  { id: "dep-fixture-2026-11-15", label: "2026-11-15 – 2026-11-18" },
];

const enDepartures: BookingCheckoutDepartureFixture[] = [
  { id: "dep-fixture-2026-10-01", label: "Oct 1 – Oct 4, 2026" },
  { id: "dep-fixture-2026-11-15", label: "Nov 15 – Nov 18, 2026" },
];

export function loadBookingCheckoutFixture(locale: AppLocale) {
  const departures = locale === "fa" ? faDepartures : enDepartures;
  return {
    slug: "fixture-daryache-experience",
    departures,
  };
}
