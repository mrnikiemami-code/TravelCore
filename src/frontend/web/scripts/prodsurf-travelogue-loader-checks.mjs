/**
 * PRODSURF-T013 — load-travelogue-detail loader.
 */
import assert from "node:assert/strict";
import { existsSrc, readSrc } from "./prodsurf-common.mjs";

assert.ok(existsSrc("features/travelogue-detail/load-travelogue-detail.ts"));

const loader = readSrc("features/travelogue-detail/load-travelogue-detail.ts");
assert.match(loader, /loadTravelogueDetailPage/);
assert.match(loader, /apiGetJson/);

console.log("prodsurf-travelogue-loader-checks: PASS");
