/**
 * Minimal locale registry for App Router routing / document lang+dir.
 *
 * Locale ≠ Currency ≠ Calendar ≠ TimeZone — this module must not infer
 * currency, calendar, or timezone from locale.
 */

export const LOCALES = ["fa", "en", "ar"] as const;

export type AppLocale = (typeof LOCALES)[number];

export type TextDirection = "rtl" | "ltr";

export type LocaleDefinition = {
  /** BCP 47 canonical code used in public URLs and HTML lang. */
  code: AppLocale;
  /** Default document direction for this locale (not a layout-primitive system). */
  direction: TextDirection;
  /** Enabled in the product registry. */
  enabled: boolean;
  /** May appear on public locale-prefixed routes. */
  publicAvailability: boolean;
};

/**
 * Product default for entry/negotiation when URL has no explicit locale.
 * Configuration value — not an eternal schema assumption.
 */
export const DEFAULT_LOCALE: AppLocale = "fa";

export const LOCALE_REGISTRY: Readonly<Record<AppLocale, LocaleDefinition>> = {
  fa: {
    code: "fa",
    direction: "rtl",
    enabled: true,
    publicAvailability: true,
  },
  en: {
    code: "en",
    direction: "ltr",
    enabled: true,
    publicAvailability: true,
  },
  ar: {
    code: "ar",
    direction: "rtl",
    enabled: true,
    publicAvailability: true,
  },
};

export function isAppLocale(value: string): value is AppLocale {
  return (LOCALES as readonly string[]).includes(value);
}

export function getLocaleDefinition(locale: AppLocale): LocaleDefinition {
  return LOCALE_REGISTRY[locale];
}

/** HTML `lang` = BCP 47 code from the URL locale segment. */
export function getHtmlLang(locale: AppLocale): string {
  return getLocaleDefinition(locale).code;
}

/** HTML `dir` derived from locale registry (server-side document direction). */
export function getHtmlDir(locale: AppLocale): TextDirection {
  return getLocaleDefinition(locale).direction;
}

export function listPublicLocales(): AppLocale[] {
  return LOCALES.filter((code) => LOCALE_REGISTRY[code].publicAvailability);
}
