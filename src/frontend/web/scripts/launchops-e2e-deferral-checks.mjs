/**
 * LAUNCHOPS-T014 — E2E / live crawl deferral evidence.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, webRoot } from "./launchops-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/LAUNCHOPS-T014-e2e-live-crawl-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

const pkg = JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8"));
const deps = JSON.stringify({ ...pkg.dependencies, ...pkg.devDependencies });
assert.doesNotMatch(deps, /playwright|cypress|puppeteer/i);

console.log("launchops-e2e-deferral-checks: PASS");
