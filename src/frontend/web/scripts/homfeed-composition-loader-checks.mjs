/** HOMFEED-T002 — Home composition loader. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const loader = readSrc("features/home-discovery/load-home-discovery-composition.ts");
assert.match(loader, /loadHomeDiscoveryComposition/);
assert.match(loader, /loadTravelogueDiscoveryList/);
assert.match(loader, /loadHotelDiscoveryList/);

console.log("homfeed-composition-loader-checks: PASS");
