import type { AppLocale } from "@/lib/i18n";
import { apiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { TourDetailPageViewModel } from "@/features/tour-detail/load-tour-detail";
import { experienceTourDetailEnFixture } from "./en";
import { experienceTourDetailFaFixture } from "./fa";

/**
 * Locale-specific Experience Tour detail fixture loader (UIVAL-T003).
 * FA and EN are distinct published copies — no silent cross-locale reuse.
 */
export function loadExperienceTourDetailFixture(
  locale: AppLocale,
): ApiResult<TourDetailPageViewModel> {
  if (locale === "fa") {
    return apiOk(experienceTourDetailFaFixture);
  }
  if (locale === "en") {
    return apiOk(experienceTourDetailEnFixture);
  }
  return {
    ok: false,
    kind: "config",
    status: 0,
    message: `No ExperienceTourDetail fixture published for locale "${locale}".`,
  };
}

export { experienceTourDetailFaFixture } from "./fa";
export { experienceTourDetailEnFixture } from "./en";
