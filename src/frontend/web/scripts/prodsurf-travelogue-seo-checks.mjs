/**
 * PRODSURF-T003 — Travelogue SEO metadata compose.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

const page = read(pagePath(["travelogues", "[travelogueId]"]));
assert.match(page, /generateMetadata/);
assert.match(page, /loadComposedSeoMetadata/);
assert.match(page, /travelogues\/\$\{travelogue\.travelogueId\}/);

console.log("prodsurf-travelogue-seo-checks: PASS");
