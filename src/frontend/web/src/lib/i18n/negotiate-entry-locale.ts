/**
 * Root `/` entry locale negotiation (ADR 0007 · i18n §6–7).
 *
 * Used ONLY when URL has no explicit locale segment.
 * Public canonical routes remain locale-prefixed; browser preference
 * must never override an explicit `/fa|en|ar/` URL.
 */
import { DEFAULT_LOCALE, listPublicLocales, type AppLocale } from "./config.ts";

function parseAcceptLanguage(header: string): Array<{ tag: string; q: number }> {
  return header
    .split(",")
    .map((part) => {
      const trimmed = part.trim();
      if (!trimmed) return null;
      const [rawTag, ...params] = trimmed.split(";");
      const tag = rawTag.trim().toLowerCase();
      if (!tag) return null;
      const qParam = params.join(";").match(/q=([\d.]+)/i);
      const q = qParam ? Number.parseFloat(qParam[1]!) : 1;
      if (Number.isNaN(q)) return null;
      return { tag, q };
    })
    .filter((entry): entry is { tag: string; q: number } => entry !== null)
    .sort((a, b) => b.q - a.q);
}

function matchPublicLocale(tag: string, publicLocales: readonly AppLocale[]): AppLocale | null {
  if (publicLocales.includes(tag as AppLocale)) {
    return tag as AppLocale;
  }
  const base = tag.split("-")[0];
  if (base && publicLocales.includes(base as AppLocale)) {
    return base as AppLocale;
  }
  return null;
}

/** Map Accept-Language to the best public locale for `/` redirect only. */
export function negotiateEntryLocale(
  acceptLanguage: string | null | undefined,
): AppLocale {
  const publicLocales = listPublicLocales();

  if (!acceptLanguage?.trim()) {
    return DEFAULT_LOCALE;
  }

  for (const { tag } of parseAcceptLanguage(acceptLanguage)) {
    const matched = matchPublicLocale(tag, publicLocales);
    if (matched) {
      return matched;
    }
  }

  return DEFAULT_LOCALE;
}
