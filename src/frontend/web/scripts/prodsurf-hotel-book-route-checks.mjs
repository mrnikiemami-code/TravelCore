/**
 * PRODSURF-T006 — Production hotel booking prepare route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

const page = read(pagePath(["hotels", "[slug]", "book"]));
assert.match(page, /PublicHotelBookingPrepareForm/);
assert.match(page, /loadPlaceDetailPage/);
assert.match(page, /kind !== "Hotel"/);

console.log("prodsurf-hotel-book-route-checks: PASS");
