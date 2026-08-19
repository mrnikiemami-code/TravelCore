/** HOMFEED-T001 — Home composition types + preview limits. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const types = readSrc("features/home-discovery/types.ts");
assert.match(types, /HomeDiscoveryComposition/);
assert.match(types, /HOME_DISCOVERY_PREVIEW_LIMIT/);

console.log("homfeed-composition-types-checks: PASS");
