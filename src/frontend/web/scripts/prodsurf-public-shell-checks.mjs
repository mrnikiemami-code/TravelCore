/**
 * PRODSURF-T009 — Public shell on new production routes.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./prodsurf-common.mjs";

for (const segments of [
  ["travelogues", "[travelogueId]"],
  ["hotels", "[slug]"],
  ["hotels", "[slug]", "book"],
]) {
  const page = read(pagePath(segments));
  assert.match(page, /PublicShell/);
}

console.log("prodsurf-public-shell-checks: PASS");
