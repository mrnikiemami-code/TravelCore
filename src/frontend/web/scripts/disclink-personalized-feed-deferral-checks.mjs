/**
 * DISCLINK-T015 — Personalized feed deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, readSrc } from "./disclink-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/DISCLINK-T015-personalized-feed-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

const home = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(home, /not a personalized feed/i);

console.log("disclink-personalized-feed-deferral-checks: PASS");
