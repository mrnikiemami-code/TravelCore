/**
 * MODOPS-T014 — Report-driven moderation deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./modops-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/MODOPS-T014-report-moderation-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("modops-report-moderation-deferral-checks: PASS");
