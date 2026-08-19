/**
 * DISCLINK-T003 — UGC composition links to travelogue detail.
 */
import assert from "node:assert/strict";
import { readSrc } from "./disclink-common.mjs";

const ugc = readSrc("features/public-experience/ugc-composition-list.tsx");
assert.match(ugc, /\/travelogues\//);
assert.match(ugc, /travelogueId/);

console.log("disclink-ugc-travelogue-links-checks: PASS");
