import type { AppLocale } from "@/lib/i18n";
import { apiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { VisaDetailPageViewModel } from "@/types/pages/visa-detail";
import { visaDetailEnFixture } from "./en";
import { visaDetailFaFixture } from "./fa";

export function loadVisaDetailFixture(
  locale: AppLocale,
): ApiResult<VisaDetailPageViewModel> {
  if (locale === "fa") {
    return apiOk(visaDetailFaFixture);
  }
  if (locale === "en") {
    return apiOk(visaDetailEnFixture);
  }
  return {
    ok: false,
    kind: "config",
    status: 0,
    message: `No VisaDetail fixture published for locale "${locale}".`,
  };
}

export { visaDetailFaFixture } from "./fa";
export { visaDetailEnFixture } from "./en";
