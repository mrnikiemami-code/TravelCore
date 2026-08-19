/**
 * DISCLINK-T009 — Discovery index is not a search engine.
 */
import assert from "node:assert/strict";
import { readSrc } from "./disclink-common.mjs";

const view = readSrc("features/travelogue-detail/travelogue-discovery-view.tsx");
assert.match(view, /not a search engine/i);
assert.doesNotMatch(view, /SearchEngine|ranking/i);

console.log("disclink-discovery-not-search-checks: PASS");
