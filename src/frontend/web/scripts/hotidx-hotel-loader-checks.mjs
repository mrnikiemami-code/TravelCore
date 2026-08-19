/** HOTIDX-T007 — Hotel discovery list loader. */
import assert from "node:assert/strict";
import { readSrc } from "./hotidx-common.mjs";

const loader = readSrc("features/hotel-discovery/load-hotel-discovery-list.ts");
assert.match(loader, /\/api\/place\/public\/hotels/);
assert.match(loader, /loadHotelDiscoveryList/);

console.log("hotidx-hotel-loader-checks: PASS");
