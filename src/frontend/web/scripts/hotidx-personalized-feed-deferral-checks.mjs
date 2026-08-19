/** HOTIDX-T015 — Personalized hotel feed deferral guard. */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./hotidx-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/HOTIDX-T015-personalized-hotel-feed-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("hotidx-personalized-feed-deferral-checks: PASS");
