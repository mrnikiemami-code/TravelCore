/**
 * PRODDEL-T015 — Production deployment / E2E deferral checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, webRoot } from "./proddel-common.mjs";

const plan = fs.readFileSync(
  path.join(repoRoot, "docs/plans/PRODDEL-implementation-plan.md"),
  "utf8",
);
assert.match(plan, /defer/i);

const evidence = path.join(
  repoRoot,
  "docs/plans/PRODDEL-T015-production-deployment-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

const pkg = JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8"));
const depBlob = JSON.stringify({
  dependencies: pkg.dependencies ?? {},
  devDependencies: pkg.devDependencies ?? {},
});
assert.doesNotMatch(depBlob, /playwright|cypress|puppeteer/i);

console.log("proddel-production-deployment-deferral-checks: PASS");
