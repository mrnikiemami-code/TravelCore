/**
 * PRODSURF-T014 — Real payment/flight/hotel providers deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, webRoot } from "./prodsurf-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/PRODSURF-T014-real-providers-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

const plan = fs.readFileSync(
  path.join(repoRoot, "docs/plans/PRODSURF-implementation-plan.md"),
  "utf8",
);
assert.match(plan, /Real payment/i);

const pkg = JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8"));
const deps = JSON.stringify({ ...pkg.dependencies, ...pkg.devDependencies });
assert.doesNotMatch(deps, /stripe|adyen|amadeus|sabre/i);

console.log("prodsurf-real-providers-deferral-checks: PASS");
