import { headers } from "next/headers";
import { RouteLoadingSkeleton } from "@/components/ui/route-state";
import {
  getLoadingCopy,
  localeFromPathname,
} from "@/lib/i18n/ui-labels";

async function resolveLocaleLabel() {
  const h = await headers();
  const path =
    h.get("x-pathname") ||
    h.get("x-invoke-path") ||
    h.get("next-url") ||
    h.get("referer") ||
    "";
  const locale = localeFromPathname(path);
  return getLoadingCopy(locale).loadingLabel;
}

/**
 * Locale-segment loading UI (Server Component).
 * No Client Component. No fake percentages. Token-based skeleton.
 */
export default async function LocaleLoading() {
  const label = await resolveLocaleLabel();
  return <RouteLoadingSkeleton label={label} />;
}
