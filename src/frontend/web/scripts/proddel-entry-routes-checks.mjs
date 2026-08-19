/**
 * PRODDEL-T010 — Planner / flights / visa production entry routes.
 */
import assert from "node:assert/strict";
import { pagePath, read, readSrc } from "./proddel-common.mjs";

for (const segments of [["plan"], ["flights"], ["visas", "[code]"]]) {
  assert.ok(read(pagePath(segments)).length > 0, `route exists: ${segments.join("/")}`);
}

const homeView = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(homeView, /\/plan/);
assert.match(homeView, /\/flights/);
assert.match(homeView, /\/visas/);

console.log("proddel-entry-routes-checks: PASS");
