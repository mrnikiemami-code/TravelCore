/**
 * LAUNCHOPS-T004 — Search Console ops runbook evidence.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./launchops-common.mjs";

const runbook = path.join(
  repoRoot,
  "docs/plans/LAUNCHOPS-T004-search-console-ops-runbook.md",
);
assert.ok(fs.existsSync(runbook));
assert.match(fs.readFileSync(runbook, "utf8"), /Search Console/i);

console.log("launchops-search-console-runbook-checks: PASS");
