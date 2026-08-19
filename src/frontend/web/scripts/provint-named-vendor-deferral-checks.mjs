/**
 * PROVINT-T014 — Named vendor selection deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./provint-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/PROVINT-T014-named-vendor-selection-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

const plan = fs.readFileSync(
  path.join(repoRoot, "docs/plans/PROVINT-implementation-plan.md"),
  "utf8",
);
assert.match(plan, /Named vendor selection/i);

console.log("provint-named-vendor-deferral-checks: PASS");
