/**
 * PRODSURF-T008 — UIVAL dev routes retained (noindex).
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

for (const segments of [
  ["dev", "hotel-detail"],
  ["dev", "travelogue"],
]) {
  const page = read(pagePath(segments));
  assert.match(page, /index:\s*false/);
}

console.log("prodsurf-uival-dev-routes-checks: PASS");
