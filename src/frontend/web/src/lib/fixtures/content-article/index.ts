import type { AppLocale } from "@/lib/i18n";
import { apiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { ContentDetailPageViewModel } from "@/types/pages/content-detail";
import { contentArticleEnFixture } from "./en";
import { contentArticleFaFixture } from "./fa";

export function loadContentArticleFixture(
  locale: AppLocale,
): ApiResult<ContentDetailPageViewModel> {
  if (locale === "fa") {
    return apiOk(contentArticleFaFixture);
  }
  if (locale === "en") {
    return apiOk(contentArticleEnFixture);
  }
  return {
    ok: false,
    kind: "config",
    status: 0,
    message: `No Content Article fixture published for locale "${locale}".`,
  };
}

export { contentArticleFaFixture } from "./fa";
export { contentArticleEnFixture } from "./en";
