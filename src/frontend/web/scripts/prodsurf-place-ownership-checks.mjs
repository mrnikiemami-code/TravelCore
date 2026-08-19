/**
 * PRODSURF-T011 — Place catalog ownership on hotels route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

const page = read(pagePath(["hotels", "[slug]"]));
assert.match(page, /loadPlaceDetailPage/);
assert.match(page, /P07/);

console.log("prodsurf-place-ownership-checks: PASS");
