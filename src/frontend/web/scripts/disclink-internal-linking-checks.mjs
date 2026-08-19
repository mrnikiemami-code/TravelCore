/**
 * DISCLINK-T013 — SEO internal linking loaders preserved.
 */
import assert from "node:assert/strict";
import { readSrc } from "./disclink-common.mjs";

assert.match(readSrc("features/public-experience/load-related-tours.ts"), /related-published/);
assert.match(readSrc("features/public-experience/load-related-content.ts"), /related-published/);

console.log("disclink-internal-linking-checks: PASS");
