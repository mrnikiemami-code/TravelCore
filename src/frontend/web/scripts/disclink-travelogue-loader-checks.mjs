/**
 * DISCLINK-T008 — Travelogue list loader.
 */
import assert from "node:assert/strict";
import { existsSrc, readSrc } from "./disclink-common.mjs";

assert.ok(existsSrc("features/travelogue-detail/load-travelogue-list.ts"));
const loader = readSrc("features/travelogue-detail/load-travelogue-list.ts");
assert.match(loader, /loadTravelogueDiscoveryList/);
assert.match(loader, /\/api\/ugc\/public\/travelogues/);

console.log("disclink-travelogue-loader-checks: PASS");
