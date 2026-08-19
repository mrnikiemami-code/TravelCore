/** HOMFEED-T011 — Not-search-engine guard. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /not a search engine/i);
const loader = readSrc("features/home-discovery/load-home-discovery-composition.ts");
assert.match(loader, /not Search/);

console.log("homfeed-not-search-engine-checks: PASS");
