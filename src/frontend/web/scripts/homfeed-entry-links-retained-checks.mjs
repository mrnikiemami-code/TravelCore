/** HOMFEED-T008 — Discovery entry links retained. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /productionLinks/);
assert.match(view, /\/tours/);
assert.match(view, /\/plan/);

console.log("homfeed-entry-links-retained-checks: PASS");
