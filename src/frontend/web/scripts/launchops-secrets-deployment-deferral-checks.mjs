/**
 * LAUNCHOPS-T011 — Secrets / deployment vendor deferred guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./launchops-common.mjs";

const p29 = fs.readFileSync(
  path.join(repoRoot, "docs/plans/P29-implementation-plan.md"),
  "utf8",
);
assert.match(p29, /deployment/i);

const evidence = path.join(
  repoRoot,
  "docs/plans/LAUNCHOPS-T011-secrets-deployment-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("launchops-secrets-deployment-deferral-checks: PASS");
