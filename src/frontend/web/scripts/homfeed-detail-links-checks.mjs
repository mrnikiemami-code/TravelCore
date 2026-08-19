/** HOMFEED-T007 — Detail links from preview cards. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /travelogueId/);
assert.match(view, /item\.slug/);

console.log("homfeed-detail-links-checks: PASS");
