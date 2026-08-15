import { redirect } from "next/navigation";
import { DEFAULT_LOCALE } from "@/lib/i18n";

/**
 * Root `/` is entry only. Canonical public content remains locale-prefixed.
 * Negotiation details (Accept-Language, stored preference) → later SEO/i18n work.
 * Smallest safe behavior: product default locale (`fa`).
 */
export default function RootPage() {
  redirect(`/${DEFAULT_LOCALE}`);
}
