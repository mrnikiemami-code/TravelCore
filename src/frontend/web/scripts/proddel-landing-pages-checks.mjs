/**
 * PRODDEL-T011 — Programmatic landing pages production route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const page = read(pagePath(["landing-pages", "[slug]"]));
assert.match(page, /ContentDetailView/);
assert.match(page, /"LandingPage"/);
assert.match(page, /loadComposedSeoMetadata/);

console.log("proddel-landing-pages-checks: PASS");
