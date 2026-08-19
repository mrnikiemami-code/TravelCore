/**
 * SEOVAL-T015 — Search Console / production validation deferral checks.
 * ROADMAP item #15 is explicitly deferred to production operations.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, webRoot } from "./seoval-common.mjs";

const roadmap = fs.readFileSync(path.join(repoRoot, "docs/ROADMAP.md"), "utf8");
assert.match(roadmap, /Search Console \/ production validation later/);

const plan = fs.readFileSync(
  path.join(repoRoot, "docs/plans/SEOVAL-implementation-plan.md"),
  "utf8",
);
assert.match(plan, /deferred/i);

const evidence = path.join(
  repoRoot,
  "docs/plans/SEOVAL-T015-search-console-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence), "T015 deferral evidence doc missing");

const evidenceSrc = fs.readFileSync(evidence, "utf8");
assert.match(evidenceSrc, /DEFERRED|deferred/i);
assert.match(evidenceSrc, /production/i);

// Guard: no premature Search Console npm dependency
const pkgJson = JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8"));
const depBlob = JSON.stringify({
  dependencies: pkgJson.dependencies ?? {},
  devDependencies: pkgJson.devDependencies ?? {},
});
assert.doesNotMatch(depBlob, /search-console|googleapis.*searchconsole/i);

console.log("seoval-search-console-checks: PASS");
console.log("  ROADMAP + plan deferral documented: ok");
console.log("  T015 evidence + no premature integration: ok");
