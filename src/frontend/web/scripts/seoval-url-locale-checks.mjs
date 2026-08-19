/**
 * SEOVAL-T001 — URL/locale constitution validation checks.
 */
import assert from "node:assert/strict";
import {
  existsSrc,
  readSrc,
} from "./seoval-common.mjs";

const i18nConfig = readSrc("lib/i18n/config.ts");
assert.match(i18nConfig, /export const LOCALES = \["fa", "en", "ar"\]/);
assert.match(i18nConfig, /LOCALE_REGISTRY/);
assert.match(i18nConfig, /DEFAULT_LOCALE.*"fa"/);
assert.match(i18nConfig, /listPublicLocales/);

const layout = readSrc("app/[locale]/layout.tsx");
assert.match(layout, /generateStaticParams/);
assert.match(layout, /listPublicLocales/);
assert.match(layout, /getHtmlLang/);
assert.match(layout, /getHtmlDir/);
assert.match(layout, /isAppLocale/);
assert.match(layout, /dynamicParams\s*=\s*false/);

const rootPage = readSrc("app/page.tsx");
assert.match(rootPage, /redirect\(`\/\$\{DEFAULT_LOCALE\}`\)/);
assert.match(rootPage, /Accept-Language/);

const proxy = readSrc("proxy.ts");
assert.match(proxy, /Not used for locale negotiation/);
assert.doesNotMatch(proxy, /Accept-Language/);

assert.ok(existsSrc("app/[locale]/destinations/[slug]/page.tsx"), "locale-prefixed destination route");

console.log("seoval-url-locale-checks: PASS");
console.log("  locale registry (fa/en/ar): ok");
console.log("  [locale] layout constitution: ok");
console.log("  root redirect + proxy (no silent negotiation): ok");
