/**
 * MODOPS-T015 — Bulk / automated moderation deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./modops-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/MODOPS-T015-bulk-moderation-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("modops-bulk-moderation-deferral-checks: PASS");
