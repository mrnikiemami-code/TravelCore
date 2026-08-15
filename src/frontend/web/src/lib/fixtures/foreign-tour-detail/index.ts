import type { AppLocale } from "@/lib/i18n";
import { apiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { ForeignTourDetailPageViewModel } from "@/types/pages/foreign-tour-detail";
import { foreignTourDetailEnFixture } from "./en";
import { foreignTourDetailFaFixture } from "./fa";

/**
 * Locale-specific fixture loader — FA and EN are distinct published copies.
 * Does not silently reuse FA content for EN.
 */
export function loadForeignTourDetailFixture(
  locale: AppLocale,
): ApiResult<ForeignTourDetailPageViewModel> {
  if (locale === "fa") {
    return apiOk(foreignTourDetailFaFixture);
  }
  if (locale === "en") {
    return apiOk(foreignTourDetailEnFixture);
  }
  // ar (and others): no fabricated published fixture in T012
  return {
    ok: false,
    kind: "config",
    status: 0,
    message: `No ForeignTourDetail fixture published for locale "${locale}".`,
  };
}

export { foreignTourDetailFaFixture } from "./fa";
export { foreignTourDetailEnFixture } from "./en";
