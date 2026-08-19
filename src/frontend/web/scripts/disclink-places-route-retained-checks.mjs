/**
 * DISCLINK-T011 — Places production route retained.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./disclink-common.mjs";

const page = read(pagePath(["places", "[slug]"]));
assert.match(page, /PlaceDetailView/);
assert.match(page, /loadPlaceDetailPage/);

console.log("disclink-places-route-retained-checks: PASS");
