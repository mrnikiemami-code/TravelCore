/**
 * LAUNCHOPS-T005 — Production deployment checklist evidence.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./launchops-common.mjs";

const checklist = path.join(
  repoRoot,
  "docs/plans/LAUNCHOPS-T005-production-deployment-checklist.md",
);
assert.ok(fs.existsSync(checklist));
assert.match(fs.readFileSync(checklist, "utf8"), /deployment/i);

console.log("launchops-deployment-checklist-checks: PASS");
