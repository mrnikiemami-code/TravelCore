import type { AppLocale } from "@/lib/i18n";
import { HOME_DESTINATION_SLUG_CANDIDATES } from "@/features/home-discovery/types";

export type TourDestinationOption = {
  slug: string;
  label: string;
};

/**
 * Human-friendly destination choices for Tour listing (P36-T004).
 * Slugs remain API contract values; labels are traveler-facing only.
 */
export function tourDestinationOptions(
  locale: AppLocale,
): TourDestinationOption[] {
  return HOME_DESTINATION_SLUG_CANDIDATES.map((slug) => ({
    slug,
    label: labelForDestinationSlug(locale, slug),
  }));
}

export function labelForDestinationSlug(
  locale: AppLocale,
  slug: string,
): string {
  const key = slug.replace(/^demofeed-/, "").toLowerCase();
  if (locale === "fa") {
    switch (key) {
      case "istanbul":
        return "استانبول";
      case "tehran":
        return "تهران";
      case "turkey":
        return "ترکیه";
      case "iran":
        return "ایران";
      default:
        return key.replace(/-/g, " ");
    }
  }
  if (locale === "ar") {
    switch (key) {
      case "istanbul":
        return "إسطنبول";
      case "tehran":
        return "طهران";
      case "turkey":
        return "تركيا";
      case "iran":
        return "إيران";
      default:
        return key.replace(/-/g, " ");
    }
  }
  switch (key) {
    case "istanbul":
      return "Istanbul";
    case "tehran":
      return "Tehran";
    case "turkey":
      return "Turkey";
    case "iran":
      return "Iran";
    default:
      return key.replace(/-/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());
  }
}

export function looksLikeTechnicalId(value: string): boolean {
  const v = value.trim();
  if (!v) return true;
  if (/^undefined$/i.test(v) || /^null$/i.test(v)) return true;
  // UUID v4-ish or long hex ids
  if (
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(v)
  ) {
    return true;
  }
  if (/^[0-9a-f]{24,}$/i.test(v)) return true;
  return false;
}
