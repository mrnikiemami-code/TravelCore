/** HOMFEED-T003 — Travelogue preview section on home. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /travelogues\.map/);
assert.match(view, /\/travelogues\//);

console.log("homfeed-travelogue-preview-checks: PASS");
