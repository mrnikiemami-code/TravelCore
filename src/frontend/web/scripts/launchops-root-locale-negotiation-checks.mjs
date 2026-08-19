/**
 * LAUNCHOPS-T001 — Root Accept-Language entry negotiation.
 */
import assert from "node:assert/strict";
import { existsSrc, readSrc } from "./launchops-common.mjs";

assert.ok(existsSrc("lib/i18n/negotiate-entry-locale.ts"));

const negotiate = readSrc("lib/i18n/negotiate-entry-locale.ts");
assert.match(negotiate, /negotiateEntryLocale/);
assert.match(negotiate, /DEFAULT_LOCALE/);
assert.match(negotiate, /listPublicLocales/);

const root = readSrc("app/page.tsx");
assert.match(root, /negotiateEntryLocale/);
assert.match(root, /accept-language/i);
assert.match(root, /headers\(/);

console.log("launchops-root-locale-negotiation-checks: PASS");
