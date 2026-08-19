/**
 * DISCLINK-T004 — Hotel book CTA uses hotels path.
 */
import assert from "node:assert/strict";
import { readSrc } from "./disclink-common.mjs";

const view = readSrc("features/place-detail/place-detail-view.tsx");
assert.match(view, /\/hotels\//);
assert.match(view, /\/book/);
assert.doesNotMatch(view, /places\/\$\{encodeURIComponent\(vm\.slug\)\}\/book/);

console.log("disclink-hotel-book-path-checks: PASS");
