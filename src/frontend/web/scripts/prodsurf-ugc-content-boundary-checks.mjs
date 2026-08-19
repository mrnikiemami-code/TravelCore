/**
 * PRODSURF-T010 — UGC travelogue is not Content Article.
 */
import assert from "node:assert/strict";
import { readSrc } from "./prodsurf-common.mjs";

const view = readSrc("features/travelogue-detail/travelogue-detail-view.tsx");
assert.match(view, /not editorial Article/i);
assert.doesNotMatch(view, /ContentDetailView/);

const loader = readSrc("features/travelogue-detail/load-travelogue-detail.ts");
assert.match(loader, /\/api\/ugc\/public\/travelogues\//);
assert.doesNotMatch(loader, /\/api\/content\//);

console.log("prodsurf-ugc-content-boundary-checks: PASS");
