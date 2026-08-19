/**
 * LAUNCHOPS-T015 — Live GSC verification deferral evidence.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, webRoot } from "./launchops-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/LAUNCHOPS-T015-live-gsc-verification-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

const deps = JSON.stringify({
  dependencies: JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8")).dependencies ?? {},
  devDependencies: JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8")).devDependencies ?? {},
});
assert.doesNotMatch(deps, /googleapis.*searchconsole|@googleapis\/searchconsole/i);

console.log("launchops-live-gsc-deferral-checks: PASS");
