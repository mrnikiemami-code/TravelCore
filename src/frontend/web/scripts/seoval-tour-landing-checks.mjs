/**
 * SEOVAL-T011 — Tour landing page metadata integration checks.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./seoval-common.mjs";

const tourDetail = read(pagePath(["tours", "[slug]"]));
assert.match(tourDetail, /generateMetadata/);
assert.match(tourDetail, /loadComposedSeoMetadata/);
assert.match(tourDetail, /loadTourDetailPage/);
assert.match(tourDetail, /tours\/\$\{slug\}/);
assert.match(tourDetail, /Published ≠ Index/);

const tourListing = read(pagePath(["tours"]));
assert.match(tourListing, /generateMetadata/);

console.log("seoval-tour-landing-checks: PASS");
console.log("  tour detail SEO metadata: ok");
console.log("  tour listing metadata: ok");
