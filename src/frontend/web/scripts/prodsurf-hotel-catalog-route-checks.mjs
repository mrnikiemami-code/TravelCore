/**
 * PRODSURF-T004 — Production hotel catalog route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

const page = read(pagePath(["hotels", "[slug]"]));
assert.match(page, /PlaceDetailView/);
assert.match(page, /loadPlaceDetailPage/);
assert.match(page, /kind !== "Hotel"/);

console.log("prodsurf-hotel-catalog-route-checks: PASS");
