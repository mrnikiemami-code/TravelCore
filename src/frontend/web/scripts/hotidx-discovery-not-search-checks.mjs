/** HOTIDX-T012 — Discovery-not-search guard. */
import assert from "node:assert/strict";
import { pagePath, read, readSrc } from "./hotidx-common.mjs";

const page = read(pagePath(["hotels"]));
assert.match(page, /not Search engine/);
const view = readSrc("features/hotel-discovery/hotel-discovery-view.tsx");
assert.match(view, /not a search engine/);

console.log("hotidx-discovery-not-search-checks: PASS");
