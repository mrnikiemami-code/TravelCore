/**
 * PRODSURF-T015 — Live moderation workflow deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./prodsurf-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/PRODSURF-T015-moderation-workflow-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("prodsurf-moderation-workflow-deferral-checks: PASS");
