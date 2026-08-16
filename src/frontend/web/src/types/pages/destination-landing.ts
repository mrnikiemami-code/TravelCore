import type { AppLocale } from "@/lib/i18n";
import type { PageViewModel } from "@/lib/api/read-models";

/**
 * DestinationLandingPage — page presentation model (TC-P04-T009).
 * Domain Model ≠ API Contract ≠ Page View Model
 * Destination remains authority; this is composition only.
 */

export type DestinationPublicCrumbView = {
  /** Localized display name for the requested locale. */
  name: string;
  /** Locale-owned slug when available for public linking; otherwise null (no UUID link). */
  slug: string | null;
  kind: string;
  code: string;
};

export type DestinationPublicChildView = {
  name: string;
  slug: string | null;
  kind: string;
  code: string;
};

export type DestinationLandingFields = {
  locale: AppLocale;
  kind: string;
  code: string;
  /** Localized name for the requested locale (required for public page). */
  name: string;
  description: string | null;
  slug: string;
  englishName: string;
  isoCountryCode: string | null;
  latitude: number | null;
  longitude: number | null;
  breadcrumb: DestinationPublicCrumbView[];
  children: DestinationPublicChildView[];
};

export type DestinationLandingPageViewModel =
  PageViewModel<DestinationLandingFields>;
