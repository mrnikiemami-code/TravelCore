/**
 * MODOPS-T009 — Admin moderation page remains noindex.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./modops-common.mjs";

const page = read(pagePath(["admin", "ugc", "moderation"]));
assert.match(page, /robots:\s*\{\s*index:\s*false/);

console.log("modops-admin-noindex-checks: PASS");
