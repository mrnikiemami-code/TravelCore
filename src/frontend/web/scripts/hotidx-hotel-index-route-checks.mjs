/** HOTIDX-T005 — /hotels discovery index route. */
import assert from "node:assert/strict";
import { pagePath, read } from "./hotidx-common.mjs";

const page = read(pagePath(["hotels"]));
assert.match(page, /HotelDiscoveryView/);
assert.match(page, /loadHotelDiscoveryList/);

console.log("hotidx-hotel-index-route-checks: PASS");
