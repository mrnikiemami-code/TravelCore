/**
 * PRODDEL-T001 — Production home uses HomeDiscoveryView.
 */
import assert from "node:assert/strict";
import { pagePath, read, readSrc } from "./proddel-common.mjs";

const home = read(pagePath([]));
assert.match(home, /HomeDiscoveryView/);
assert.match(home, /PublicShell/);
assert.doesNotMatch(home, /SAMPLE_USD|Bidi isolation smoke|Money presentation smoke/);

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.doesNotMatch(view.slice(0, 200), /['"]use client['"]/);

console.log("proddel-home-discovery-checks: PASS");
