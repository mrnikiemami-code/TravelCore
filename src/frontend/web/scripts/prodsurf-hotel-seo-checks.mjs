/**
 * PRODSURF-T005 — Hotel SEO metadata on hotels route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

const page = read(pagePath(["hotels", "[slug]"]));
assert.match(page, /generateMetadata/);
assert.match(page, /loadComposedSeoMetadata/);
assert.match(page, /hotels\/\$\{slug\}/);

console.log("prodsurf-hotel-seo-checks: PASS");
