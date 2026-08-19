/** HOMFEED-T006 — Index see-all links on home. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /seeAllTravelogues/);
assert.match(view, /seeAllHotels/);
assert.match(view, /\/travelogues/);
assert.match(view, /\/hotels/);

console.log("homfeed-index-links-checks: PASS");
