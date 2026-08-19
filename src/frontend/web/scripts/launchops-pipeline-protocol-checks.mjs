/**
 * LAUNCHOPS-T008 — Pipeline protocol readiness.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./launchops-common.mjs";

assert.ok(
  fs.existsSync(path.join(repoRoot, "docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md")),
);
assert.ok(fs.existsSync(path.join(repoRoot, "docs/ai/pipeline-runtime-policy.json")));

const policy = JSON.parse(
  fs.readFileSync(path.join(repoRoot, "docs/ai/pipeline-runtime-policy.json"), "utf8"),
);
assert.equal(policy.autoContinueAfterTaskAccept, true);

console.log("launchops-pipeline-protocol-checks: PASS");
