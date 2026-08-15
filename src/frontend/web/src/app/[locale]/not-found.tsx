import { headers } from "next/headers";
import { NotFoundView } from "@/components/ui/not-found-view";
import { localeFromPathname } from "@/lib/i18n/ui-labels";

/**
 * Locale-segment not-found (Server Component).
 * Rendered inside [locale]/layout so html lang/dir stay correct for valid locales.
 */
export default async function LocaleNotFound() {
  const h = await headers();
  const locale = localeFromPathname(h.get("x-pathname"));
  return <NotFoundView locale={locale} />;
}
