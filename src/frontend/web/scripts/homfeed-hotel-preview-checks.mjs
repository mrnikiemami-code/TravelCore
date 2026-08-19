/** HOMFEED-T004 — Hotel preview section on home. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /hotels\.map/);
assert.match(view, /\/hotels\//);

console.log("homfeed-hotel-preview-checks: PASS");
