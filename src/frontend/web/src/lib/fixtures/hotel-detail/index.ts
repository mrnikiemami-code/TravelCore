import type { AppLocale } from "@/lib/i18n";
import { apiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { PlaceDetailPageViewModel } from "@/types/pages/place-detail";
import { hotelDetailEnFixture } from "./en";
import { hotelDetailFaFixture } from "./fa";

/** HotelDetailPage archetype = Place catalog with kind Hotel (P07). */
export function loadHotelDetailFixture(
  locale: AppLocale,
): ApiResult<PlaceDetailPageViewModel> {
  if (locale === "fa") {
    return apiOk(hotelDetailFaFixture);
  }
  if (locale === "en") {
    return apiOk(hotelDetailEnFixture);
  }
  return {
    ok: false,
    kind: "config",
    status: 0,
    message: `No HotelDetail fixture published for locale "${locale}".`,
  };
}

export { hotelDetailFaFixture } from "./fa";
export { hotelDetailEnFixture } from "./en";
