/** HOMFEED-T010 — Not-user-personalized guard. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /not a personalized feed/i);
assert.doesNotMatch(view, /cookie/i);
assert.doesNotMatch(view, /recommendation engine/i);

console.log("homfeed-not-personalized-checks: PASS");
