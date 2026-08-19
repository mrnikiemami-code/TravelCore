/**
 * SEOVAL-T012 — Place page metadata integration checks.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./seoval-common.mjs";

const placeDetail = read(pagePath(["places", "[slug]"]));
assert.match(placeDetail, /generateMetadata/);
assert.match(placeDetail, /loadComposedSeoMetadata/);
assert.match(placeDetail, /loadPlaceDetailPage/);
assert.match(placeDetail, /places\/\$\{slug\}/);
assert.match(placeDetail, /loadSeoBreadcrumbJsonLd/);

console.log("seoval-place-pages-checks: PASS");
console.log("  place detail SEO metadata + JSON-LD: ok");
