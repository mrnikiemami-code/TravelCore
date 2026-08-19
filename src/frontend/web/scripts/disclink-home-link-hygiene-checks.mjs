/**
 * DISCLINK-T006 — Production home link hygiene.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./disclink-common.mjs";

const home = read(pagePath([]));
assert.doesNotMatch(home, /includeDevLinks/);
assert.match(home, /<HomeDiscoveryView locale=\{locale\} \/>/);

console.log("disclink-home-link-hygiene-checks: PASS");
