/**
 * DISCLINK-T012 — UGC travelogue links are not Content Article routes.
 */
import assert from "node:assert/strict";
import { readSrc } from "./disclink-common.mjs";

const ugc = readSrc("features/public-experience/ugc-composition-list.tsx");
assert.match(ugc, /not an editorial Article/i);
assert.doesNotMatch(ugc, /\/articles\//);

console.log("disclink-ugc-boundary-checks: PASS");
