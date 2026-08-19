/**
 * PRODSURF-T007 — Transactional noindex on hotels book route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

const page = read(pagePath(["hotels", "[slug]", "book"]));
assert.match(page, /index:\s*false/);

console.log("prodsurf-transactional-noindex-checks: PASS");
