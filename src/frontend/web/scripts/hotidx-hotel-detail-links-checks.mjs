/** HOTIDX-T009 — Index links to hotel detail routes. */
import assert from "node:assert/strict";
import { readSrc } from "./hotidx-common.mjs";

const view = readSrc("features/hotel-discovery/hotel-discovery-view.tsx");
assert.match(view, /\/hotels\/\$\{encodeURIComponent\(item\.slug\)\}/);

console.log("hotidx-hotel-detail-links-checks: PASS");
