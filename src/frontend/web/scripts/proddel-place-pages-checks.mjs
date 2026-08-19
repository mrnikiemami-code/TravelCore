/**
 * PRODDEL-T007 — Place pages production route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const page = read(pagePath(["places", "[slug]"]));
assert.match(page, /PlaceDetailView/);
assert.match(page, /loadPlaceDetailPage/);
assert.match(page, /loadComposedSeoMetadata/);

console.log("proddel-place-pages-checks: PASS");
