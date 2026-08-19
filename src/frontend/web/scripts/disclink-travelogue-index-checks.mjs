/**
 * DISCLINK-T001 — Travelogue discovery index route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./disclink-common.mjs";

const page = read(pagePath(["travelogues"]));
assert.match(page, /TravelogueDiscoveryView/);
assert.match(page, /loadTravelogueDiscoveryList/);

console.log("disclink-travelogue-index-checks: PASS");
