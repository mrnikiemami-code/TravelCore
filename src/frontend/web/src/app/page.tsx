import { headers } from "next/headers";
import { redirect } from "next/navigation";
import { negotiateEntryLocale } from "@/lib/i18n";

/**
 * Root `/` is entry only. Canonical public content remains locale-prefixed.
 * Accept-Language applies here only — never overrides explicit `/fa|en|ar/` URLs (ADR 0007).
 */
export default async function RootPage() {
  const headerStore = await headers();
  const locale = negotiateEntryLocale(headerStore.get("accept-language"));
  redirect(`/${locale}`);
}
