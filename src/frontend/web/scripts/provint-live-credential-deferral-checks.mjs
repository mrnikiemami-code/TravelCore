/**
 * PROVINT-T015 — Live credential wiring deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./provint-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/PROVINT-T015-live-credential-wiring-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("provint-live-credential-deferral-checks: PASS");
