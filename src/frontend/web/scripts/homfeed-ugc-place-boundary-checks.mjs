/** HOMFEED-T013 — UGC vs Place boundary on home sections. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const loader = readSrc("features/home-discovery/load-home-discovery-composition.ts");
assert.match(loader, /loadTravelogueDiscoveryList/);
assert.match(loader, /loadHotelDiscoveryList/);
assert.doesNotMatch(loader, /ContentItem/);

console.log("homfeed-ugc-place-boundary-checks: PASS");
