/** HOMFEED-T005 — Locale home page wires composition loader. */
import assert from "node:assert/strict";
import { pagePath, read } from "./homfeed-common.mjs";

const page = read(pagePath([]));
assert.match(page, /loadHomeDiscoveryComposition/);
assert.match(page, /composition={composition}/);

console.log("homfeed-home-page-wiring-checks: PASS");
