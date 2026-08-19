/**
 * PRODSURF-T012 — TravelogueDetailView reused on production route.
 */
import assert from "node:assert/strict";
import { pagePath, read, readSrc } from "./prodsurf-common.mjs";

const page = read(pagePath(["travelogues", "[travelogueId]"]));
assert.match(page, /TravelogueDetailView/);

const view = readSrc("features/travelogue-detail/travelogue-detail-view.tsx");
assert.doesNotMatch(view, /['"]use client['"]/);

console.log("prodsurf-travelogue-view-checks: PASS");
