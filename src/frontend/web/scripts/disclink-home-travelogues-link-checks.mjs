/**
 * DISCLINK-T005 — Home discovery travelogues entry link.
 */
import assert from "node:assert/strict";
import { readSrc } from "./disclink-common.mjs";

const home = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(home, /\/travelogues/);

console.log("disclink-home-travelogues-link-checks: PASS");
