/** HOTIDX-T008 — Hotel discovery view component. */
import assert from "node:assert/strict";
import { readSrc } from "./hotidx-common.mjs";

const view = readSrc("features/hotel-discovery/hotel-discovery-view.tsx");
assert.match(view, /HotelDiscoveryView/);
assert.match(view, /not a search engine/);

console.log("hotidx-hotel-discovery-view-checks: PASS");
