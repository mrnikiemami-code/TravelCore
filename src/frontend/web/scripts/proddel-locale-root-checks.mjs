/**
 * PRODDEL-T013 — Locale root constitution.
 */
import assert from "node:assert/strict";
import { readSrc } from "./proddel-common.mjs";

const root = readSrc("app/page.tsx");
assert.match(root, /negotiateEntryLocale/);
assert.match(root, /redirect\(`\/\$\{/);

const layout = readSrc("app/[locale]/layout.tsx");
assert.match(layout, /isAppLocale/);
assert.match(layout, /getHtmlLang/);
assert.match(layout, /getHtmlDir/);

console.log("proddel-locale-root-checks: PASS");
