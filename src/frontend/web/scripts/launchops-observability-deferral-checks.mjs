/**
 * LAUNCHOPS-T010 — Observability vendor products deferred guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, webRoot } from "./launchops-common.mjs";

const plan = fs.readFileSync(
  path.join(repoRoot, "docs/plans/LAUNCHOPS-implementation-plan.md"),
  "utf8",
);
assert.match(plan, /defer/i);

const pkg = JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8"));
const deps = JSON.stringify({ ...pkg.dependencies, ...pkg.devDependencies });
assert.doesNotMatch(deps, /datadog|newrelic|sentry/i);

console.log("launchops-observability-deferral-checks: PASS");
