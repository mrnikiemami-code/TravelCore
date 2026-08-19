import type { AppLocale } from "@/lib/i18n";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";

const faSelection: RelatedTourView[] = [
  {
    tourProductId: "fixture-list-exp-01",
    kind: "Experience",
    code: "EXP-DARYACHE-01",
    name: "تور دریاچه دالامپر — تجربه طبیعت‌گردی",
    slug: "fixture-daryache-experience",
  },
  {
    tourProductId: "fixture-list-pkg-01",
    kind: "Package",
    code: "PKG-IST-01",
    name: "تور استانبول — پکیج خارجی",
    slug: "fixture-istanbul-package",
  },
];

const enSelection: RelatedTourView[] = [
  {
    tourProductId: "fixture-list-exp-01",
    kind: "Experience",
    code: "EXP-DARYACHE-01",
    name: "Daryache Nature Experience",
    slug: "fixture-daryache-experience",
  },
  {
    tourProductId: "fixture-list-pkg-01",
    kind: "Package",
    code: "PKG-IST-01",
    name: "Istanbul Package Tour",
    slug: "fixture-istanbul-package",
  },
];

/**
 * UIVAL-T004 fixture selection when destination filter is active.
 * Presentation-only catalog cards — not Search engine results.
 */
export function loadTourListingFixtureSelection(
  locale: AppLocale,
): RelatedTourView[] {
  return locale === "fa" ? faSelection : enSelection;
}
