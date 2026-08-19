/**
 * LAUNCHOPS-T002 — Explicit locale URL must not be silently overridden.
 */
import assert from "node:assert/strict";
import { readSrc } from "./launchops-common.mjs";

const proxy = readSrc("proxy.ts");
assert.match(proxy, /Not used for locale negotiation/);
assert.doesNotMatch(proxy, /Accept-Language/i);

const layout = readSrc("app/[locale]/layout.tsx");
assert.match(layout, /isAppLocale/);
assert.match(layout, /notFound\(\)/);

console.log("launchops-locale-override-guard-checks: PASS");
