/**
 * PRODSURF-T002 — Production travelogue detail route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

const page = read(pagePath(["travelogues", "[travelogueId]"]));
assert.match(page, /TravelogueDetailView/);
assert.match(page, /loadTravelogueDetailPage/);
assert.doesNotMatch(page, /loadTravelogueDetailFixture/);

console.log("prodsurf-travelogue-route-checks: PASS");
