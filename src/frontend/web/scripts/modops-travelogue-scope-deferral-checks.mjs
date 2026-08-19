/**
 * MODOPS-T013 — Review / photo / comment queue deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./modops-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/MODOPS-T013-travelogue-scope-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("modops-travelogue-scope-deferral-checks: PASS");
