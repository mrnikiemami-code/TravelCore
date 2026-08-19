import type { AppLocale } from "@/lib/i18n";
import { apiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { DestinationLandingPageViewModel } from "@/types/pages/destination-landing";
import { destinationLandingEnFixture } from "./en";
import { destinationLandingFaFixture } from "./fa";

export function loadDestinationLandingFixture(
  locale: AppLocale,
): ApiResult<DestinationLandingPageViewModel> {
  if (locale === "fa") {
    return apiOk(destinationLandingFaFixture);
  }
  if (locale === "en") {
    return apiOk(destinationLandingEnFixture);
  }
  return {
    ok: false,
    kind: "config",
    status: 0,
    message: `No DestinationLanding fixture published for locale "${locale}".`,
  };
}

export { destinationLandingFaFixture } from "./fa";
export { destinationLandingEnFixture } from "./en";
